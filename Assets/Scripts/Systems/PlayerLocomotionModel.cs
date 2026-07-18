using UnityEngine;

namespace EchoesOfTheRuins
{
    public enum LocomotionState
    {
        Idle,
        Walk,
        Run,
        Sprint,
        Crouch,
        Jump,
        Fall,
        Throw,
        Hit,
        Locked
    }

    public readonly struct PlayerInputFrame
    {
        public readonly Vector2 Move;
        public readonly float CameraYaw;
        public readonly bool Sprint;
        public readonly bool Crouch;
        public readonly bool JumpPressed;
        public readonly bool Grounded;

        public PlayerInputFrame(
            Vector2 move,
            float cameraYaw,
            bool sprint,
            bool crouch,
            bool jumpPressed,
            bool grounded)
        {
            Move = Vector2.ClampMagnitude(move, 1f);
            CameraYaw = cameraYaw;
            Sprint = sprint;
            Crouch = crouch;
            JumpPressed = jumpPressed;
            Grounded = grounded;
        }
    }

    public readonly struct LocomotionFrame
    {
        public readonly Vector3 WorldDirection;
        public readonly float Speed;
        public readonly float FacingYaw;
        public readonly LocomotionState State;

        public LocomotionFrame(
            Vector3 worldDirection,
            float speed,
            float facingYaw,
            LocomotionState state)
        {
            WorldDirection = worldDirection;
            Speed = speed;
            FacingYaw = facingYaw;
            State = state;
        }
    }

    public sealed class PlayerLocomotionModel
    {
        private readonly float walkSpeed;
        private readonly float runSpeed;
        private readonly float sprintSpeed;
        private readonly float crouchSpeed;
        private readonly float acceleration;
        private readonly float braking;
        private readonly float turnSpeed;
        private float speed;
        private float facingYaw;

        public PlayerLocomotionModel(
            float walkSpeed = 2f,
            float runSpeed = 4.5f,
            float sprintSpeed = 7f,
            float crouchSpeed = 2.2f,
            float acceleration = 12f,
            float braking = 16f,
            float turnDegreesPerSecond = 600f)
        {
            this.walkSpeed = walkSpeed;
            this.runSpeed = runSpeed;
            this.sprintSpeed = sprintSpeed;
            this.crouchSpeed = crouchSpeed;
            this.acceleration = acceleration;
            this.braking = braking;
            turnSpeed = turnDegreesPerSecond;
        }

        public LocomotionFrame Tick(float dt, PlayerInputFrame input)
        {
            Quaternion cameraYaw = Quaternion.Euler(0f, input.CameraYaw, 0f);
            Vector3 direction = cameraYaw * new Vector3(input.Move.x, 0f, input.Move.y);
            if (direction.sqrMagnitude > 1f) direction.Normalize();

            float magnitude = input.Move.magnitude;
            float targetSpeed = magnitude < .05f
                ? 0f
                : input.Crouch
                    ? crouchSpeed
                    : input.Sprint
                        ? sprintSpeed
                        : magnitude < .55f
                            ? walkSpeed * magnitude / .55f
                            : runSpeed;

            float rate = targetSpeed > speed ? acceleration : braking;
            speed = Mathf.MoveTowards(speed, targetSpeed, rate * Mathf.Max(0f, dt));

            if (direction.sqrMagnitude > .0025f)
            {
                float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                facingYaw = Mathf.MoveTowardsAngle(facingYaw, targetYaw, turnSpeed * Mathf.Max(0f, dt));
            }

            LocomotionState state = !input.Grounded
                ? LocomotionState.Fall
                : input.JumpPressed
                    ? LocomotionState.Jump
                    : input.Crouch && speed > .05f
                        ? LocomotionState.Crouch
                        : speed < .05f
                            ? LocomotionState.Idle
                            : input.Sprint
                                ? LocomotionState.Sprint
                                : magnitude < .55f
                                    ? LocomotionState.Walk
                                    : LocomotionState.Run;

            return new LocomotionFrame(direction.normalized, speed, facingYaw, state);
        }
    }
}
