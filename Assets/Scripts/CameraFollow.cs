using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.6f, 0f);
        [SerializeField, Min(0f)] private float followSharpness = 15f;

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.Lerp(transform.position, target.TransformPoint(localOffset), 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
        }
    }
}
