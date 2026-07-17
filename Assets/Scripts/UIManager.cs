using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Prototype HUD drawn without a UGUI package dependency.</summary>
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField] private GuardianAI guardian;
        [SerializeField, Min(0f)] private float feedbackDuration = 2f;

        private string coreCount = "Cores: 0/3";
        private string objective = "Find the remaining energy cores";
        private string feedback;
        private float feedbackExpiresAt;
        private bool subscribed;
        private PlayerController player;
        private TutorialDirector tutorial;
        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle promptStyle;

        public void Configure(GuardianAI patrolGuardian) => guardian = patrolGuardian;

        private void Start()
        {
            Subscribe();
            if (GameManager.Instance != null)
                UpdateCoreCount(GameManager.Instance.GameState.CollectedCoreCount, GameManager.Instance.RequiredCoreCount);
        }

        private void Update()
        {
            if (!subscribed) Subscribe();
            if (player == null && GameManager.Instance != null && GameManager.Instance.PlayerTransform != null)
                player = GameManager.Instance.PlayerTransform.GetComponent<PlayerController>();
            if (tutorial == null) tutorial = TutorialDirector.Active;
            if (feedbackExpiresAt > 0f && Time.time >= feedbackExpiresAt)
            {
                feedback = string.Empty;
                feedbackExpiresAt = 0f;
            }
            // Detection is a live status, not a one-time message. Timed reset/unlock
            // feedback keeps priority until it expires; victory remains persistent.
            if (feedbackExpiresAt == 0f && guardian != null && GameManager.Instance != null && !GameManager.Instance.HasWon)
                feedback = FormatGuardianState(guardian.CurrentState);
        }

        private void OnGUI()
        {
            EnsureStyles();
            GUI.Box(new Rect(18f, 18f, 420f, 126f), GUIContent.none);
            GUI.Label(new Rect(34f, 29f, 380f, 28f), "ECHOES OF THE RUINS  /  遗迹回响", titleStyle);
            GUI.Label(new Rect(34f, 62f, 370f, 23f), coreCount, labelStyle);
            GUI.Label(new Rect(34f, 87f, 380f, 23f), objective, labelStyle);
            GUI.Label(new Rect(34f, 112f, 380f, 23f), feedback, labelStyle);

            if (tutorial != null && tutorial.Stage != TutorialStage.Complete)
            {
                const float width = 620f;
                GUI.Box(new Rect((Screen.width - width) * .5f, 32f, width, 86f), GUIContent.none);
                GUI.Label(new Rect((Screen.width - width) * .5f + 18f, 45f, width - 36f, 62f), tutorial.Prompt, promptStyle);
            }

            string stealth = player == null ? "" : (player.IsInShadow ? "阴影 / SHADOW" : "暴露 / EXPOSED") + (player.IsCrouching ? "  |  蹲伏 / CROUCH" : "") + "  |  回响石 / ECHO: " + player.EchoStones;
            GUI.Box(new Rect(18f, Screen.height - 57f, 650f, 38f), GUIContent.none);
            GUI.Label(new Rect(30f, Screen.height - 49f, 620f, 26f), "C 蹲伏   Shift 疾跑   Q 回响石   " + stealth, labelStyle);
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null || !subscribed) return;
            GameManager.Instance.CoreCountChanged -= UpdateCoreCount;
            GameManager.Instance.ExitUnlocked -= ShowExitUnlocked;
            GameManager.Instance.PlayerReset -= ShowReset;
            GameManager.Instance.Victory -= ShowVictory;
            subscribed = false;
        }

        private void Subscribe()
        {
            if (subscribed || GameManager.Instance == null) return;
            GameManager.Instance.CoreCountChanged += UpdateCoreCount;
            GameManager.Instance.ExitUnlocked += ShowExitUnlocked;
            GameManager.Instance.PlayerReset += ShowReset;
            GameManager.Instance.Victory += ShowVictory;
            subscribed = true;
        }

        private void UpdateCoreCount(int count, int required)
        {
            coreCount = "Cores: " + count + "/" + required;
            objective = count < required ? "Find the remaining energy cores" : "Reach the exit";
        }

        private void ShowExitUnlocked() => ShowFeedback("EXIT UNLOCKED");
        private void ShowReset(string reason) => ShowFeedback(reason);
        private void ShowVictory() => ShowFeedback("ESCAPED THE RUINS", true);

        private void ShowFeedback(string message, bool persistent = false)
        {
            feedback = message;
            feedbackExpiresAt = persistent ? -1f : Time.time + feedbackDuration;
        }

        private static string FormatGuardianState(GuardianState state)
        {
            switch (state)
            {
                case GuardianState.Investigate: return "SUSPICIOUS - guardian heard an echo";
                case GuardianState.Search: return "SEARCHING - stay in shadow";
                case GuardianState.Chase: return "DETECTED - RUN!";
                case GuardianState.Capture: return "CAUGHT - returning to checkpoint";
                default: return "HIDDEN - observe the patrol";
            }
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold };
            titleStyle.normal.textColor = new Color(.35f, .9f, 1f);
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleLeft };
            labelStyle.normal.textColor = Color.white;
            promptStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            promptStyle.normal.textColor = new Color(.9f, .96f, 1f);
        }
    }
}
