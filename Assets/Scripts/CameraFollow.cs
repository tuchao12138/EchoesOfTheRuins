using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField, Min(0f)] private float followSharpness = 15f;

        public void Configure(Transform followTarget, Vector3 offset)
        {
            target = followTarget;
            localOffset = offset;
        }

        private void LateUpdate()
        {
            if (target == null) return;
            // The production pivot is parented to the player. Smoothing that child in
            // world space creates a second follow delay and makes the floor appear to
            // slide beneath the character, so keep that hierarchy exact.
            if (transform.IsChildOf(target))
            {
                transform.localPosition = localOffset;
                return;
            }
            transform.position = Vector3.Lerp(transform.position, target.TransformPoint(localOffset), 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
        }
    }
}
