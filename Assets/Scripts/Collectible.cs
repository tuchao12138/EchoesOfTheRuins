using UnityEngine;

namespace EchoesOfTheRuins
{
[RequireComponent(typeof(SphereCollider))]
    public sealed class Collectible : MonoBehaviour, IInteractable, IHoldInteractable, IInteractionPromptProvider
    {
        [SerializeField] private string coreId;
        [SerializeField, Min(.5f)] private float activationSeconds = 1.5f;
        private bool collected;

        public string CoreId => coreId;
        public float HoldDuration => activationSeconds;
        public bool IsHoldInterrupted
        {
            get
            {
                foreach (GuardianAI guardian in FindObjectsByType<GuardianAI>(FindObjectsSortMode.None))
                    if (guardian != null && guardian.IsChasing) return true;
                return false;
            }
        }
        public string InteractionPrompt => IsHoldInterrupted
            ? "CORE LOCKED — BREAK LINE OF SIGHT / 核心锁定"
            : "HOLD E 1.5s — ATTUNE CORE / 按住 E 激活核心";

        public void Configure(string id) => coreId = id;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(coreId)) coreId = gameObject.name;
        }

        private void Start()
        {
            if (GameManager.Instance != null && GameManager.Instance.GameState.HasCollectedCore(coreId))
            {
                collected = true;
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || !other.CompareTag("Player")) return;
            other.GetComponent<PlayerInteractor>()?.Register(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            other.GetComponent<PlayerInteractor>()?.Unregister(this);
        }

        public void Interact(PlayerInteractor player)
        {
            if (collected || GameManager.Instance == null) return;
            if (!GameManager.Instance.CollectCore(coreId)) return;
            collected = true;
            player?.Unregister(this);
            gameObject.SetActive(false);
        }

        public void CompleteHold(PlayerInteractor player) => Interact(player);
    }
}
