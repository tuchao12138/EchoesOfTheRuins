using UnityEngine;

namespace EchoesOfTheRuins
{
    [RequireComponent(typeof(Collider))]
    public sealed class Collectible : MonoBehaviour
    {
        [SerializeField] private string coreId;
        private bool collected;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(coreId)) coreId = gameObject.name;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (collected || !other.CompareTag("Player") || GameManager.Instance == null) return;
            if (!GameManager.Instance.CollectCore(coreId)) return;
            collected = true;
            gameObject.SetActive(false);
        }
    }
}
