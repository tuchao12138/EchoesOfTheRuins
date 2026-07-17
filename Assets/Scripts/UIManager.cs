using UnityEngine;
using UnityEngine.UI;

namespace EchoesOfTheRuins
{
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField] private Text coreCountText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text feedbackText;
        [SerializeField] private GuardianAI guardian;

        private void OnEnable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.CoreCountChanged += UpdateCoreCount;
            GameManager.Instance.ExitUnlocked += ShowExitUnlocked;
            GameManager.Instance.PlayerReset += ShowReset;
            GameManager.Instance.Victory += ShowVictory;
        }

        private void Start()
        {
            if (GameManager.Instance != null) UpdateCoreCount(GameManager.Instance.GameState.CollectedCoreCount, GameManager.Instance.RequiredCoreCount);
        }

        private void Update()
        {
            if (guardian != null && GameManager.Instance != null && !GameManager.Instance.HasWon && feedbackText != null)
                feedbackText.text = guardian.IsChasing ? "DETECTED — RUN!" : "Hidden";
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.CoreCountChanged -= UpdateCoreCount;
            GameManager.Instance.ExitUnlocked -= ShowExitUnlocked;
            GameManager.Instance.PlayerReset -= ShowReset;
            GameManager.Instance.Victory -= ShowVictory;
        }

        private void UpdateCoreCount(int count, int required)
        {
            if (coreCountText != null) coreCountText.text = "Cores: " + count + "/" + required;
            if (objectiveText != null) objectiveText.text = count < required ? "Find the remaining energy cores" : "Reach the exit";
        }
        private void ShowExitUnlocked() { if (feedbackText != null) feedbackText.text = "EXIT UNLOCKED"; }
        private void ShowReset(string reason) { if (feedbackText != null) feedbackText.text = reason; }
        private void ShowVictory() { if (feedbackText != null) feedbackText.text = "ESCAPED THE RUINS"; }
    }
}
