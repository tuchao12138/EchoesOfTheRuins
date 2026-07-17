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
            if (feedbackExpiresAt > 0f && Time.time >= feedbackExpiresAt)
            {
                feedback = string.Empty;
                feedbackExpiresAt = 0f;
            }
            // Detection is a live status, not a one-time message. Timed reset/unlock
            // feedback keeps priority until it expires; victory remains persistent.
            if (feedbackExpiresAt == 0f && guardian != null && GameManager.Instance != null && !GameManager.Instance.HasWon)
                feedback = guardian.IsChasing ? "DETECTED - RUN!" : "Hidden";
        }

        private void OnGUI()
        {
            GUI.Label(new Rect(20f, 20f, 300f, 28f), coreCount);
            GUI.Label(new Rect(20f, 48f, 500f, 28f), objective);
            if (!string.IsNullOrEmpty(feedback)) GUI.Label(new Rect(20f, 76f, 500f, 28f), feedback);
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
    }
}
