using UnityEngine;

namespace EchoesOfTheRuins
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField, Min(0.1f)] private float moveSpeed = 5f;
        [SerializeField, Min(0.1f)] private float jumpHeight = 1.4f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float mouseSensitivity = 2f;
        private CharacterController controller;
        private float verticalVelocity;
        private float pitch;

        public void Configure(Transform pivot) => cameraPivot = pivot;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraPivot == null && Camera.main != null) cameraPivot = Camera.main.transform;
        }

        private void Start() => Cursor.lockState = CursorLockMode.Locked;

        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            transform.Rotate(Vector3.up * mouseX);
            if (cameraPivot != null)
            {
                pitch = Mathf.Clamp(pitch - mouseY, -75f, 75f);
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }

            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
                if (Input.GetButtonDown("Jump")) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            verticalVelocity += gravity * Time.deltaTime;
            Vector3 movement = (transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal")).normalized * moveSpeed;
            movement.y = verticalVelocity;
            controller.Move(movement * Time.deltaTime);
        }
    }
}
