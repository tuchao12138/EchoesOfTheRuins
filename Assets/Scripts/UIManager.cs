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
            GUI.Box(new Rect(16f, 16f, 390f, 118f), "ECHOES OF THE RUINS");
            GUI.Label(new Rect(30f, 46f, 340f, 24f), coreCount);
            GUI.Label(new Rect(30f, 70f, 360f, 24f), objective);
            GUI.Label(new Rect(30f, 94f, 360f, 24f), feedback);
            string stealth = player == null ? "" : (player.IsInShadow ? "SHADOW" : "EXPOSED") + (player.IsCrouching ? " | CROUCH" : "") + " | Echo: " + player.EchoStones;
            GUI.Label(new Rect(16f, Screen.height - 46f, 520f, 26f), "C crouch  Shift sprint  Q echo stone  " + stealth);
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
    }
}
