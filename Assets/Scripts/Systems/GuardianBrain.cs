namespace EchoesOfTheRuins
{
    public enum GuardianState
    {
        Patrol,
        Investigate,
        Search,
        Chase,
        Capture
    }

    public readonly struct GuardianPerception
    {
        public readonly bool CanSeePlayer;
        public readonly bool HeardNoise;
        public readonly bool InCaptureRange;

        public GuardianPerception(bool canSeePlayer, bool heardNoise, bool inCaptureRange)
        {
            CanSeePlayer = canSeePlayer;
            HeardNoise = heardNoise;
            InCaptureRange = inCaptureRange;
        }
    }

    /// <summary>Deterministic stealth decision layer. MonoBehaviours supply perception and move the agent.</summary>
    public sealed class GuardianBrain
    {
        private readonly float searchDuration;
        private float searchRemaining;

        public GuardianState State { get; private set; } = GuardianState.Patrol;

        public GuardianBrain(float searchDuration = 4f)
        {
            this.searchDuration = searchDuration;
        }

        public GuardianState Tick(float deltaTime, GuardianPerception perception)
        {
            if (perception.InCaptureRange)
            {
                State = GuardianState.Capture;
                return State;
            }

            if (perception.CanSeePlayer)
            {
                searchRemaining = searchDuration;
                State = GuardianState.Chase;
                return State;
            }

            if (perception.HeardNoise)
            {
                searchRemaining = searchDuration;
                State = GuardianState.Investigate;
                return State;
            }

            if (State == GuardianState.Chase || State == GuardianState.Investigate || State == GuardianState.Search)
            {
                searchRemaining -= deltaTime;
                State = searchRemaining > 0f ? GuardianState.Search : GuardianState.Patrol;
            }
            else
            {
                State = GuardianState.Patrol;
            }

            return State;
        }
    }
}
