using UnityEngine;

namespace EchoesOfTheRuins
{
    public static class CameraCollisionSolver
    {
        public static float ResolveDistance(
            Vector3 origin,
            Vector3 direction,
            float desiredDistance,
            float radius,
            int collisionMask,
            float padding,
            float minimumDistance)
        {
            if (desiredDistance <= minimumDistance) return minimumDistance;
            if (Physics.SphereCast(origin, radius, direction.normalized, out RaycastHit hit, desiredDistance, collisionMask, QueryTriggerInteraction.Ignore))
                return Mathf.Max(minimumDistance, hit.distance - padding);
            return desiredDistance;
        }
    }

    /// <summary>Keeps the shoulder camera in front of level geometry and restores it smoothly.</summary>
    public sealed class ThirdPersonCameraCollision : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private Vector3 desiredLocalPosition = new Vector3(1.05f, .4f, -5.75f);
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField, Min(.05f)] private float radius = .24f;
        [SerializeField, Min(0f)] private float padding = .12f;
        [SerializeField, Min(.2f)] private float minimumDistance = .7f;
        [SerializeField, Min(0f)] private float restoreSharpness = 12f;

        public void Configure(Transform cameraPivot, Vector3 localPosition)
        {
            pivot = cameraPivot;
            desiredLocalPosition = localPosition;
            transform.localPosition = localPosition;
        }

        private void LateUpdate()
        {
            if (pivot == null) return;
            float desiredDistance = desiredLocalPosition.magnitude;
            Vector3 worldDirection = pivot.TransformDirection(desiredLocalPosition.normalized);
            float safeDistance = CameraCollisionSolver.ResolveDistance(
                pivot.position, worldDirection, desiredDistance, radius, collisionMask, padding, minimumDistance);
            Vector3 targetLocalPosition = desiredLocalPosition.normalized * safeDistance;
            float blend = 1f - Mathf.Exp(-restoreSharpness * Time.deltaTime);
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPosition, blend);
        }
    }
}
