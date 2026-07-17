using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class TutorialDirector : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float protectionSeconds = 8f;
        private TutorialProgress progress;
        private SpawnProtection protection;
        private PlayerController player;
        private float stageStartedAt;

        public static TutorialDirector Active { get; private set; }
        public TutorialStage Stage => progress == null ? TutorialStage.Objective : progress.Stage;
        public bool CanBeDetected => protection == null || protection.CanBeDetected;
        public string Prompt => PromptFor(Stage);

        public void Configure(PlayerController configuredPlayer)
        {
            player = configuredPlayer;
        }

        private void Awake()
        {
            Active = this;
            progress = new TutorialProgress();
            protection = new SpawnProtection(protectionSeconds);
            progress.StageChanged += OnStageChanged;
        }

        private void Start()
        {
            if (player == null && GameManager.Instance?.PlayerTransform != null)
                player = GameManager.Instance.PlayerTransform.GetComponent<PlayerController>();
            if (player == null) return;
            player.MovementStarted += progress.NotifyMoved;
            player.CrouchChanged += OnCrouchChanged;
            player.ShadowChanged += OnShadowChanged;
            player.EchoStoneUsed += progress.NotifyEchoStoneUsed;
        }

        private void Update()
        {
            protection.Tick(Time.deltaTime);
            if (Stage == TutorialStage.Objective && Time.time - stageStartedAt > 3.5f) progress.ContinueFromObjective();
            if (Stage == TutorialStage.Awareness && Time.time - stageStartedAt > 3.5f) progress.ContinueFromAwareness();
        }

        private void OnCrouchChanged(bool value)
        {
            if (value && player != null && player.IsInShadow) progress.NotifyCrouchedInShadow();
        }

        private void OnShadowChanged(bool value)
        {
            if (value && player != null && player.IsCrouching) progress.NotifyCrouchedInShadow();
        }

        private void OnStageChanged(TutorialStage _) => stageStartedAt = Time.time;

        private void OnDestroy()
        {
            if (Active == this) Active = null;
            if (progress != null) progress.StageChanged -= OnStageChanged;
            if (player == null) return;
            player.MovementStarted -= progress.NotifyMoved;
            player.CrouchChanged -= OnCrouchChanged;
            player.ShadowChanged -= OnShadowChanged;
            player.EchoStoneUsed -= progress.NotifyEchoStoneUsed;
        }

        private static string PromptFor(TutorialStage stage)
        {
            switch (stage)
            {
                case TutorialStage.Objective: return "目标 / OBJECTIVE\n收集 3 枚能量核心并逃离遗迹";
                case TutorialStage.Movement: return "移动 / MOVE\nWASD 移动，鼠标观察，Space 跳跃";
                case TutorialStage.Shadow: return "潜行 / STEALTH\n按 C 蹲伏并进入蓝色阴影";
                case TutorialStage.EchoStone: return "回响石 / ECHO STONE\n按 Q 干扰守卫，制造调查点";
                case TutorialStage.Awareness: return "观察守卫 / WATCH THE GUARD\n金色视野锥代表危险，红色代表追击";
                default: return "教学完成 / TUTORIAL COMPLETE\n收集核心，前往封印出口";
            }
        }
    }
}
