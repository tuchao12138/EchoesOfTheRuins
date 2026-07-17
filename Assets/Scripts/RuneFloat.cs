using UnityEngine;

namespace EchoesOfTheRuins
{
    public sealed class RuneFloat : MonoBehaviour
    {
        [SerializeField] private float height = .22f;
        [SerializeField] private float frequency = 1.3f;
        [SerializeField] private float rotationSpeed = 75f;
        private Vector3 startPosition;

        private void Awake() => startPosition = transform.localPosition;

        private void Update()
        {
            transform.localPosition = startPosition + Vector3.up * Mathf.Sin(Time.time * frequency) * height;
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
