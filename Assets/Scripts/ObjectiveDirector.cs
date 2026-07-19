using System;
using System.Collections;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class ObjectiveDirector : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Transform firstCoreTarget;
        [SerializeField] private Transform exitTarget;
        [SerializeField] private Transform guardianObservationTarget;
        [SerializeField] private Camera revealCamera;
        [SerializeField] private WorldObjectiveMarker worldMarker;
        [SerializeField, Min(.1f)] private float routeRevealSeconds = 6f;
        private bool moved;
        private bool sprinted;

        public static ObjectiveDirector Active { get; private set; }
        public ObjectiveTracker Tracker { get; private set; }
        public event Action<ObjectiveData> ObjectiveChanged;
        public PlayerController Player => player;
        public Transform FirstCoreTarget => firstCoreTarget;
        public Transform ExitTarget => exitTarget;
        public Transform GuardianObservationTarget => guardianObservationTarget;

        public void Configure(
            PlayerController configuredPlayer,
            Transform firstCore,
            Transform exit,
            Transform observationTarget,
            WorldObjectiveMarker marker,
            Camera routeCamera = null)
        {
            player = configuredPlayer;
            firstCoreTarget = firstCore;
            exitTarget = exit;
            guardianObservationTarget = observationTarget;
            worldMarker = marker;
            revealCamera = routeCamera;
        }

        private void Awake()
        {
            Active = this;
        }

        private void Start()
        {
            ObjectiveStage initial = ReadSavedStage();
            Tracker = new ObjectiveTracker(initial, GameManager.Instance == null ? 0 : GameManager.Instance.GameState.CollectedCoreCount);
            Tracker.Changed += OnObjectiveChanged;
            if (player == null && GameManager.Instance?.PlayerTransform != null)
                player = GameManager.Instance.PlayerTransform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.MovementStarted += OnMoved;
                player.LocomotionChanged += OnLocomotionChanged;
                player.CrouchChanged += OnCrouchChanged;
                player.ShadowChanged += OnShadowChanged;
                player.EchoStoneUsed += OnEchoStoneUsed;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CoreCountChanged += OnCoreCountChanged;
                GameManager.Instance.ExitUnlocked += OnExitUnlocked;
                GameManager.Instance.Victory += OnVictory;
            }
            if (Tracker.Current.Stage == ObjectiveStage.Briefing) StartCoroutine(RevealRoute());
        }

        public void NotifyGuardianObserved() => Tracker.Notify(ObjectiveSignal.ObservedGuardian, worldPosition: PositionOf(guardianObservationTarget));

        private IEnumerator RevealRoute()
        {
            if (player != null) player.SetInputLocked(true);
            float elapsed = 0f;
            Vector3 start = PositionOf(exitTarget);
            Vector3 middle = PositionOf(firstCoreTarget);
            Vector3 end = player == null ? middle : player.transform.position;
            while (elapsed < routeRevealSeconds)
            {
                elapsed += Time.deltaTime;
                if (revealCamera != null)
                {
                    float t = Mathf.Clamp01(elapsed / routeRevealSeconds);
                    revealCamera.transform.position = t < .5f ? Vector3.Lerp(start, middle, t * 2f) : Vector3.Lerp(middle, end, (t - .5f) * 2f);
                }
                yield return null;
            }
            if (player != null) player.SetInputLocked(false);
            Tracker.Notify(ObjectiveSignal.BriefingFinished, worldPosition: PositionOf(firstCoreTarget));
        }

        private void OnMoved() { moved = true; TryAdvanceMovement(); }
        private void OnLocomotionChanged(LocomotionFrame frame) { if (frame.State == LocomotionState.Sprint) sprinted = true; TryAdvanceMovement(); }
        private void TryAdvanceMovement() { if (moved && sprinted) Tracker.Notify(ObjectiveSignal.MovedAndSprinted, worldPosition: PositionOf(guardianObservationTarget)); }
        private void OnCrouchChanged(bool crouched) { if (crouched && player != null && player.IsInShadow) Tracker.Notify(ObjectiveSignal.CrouchedInShadow, worldPosition: PositionOf(firstCoreTarget)); }
        private void OnShadowChanged(bool shadowed) { if (shadowed && player != null && player.IsCrouching) Tracker.Notify(ObjectiveSignal.CrouchedInShadow, worldPosition: PositionOf(firstCoreTarget)); }
        private void OnEchoStoneUsed() => Tracker.Notify(ObjectiveSignal.UsedEchoStone, worldPosition: PositionOf(firstCoreTarget));
        private void OnCoreCountChanged(int count, int _) => Tracker.Notify(ObjectiveSignal.CoreCountChanged, count, PositionOf(firstCoreTarget));
        private void OnExitUnlocked() => Tracker.Notify(ObjectiveSignal.ExitUnlocked, worldPosition: PositionOf(exitTarget));
        private void OnVictory() => Tracker.Notify(ObjectiveSignal.Escaped);

        private void OnObjectiveChanged(ObjectiveData objective)
        {
            GameManager.Instance?.SetObjectiveStage(objective.Stage);
            if (worldMarker != null) worldMarker.SetTarget(TargetFor(objective.Stage));
            ObjectiveChanged?.Invoke(objective);
        }

        private Transform TargetFor(ObjectiveStage stage) => stage == ObjectiveStage.ReachExit ? exitTarget :
            stage == ObjectiveStage.CollectCores || stage == ObjectiveStage.Distract || stage == ObjectiveStage.Hide ? firstCoreTarget : guardianObservationTarget;

        private ObjectiveStage ReadSavedStage()
        {
            string value = GameManager.Instance?.ObjectiveStage ?? ObjectiveStage.Briefing.ToString();
            return Enum.TryParse(value, out ObjectiveStage stage) ? stage : ObjectiveStage.Briefing;
        }

        private static Vector3 PositionOf(Transform target) => target == null ? Vector3.zero : target.position;

        private void OnDestroy()
        {
            if (Active == this) Active = null;
            if (Tracker != null) Tracker.Changed -= OnObjectiveChanged;
            if (player != null)
            {
                player.MovementStarted -= OnMoved; player.LocomotionChanged -= OnLocomotionChanged;
                player.CrouchChanged -= OnCrouchChanged; player.ShadowChanged -= OnShadowChanged; player.EchoStoneUsed -= OnEchoStoneUsed;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CoreCountChanged -= OnCoreCountChanged; GameManager.Instance.ExitUnlocked -= OnExitUnlocked; GameManager.Instance.Victory -= OnVictory;
            }
        }
    }
}
