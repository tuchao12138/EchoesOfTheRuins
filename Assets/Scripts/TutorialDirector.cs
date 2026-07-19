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

        public void Configure(PlayerController configuredPlayer) { }

        private void Awake()
        {
            Active = this;
            protection = new SpawnProtection(protectionSeconds);
        }

        private void Start()
        {
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
