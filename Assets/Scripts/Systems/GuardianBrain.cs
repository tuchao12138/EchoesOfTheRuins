namespace EchoesOfTheRuins
{
    public enum GuardianState
    {
        Patrol,
        Investigate,
        Search,
        Chase,
        AttackTelegraph,
        Strike,
        Recovery,
        Capture
    }

    public readonly struct GuardianPerception
    {
        public readonly float VisualExposure;
        public readonly bool ImmediateDetection;
        public readonly bool HeardNoise;
        public readonly bool InAttackRange;

        public GuardianPerception(bool canSeePlayer, bool heardNoise, bool inAttackRange)
        {
            VisualExposure = canSeePlayer ? 1f : 0f;
            ImmediateDetection = canSeePlayer;
            HeardNoise = heardNoise;
            InAttackRange = inAttackRange;
        }

        public GuardianPerception(float visualExposure, bool heardNoise, bool inAttackRange)
        {
            VisualExposure = UnityEngine.Mathf.Clamp01(visualExposure);
            ImmediateDetection = false;
            HeardNoise = heardNoise;
            InAttackRange = inAttackRange;
        }
    }

    /// <summary>Deterministic stealth decision layer. MonoBehaviours supply perception and move the agent.</summary>
    public sealed class GuardianBrain
    {
        private readonly float searchDuration;
        private readonly float detectionSeconds;
        private readonly float suspicionDecaySeconds;
        private float suspicionGainMultiplier = 1f;
        private float suspicionDecayMultiplier = 1f;
        private float searchRemaining;

        public GuardianState State { get; private set; } = GuardianState.Patrol;
        public float Suspicion { get; private set; }

        public GuardianBrain(float searchDuration = 4f, float detectionSeconds = 1.4f, float suspicionDecaySeconds = 2.2f)
        {
            this.searchDuration = searchDuration;
            this.detectionSeconds = UnityEngine.Mathf.Max(.1f, detectionSeconds);
            this.suspicionDecaySeconds = UnityEngine.Mathf.Max(.1f, suspicionDecaySeconds);
        }

        public GuardianState Tick(float deltaTime, GuardianPerception perception)
        {
            if (State == GuardianState.Chase && perception.InAttackRange)
            {
                State = GuardianState.AttackTelegraph;
                return State;
            }

            if (perception.ImmediateDetection)
            {
                Suspicion = 1f;
                searchRemaining = searchDuration;
                State = GuardianState.Chase;
                return State;
            }

            if (perception.VisualExposure > 0f)
            {
                Suspicion = UnityEngine.Mathf.Clamp01(
                    Suspicion + deltaTime * perception.VisualExposure / detectionSeconds * suspicionGainMultiplier);
                searchRemaining = searchDuration;
                State = Suspicion >= .999f || State == GuardianState.Chase
                    ? GuardianState.Chase
                    : GuardianState.Investigate;
                return State;
            }

            if (perception.HeardNoise)
            {
                searchRemaining = searchDuration;
                State = GuardianState.Investigate;
                return State;
            }

            Suspicion = UnityEngine.Mathf.Clamp01(Suspicion - deltaTime / suspicionDecaySeconds * suspicionDecayMultiplier);

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

        public void ConfigurePressure(float gainMultiplier, float decayMultiplier)
        {
            suspicionGainMultiplier = UnityEngine.Mathf.Max(.1f, gainMultiplier);
            suspicionDecayMultiplier = UnityEngine.Mathf.Max(.1f, decayMultiplier);
        }

        public void ForceSearch()
        {
            searchRemaining = searchDuration;
            State = GuardianState.Search;
        }
    }
}
