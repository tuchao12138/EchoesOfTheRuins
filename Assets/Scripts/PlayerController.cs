using UnityEngine;
using System;

namespace EchoesOfTheRuins
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField, Min(0.1f)] private float jumpHeight = 1.4f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float mouseSensitivity = 2f;
        private CharacterController controller;
        private PlayerLocomotionModel locomotion;
        private CharacterMotionAnimator motionAnimator;
        private float verticalVelocity;
        private float pitch;
        private bool inShadow;
        private bool crouching;
        private bool inputLocked;
        private int echoStones = 3;

        public bool IsCrouching => crouching;
        public bool IsInShadow => inShadow;
        public int EchoStones => echoStones;
        public int EchoStoneCount => echoStones;
        public float VisibilityMultiplier => (inShadow ? .45f : 1f) * (crouching ? .55f : 1f);
        public event Action MovementStarted;
        public event Action<LocomotionFrame> LocomotionChanged;
        public event Action<bool> CrouchChanged;
        public event Action<bool> ShadowChanged;
        public event Action<int> EchoStoneCountChanged;
        public event Action EchoStoneUsed;
        private bool movementAnnounced;

        public void Configure(Transform pivot) => cameraPivot = pivot;

        public void SetInputLocked(bool value) => inputLocked = value;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            locomotion = new PlayerLocomotionModel();
            if (cameraPivot == null && Camera.main != null) cameraPivot = Camera.main.transform;
        }

        private void Start() => Cursor.lockState = CursorLockMode.Locked;

        private void Update()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            if (cameraPivot != null)
            {
                pitch = Mathf.Clamp(pitch - mouseY, -75f, 75f);
                cameraPivot.rotation = Quaternion.Euler(pitch, cameraPivot.eulerAngles.y + mouseX, 0f);
            }

            bool nextCrouching = inputLocked ? crouching : Input.GetKey(KeyCode.C);
            if (nextCrouching != crouching) CrouchChanged?.Invoke(nextCrouching);
            crouching = nextCrouching;
            controller.height = crouching ? 1.15f : 1.8f;
            controller.center = new Vector3(0f, controller.height * .5f, 0f);

            if (!inputLocked && Input.GetKeyDown(KeyCode.Q) && echoStones > 0)
            {
                echoStones--;
                Vector3 throwPoint = transform.position + transform.forward * 5f + Vector3.up * .25f;
                NoiseSystem.Emit(throwPoint, 11f);
                EchoStoneCountChanged?.Invoke(echoStones);
                EchoStoneUsed?.Invoke();
            }

            bool grounded = controller.isGrounded;
            bool jumpPressed = !inputLocked && Input.GetButtonDown("Jump") && grounded;
            if (grounded)
            {
                verticalVelocity = -2f;
                if (jumpPressed) verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            verticalVelocity += gravity * Time.deltaTime;

            float cameraYaw = cameraPivot != null ? cameraPivot.eulerAngles.y : transform.eulerAngles.y;
            Vector2 moveInput = inputLocked
                ? Vector2.zero
                : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            LocomotionFrame frame = locomotion.Tick(Time.deltaTime,
                new PlayerInputFrame(
                    moveInput,
                    cameraYaw,
                    !inputLocked && Input.GetKey(KeyCode.LeftShift),
                    crouching,
                    jumpPressed,
                    grounded));

            Quaternion cameraRotation = cameraPivot != null ? cameraPivot.rotation : Quaternion.identity;
            transform.rotation = Quaternion.Euler(0f, frame.FacingYaw, 0f);
            if (cameraPivot != null) cameraPivot.rotation = cameraRotation;

            Vector3 velocity = frame.WorldDirection * frame.Speed;
            if (!movementAnnounced && velocity.sqrMagnitude > .01f)
            {
                movementAnnounced = true;
                MovementStarted?.Invoke();
            }
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
            if (motionAnimator == null) motionAnimator = GetComponent<CharacterMotionAnimator>();
            motionAnimator?.Play(ToAnimationRole(frame.State));
            LocomotionChanged?.Invoke(frame);
        }

        private static AnimationRole ToAnimationRole(LocomotionState state) => state switch
        {
            LocomotionState.Walk => AnimationRole.Walk,
            LocomotionState.Run => AnimationRole.Run,
            LocomotionState.Sprint => AnimationRole.Sprint,
            LocomotionState.Crouch => AnimationRole.Crouch,
            LocomotionState.Jump => AnimationRole.Jump,
            LocomotionState.Fall => AnimationRole.Fall,
            LocomotionState.Throw => AnimationRole.Throw,
            LocomotionState.Hit => AnimationRole.Hit,
            _ => AnimationRole.Idle
        };

        public void SetInShadow(bool value)
        {
            if (inShadow == value) return;
            inShadow = value;
            ShadowChanged?.Invoke(value);
        }
    }
}
