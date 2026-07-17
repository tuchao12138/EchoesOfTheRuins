using UnityEngine;

namespace EchoesOfTheRuins
{
    [RequireComponent(typeof(Collider))]
    public sealed class ExitGate : MonoBehaviour
    {
        [SerializeField] private GameObject lockedVisual;
        [SerializeField] private GameObject unlockedVisual;
        private bool unlocked;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnEnable()
        {
            if (GameManager.Instance != null) GameManager.Instance.ExitUnlocked += Unlock;
        }

        private void Start()
        {
            if (GameManager.Instance != null && GameManager.Instance.GameState.IsExitUnlocked) Unlock();
            RefreshVisuals();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null) GameManager.Instance.ExitUnlocked -= Unlock;
        }

        private void Unlock()
        {
            unlocked = true;
            RefreshVisuals();
        }

        private void RefreshVisuals()
        {
            if (lockedVisual != null) lockedVisual.SetActive(!unlocked);
            if (unlockedVisual != null) unlockedVisual.SetActive(unlocked);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (unlocked && other.CompareTag("Player")) GameManager.Instance?.CompleteEscape();
        }
    }
}
