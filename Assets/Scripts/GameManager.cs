using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<int, int> CoreCountChanged;
        public event Action<int, string> CoreFeedback;
        public event Action<int, int> RelicCountChanged;
        public event Action ExitUnlocked;
        public event Action EscapeStarted;
        public event Action<RunPhase> RunPhaseChanged;
        public event Action<string> PlayerReset;
        public event Action Victory;

        public RuinGameState GameState { get; } = new RuinGameState();
        public int RequiredCoreCount => RuinGameState.RequiredCoreCount;
        public bool HasWon { get; private set; }
        public RunPhase CurrentRunPhase => runFlow.Phase;
        public ScoreResult LastScoreResult { get; private set; }
        public RunStats LastRunStats { get; private set; }
        public Transform PlayerTransform => player;
        public string ObjectiveStage => saveData == null ? EchoesOfTheRuins.ObjectiveStage.Briefing.ToString() : saveData.ObjectiveStage;

        [SerializeField] private Transform player;
        [SerializeField] private Transform initialCheckpoint;
        private Transform currentCheckpoint;
        private bool exitWasUnlocked;
        private SaveService saveService;
        private SaveData saveData;
        private readonly RunStats runStats = new RunStats();
        private readonly RunMetricsTracker runMetrics = new RunMetricsTracker();
        private readonly RunFlowState runFlow = new RunFlowState();
        private readonly RelicProgressState relicProgress = new RelicProgressState();
        private float runStartedAt;

        public void Configure(Transform playerTransform, Transform startingCheckpoint)
        {
            player = playerTransform;
            initialCheckpoint = startingCheckpoint;
            currentCheckpoint = startingCheckpoint;
            PlayerController controller = player != null ? player.GetComponent<PlayerController>() : null;
            if (controller != null) controller.EchoStoneUsed += runMetrics.RecordEchoStone;
        }

        public void TrackGuardian(GuardianAI guardian)
        {
            if (guardian == null) return;
            string guardianId = guardian.gameObject.name;
            guardian.AlertStateChanged += (state, _) => runMetrics.RecordGuardianState(guardianId, state);
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            runFlow.Changed += OnRunPhaseChanged;
            saveService = new SaveService();
            saveData = saveService.Load();
            GameState.RestoreCores(saveData.CollectedCoreIds);
            relicProgress.Restore(saveData.CollectedRelicIds);
            if (GameState.IsExitUnlocked) runFlow.UpdateCoreProgress(RequiredCoreCount, RequiredCoreCount);
            else if (GameState.CollectedCoreCount > 0) runFlow.BeginCoreHunt();
            runStartedAt = Time.time;
            currentCheckpoint = initialCheckpoint;
        }

        private void Start()
        {
            if (!string.IsNullOrWhiteSpace(saveData?.CheckpointId) && saveData.CheckpointId != SaveData.StartCheckpointId)
            {
                GameObject savedCheckpoint = GameObject.Find(saveData.CheckpointId);
                if (savedCheckpoint != null) currentCheckpoint = savedCheckpoint.transform;
            }

            if (GameState.IsExitUnlocked)
            {
                exitWasUnlocked = true;
                ExitUnlocked?.Invoke();
            }
        }

        public bool CollectCore(string coreId)
        {
            if (!GameState.CollectCore(coreId)) return false;

            CoreCountChanged?.Invoke(GameState.CollectedCoreCount, RequiredCoreCount);
            string zone = ResolveCoreZone(coreId);
            CoreFeedback?.Invoke(GameState.CollectedCoreCount, zone);
            saveData.CollectedCoreIds = new List<string>();
            foreach (CorePlacement core in ProductionSceneLayout.CreateDefault().Cores)
                if (GameState.HasCollectedCore(core.Id)) saveData.CollectedCoreIds.Add(core.Id);
            saveService.Save(saveData);
            runFlow.UpdateCoreProgress(GameState.CollectedCoreCount, RequiredCoreCount);
            if (GameState.IsExitUnlocked && !exitWasUnlocked)
            {
                exitWasUnlocked = true;
                ExitUnlocked?.Invoke();
            }
            return true;
        }

        public void BeginCoreHunt() => runFlow.BeginCoreHunt();

        public bool CollectRelic(string relicId)
        {
            if (!relicProgress.Collect(relicId)) return false;
            saveData.CollectedRelicIds = relicProgress.ToList();
            runStats.RelicsCollected = relicProgress.Count;
            saveService.Save(saveData);
            RelicCountChanged?.Invoke(relicProgress.Count, 2);
            return true;
        }

        public bool HasRelic(string relicId) => relicProgress.Contains(relicId);

        public void SetCheckpoint(Transform checkpoint)
        {
            if (checkpoint == null) return;
            currentCheckpoint = checkpoint;
            saveData.CheckpointId = checkpoint.name;
            saveService.Save(saveData);
        }

        public void ResetPlayerToCheckpoint(string reason = "Caught by guardian")
        {
            runMetrics.RecordCapture();
            if (player != null && currentCheckpoint != null)
            {
                var controller = player.GetComponent<CharacterController>();
                if (controller != null) controller.enabled = false;
                player.SetPositionAndRotation(currentCheckpoint.position, currentCheckpoint.rotation);
                if (controller != null) controller.enabled = true;
            }
            PlayerReset?.Invoke(reason);
        }

        public void CompleteEscape()
        {
            if (HasWon || !GameState.IsExitUnlocked) return;
            runFlow.UpdateCoreProgress(GameState.CollectedCoreCount, RequiredCoreCount);
            if (!runFlow.TryCompleteEscape()) return;
            HasWon = true;
            PlayerController controller = player == null ? null : player.GetComponent<PlayerController>();
            controller?.SetInputLocked(true);
            runStats.CompletionSeconds = Time.time - runStartedAt;
            runMetrics.ApplyTo(runStats);
            runStats.RelicsCollected = saveData.CollectedRelicIds == null ? 0 : saveData.CollectedRelicIds.Count;
            LastRunStats = runStats;
            LastScoreResult = RunHistoryService.RecordCompletedRun(saveData, runStats, DateTime.UtcNow.ToString("O"));
            saveData.ObjectiveStage = EchoesOfTheRuins.ObjectiveStage.Complete.ToString();
            saveService.Save(saveData);
            Victory?.Invoke();
        }

        private void OnRunPhaseChanged(RunPhase phase)
        {
            RunPhaseChanged?.Invoke(phase);
            if (phase == RunPhase.Escape) EscapeStarted?.Invoke();
        }

        private static string ResolveCoreZone(string coreId)
        {
            foreach (CorePlacement placement in ProductionSceneLayout.CreateDefault().Cores)
                if (placement.Id == coreId) return placement.Zone.ToString();
            return "UNKNOWN";
        }

        private void OnDestroy()
        {
            runFlow.Changed -= OnRunPhaseChanged;
            if (Instance == this) Instance = null;
        }

        public void SetObjectiveStage(ObjectiveStage stage)
        {
            if (saveData == null) return;
            string value = stage.ToString();
            if (saveData.ObjectiveStage == value) return;
            saveData.ObjectiveStage = value;
            saveService.Save(saveData);
        }
    }
}
