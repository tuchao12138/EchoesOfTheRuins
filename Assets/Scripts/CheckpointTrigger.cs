using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Updates the respawn point when the player enters a marked ruin area.</summary>
    [RequireComponent(typeof(Collider))]
    public sealed class CheckpointTrigger : MonoBehaviour
    {
        [SerializeField] private Transform respawnPoint;

        public void Configure(Transform point) => respawnPoint = point;

        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) GameManager.Instance?.SetCheckpoint(respawnPoint != null ? respawnPoint : transform);
        }
    }
}
