using UnityEngine;

namespace EchoesOfTheRuins
{
    [RequireComponent(typeof(Collider))]
    public sealed class ShadowZone : MonoBehaviour
    {
        private void Reset() => GetComponent<Collider>().isTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null) player.SetInShadow(true);
        }

        private void OnTriggerExit(Collider other)
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null) player.SetInShadow(false);
        }
    }
}
