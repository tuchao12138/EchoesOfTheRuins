using UnityEngine;

namespace EchoesOfTheRuins
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField, Min(0.1f)] private float moveSpeed = 5f;
        [SerializeField, Min(0.1f)] private float sprintSpeed = 7.5f;
        [SerializeField, Min(0.1f)] private float crouchSpeed = 2.4f;
        [SerializeField, Min(0.1f)] private float jumpHeight = 1.4f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float mouseSensitivity = 2f;
        private CharacterController controller;
        private float verticalVelocity;
        private float pitch;
        private bool inShadow;
        private bool crouching;
        private int echoStones = 3;

        public bool IsCrouching => crouching;
        public bool IsInShadow => inShadow;
        public int EchoStones => echoStones;
        public float VisibilityMultiplier => (inShadow ? .45f : 1f) * (crouching ? .55f : 1f);

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

            crouching = Input.GetKey(KeyCode.C);
            controller.height = crouching ? 1.15f : 1.8f;
            controller.center = new Vector3(0f, controller.height * .5f, 0f);

            if (Input.GetKeyDown(KeyCode.Q) && echoStones > 0)
            {
                echoStones--;
                Vector3 throwPoint = transform.position + transform.forward * 5f + Vector3.up * .25f;
                NoiseSystem.Emit(throwPoint, 11f);
            }

            if (controller.isGrounded)
            {
                verticalVelocity = -2f;
                if (Input.GetButtonDown("Jump")) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            verticalVelocity += gravity * Time.deltaTime;
            float speed = crouching ? crouchSpeed : (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed);
            Vector3 movement = (transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal")).normalized * speed;
            movement.y = verticalVelocity;
            controller.Move(movement * Time.deltaTime);
        }

        public void SetInShadow(bool value) => inShadow = value;
    }
}
