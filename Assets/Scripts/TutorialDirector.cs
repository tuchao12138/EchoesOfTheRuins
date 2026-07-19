using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class TutorialDirector : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float protectionSeconds = 8f;
        private SpawnProtection protection;

        public static TutorialDirector Active { get; private set; }
        public TutorialStage Stage => ToTutorialStage(ObjectiveDirector.Active?.Tracker?.Current.Stage ?? ObjectiveStage.Briefing);
        public bool CanBeDetected => protection == null || protection.CanBeDetected;
        public string Prompt => TutorialCopy.Get(Stage);
        public ObjectiveDirector RuntimeDirector { get; private set; }
        public WorldObjectiveMarker RuntimeMarker { get; private set; }

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
        }

        private void Start()
        {
            EnsureObjectiveRuntime(null, null, null, null, null);
            Debug.Log("SAFE ENTRY: guardian detection is disabled for eight seconds.");
        }

        private void Update()
        {
            protection.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
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
                ?? Object.FindFirstObjectByType<PlayerController>();
            firstCore ??= Object.FindFirstObjectByType<Collectible>()?.transform;
            exit ??= Object.FindFirstObjectByType<ExitGate>()?.transform;
            guardian ??= Object.FindFirstObjectByType<GuardianAI>()?.transform;
            routeCamera ??= Camera.main;

            RuntimeDirector.Configure(configuredPlayer, firstCore, exit, guardian, RuntimeMarker, routeCamera);
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
