using UnityEngine;

namespace EchoesOfTheRuins
{
    [RequireComponent(typeof(Collider))]
    public sealed class RelicCollectible : MonoBehaviour, IInteractable, IInteractionPromptProvider
    {
        [SerializeField] private string relicId;
        private bool collected;

        public string InteractionPrompt => "E — RECOVER RELIC / 收集遗物（可选）";

        public void Configure(string id) => relicId = id;

        private void Start()
        {
            if (GameManager.Instance == null || !GameManager.Instance.HasRelic(relicId)) return;
            collected = true;
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!collected && other.CompareTag("Player")) other.GetComponent<PlayerInteractor>()?.Register(this);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player")) other.GetComponent<PlayerInteractor>()?.Unregister(this);
        }

        public void Interact(PlayerInteractor player)
        {
            if (collected || GameManager.Instance == null || !GameManager.Instance.CollectRelic(relicId)) return;
            collected = true;
            player?.Unregister(this);
            gameObject.SetActive(false);
        }
    }
}
