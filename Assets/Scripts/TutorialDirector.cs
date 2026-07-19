using System;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class TutorialDirector : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float protectionSeconds = 8f;
        private SpawnProtection protection;
        private int lastReportedProtectionSecond = -1;

        public static TutorialDirector Active { get; private set; }
        public TutorialStage Stage => ToTutorialStage(ObjectiveDirector.Active?.Tracker?.Current.Stage ?? ObjectiveStage.Briefing);
        public bool CanBeDetected => protection == null || protection.CanBeDetected;
        public string Prompt => TutorialCopy.Get(Stage);
        public ObjectiveDirector RuntimeDirector { get; private set; }
        public WorldObjectiveMarker RuntimeMarker { get; private set; }
        public float SafeEntryRemaining => protection == null ? 0f : protection.RemainingSeconds;
        public event Action<TutorialStage, float> PresentationChanged;

        public void Configure(PlayerController configuredPlayer)
        {
            Configure(configuredPlayer, null, null, null, null);
        }

        public void Configure(
            PlayerController configuredPlayer,
            Transform firstCore,
            Transform exit,
            Transform guardian,
            Camera routeCamera)
        {
            EnsureObjectiveRuntime(configuredPlayer, firstCore, exit, guardian, routeCamera);
        }

        private void Awake()
        {
            Active = this;
            protection = new SpawnProtection(protectionSeconds);
            ReportPresentation(force: true);
        }

        private void Start()
        {
            EnsureObjectiveRuntime(null, null, null, null, null);
            Debug.Log("SAFE ENTRY: guardian detection is disabled for eight seconds.");
        }

        private void Update()
        {
            protection.Tick(Time.deltaTime);
            ReportPresentation();
        }

        private void OnDestroy()
        {
            if (RuntimeDirector != null) RuntimeDirector.ObjectiveChanged -= OnObjectiveChanged;
            if (Active == this) Active = null;
        }

        private void EnsureObjectiveRuntime(
            PlayerController configuredPlayer,
            Transform firstCore,
            Transform exit,
            Transform guardian,
            Camera routeCamera)
        {
            RuntimeDirector ??= GetComponent<ObjectiveDirector>() ?? gameObject.AddComponent<ObjectiveDirector>();
            RuntimeMarker ??= GetComponent<WorldObjectiveMarker>() ?? gameObject.AddComponent<WorldObjectiveMarker>();

            configuredPlayer ??= GameManager.Instance?.PlayerTransform?.GetComponent<PlayerController>()
                ?? UnityEngine.Object.FindFirstObjectByType<PlayerController>();
            firstCore ??= UnityEngine.Object.FindFirstObjectByType<Collectible>()?.transform;
            exit ??= UnityEngine.Object.FindFirstObjectByType<ExitGate>()?.transform;
            guardian ??= UnityEngine.Object.FindFirstObjectByType<GuardianAI>()?.transform;
            routeCamera ??= Camera.main;

            RuntimeDirector.Configure(configuredPlayer, firstCore, exit, guardian, RuntimeMarker, routeCamera);
            RuntimeDirector.ObjectiveChanged -= OnObjectiveChanged;
            RuntimeDirector.ObjectiveChanged += OnObjectiveChanged;
            ReportPresentation(force: true);
        }

        private void OnObjectiveChanged(ObjectiveData _) => ReportPresentation(force: true);

        private void ReportPresentation(bool force = false)
        {
            int remaining = Mathf.CeilToInt(SafeEntryRemaining);
            if (!force && remaining == lastReportedProtectionSecond) return;
            lastReportedProtectionSecond = remaining;
            PresentationChanged?.Invoke(Stage, SafeEntryRemaining);
        }

        private static TutorialStage ToTutorialStage(ObjectiveStage stage) => stage switch
        {
            ObjectiveStage.Move => TutorialStage.Movement,
            ObjectiveStage.Hide => TutorialStage.Shadow,
            ObjectiveStage.Distract => TutorialStage.EchoStone,
            ObjectiveStage.Observe => TutorialStage.Awareness,
            ObjectiveStage.Complete => TutorialStage.Complete,
            _ => TutorialStage.Objective
        };
    }
}
