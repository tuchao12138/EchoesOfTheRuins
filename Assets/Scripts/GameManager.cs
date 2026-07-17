using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public event Action<int, int> CoreCountChanged;
        public event Action ExitUnlocked;
        public event Action<string> PlayerReset;
        public event Action Victory;

        public RuinGameState GameState { get; } = new RuinGameState();
        public int RequiredCoreCount => RuinGameState.RequiredCoreCount;
        public bool HasWon { get; private set; }
        public Transform PlayerTransform => player;

        [SerializeField] private Transform player;
        [SerializeField] private Transform initialCheckpoint;
        private Transform currentCheckpoint;
        private bool exitWasUnlocked;
        private SaveService saveService;
        private SaveData saveData;
        private readonly RunStats runStats = new RunStats();
        private float runStartedAt;

        public void Configure(Transform playerTransform, Transform startingCheckpoint)
        {
            player = playerTransform;
            initialCheckpoint = startingCheckpoint;
            currentCheckpoint = startingCheckpoint;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            saveService = new SaveService();
            saveData = saveService.Load();
            GameState.RestoreCores(saveData.CollectedCoreIds);
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
            saveData.CollectedCoreIds = new List<string>();
            foreach (string knownCore in new[] { "courtyard-core", "side-chamber-core", "altar-chamber-core" })
                if (GameState.HasCollectedCore(knownCore)) saveData.CollectedCoreIds.Add(knownCore);
            saveService.Save(saveData);
            if (GameState.IsExitUnlocked && !exitWasUnlocked)
            {
                exitWasUnlocked = true;
                ExitUnlocked?.Invoke();
            }
            return true;
        }

        public void SetCheckpoint(Transform checkpoint)
        {
            if (checkpoint == null) return;
            currentCheckpoint = checkpoint;
            saveData.CheckpointId = checkpoint.name;
            saveService.Save(saveData);
        }

        public void ResetPlayerToCheckpoint(string reason = "Caught by guardian")
        {
            runStats.Captures++;
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
            HasWon = true;
            runStats.CompletionSeconds = Time.time - runStartedAt;
            ScoreResult result = ScoreService.CalculateFinalScore(runStats);
            if (result.Score > saveData.BestScore) saveData.BestScore = result.Score;
            if (saveData.BestCompletionSeconds <= 0f || runStats.CompletionSeconds < saveData.BestCompletionSeconds)
                saveData.BestCompletionSeconds = runStats.CompletionSeconds;
            saveService.Save(saveData);
            Victory?.Invoke();
        }
    }
}
