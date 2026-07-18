namespace EchoesOfTheRuins
{
    public enum GuardianAttackPhase
    {
        None,
        Telegraph,
        Strike,
        Recovery,
        HitConfirmed
    }

    public readonly struct AttackFrame
    {
        public readonly GuardianAttackPhase Phase;
        public readonly bool ShouldQueryHit;
        public readonly bool SequenceFinished;

        public AttackFrame(GuardianAttackPhase phase, bool shouldQueryHit, bool sequenceFinished)
        {
            Phase = phase;
            ShouldQueryHit = shouldQueryHit;
            SequenceFinished = sequenceFinished;
        }
    }

    public sealed class GuardianAttackSequence
    {
        private const float TimingEpsilon = .00001f;
        private readonly float telegraph;
        private readonly float strike;
        private readonly float recovery;
        private float elapsed;
        private bool active;
        private bool queried;
        private bool hit;
        private bool recovering;

        public GuardianAttackSequence(
            float telegraphSeconds = .6f,
            float strikeSeconds = .15f,
            float recoverySeconds = .6f)
        {
            telegraph = telegraphSeconds;
            strike = strikeSeconds;
            recovery = recoverySeconds;
        }

        public void Begin()
        {
            elapsed = 0f;
            active = true;
            queried = false;
            hit = false;
            recovering = false;
        }

        public void ConfirmHit()
        {
            hit = true;
        }

        public AttackFrame Tick(float deltaTime, bool targetValid)
        {
            if (!active)
            {
                return new AttackFrame(GuardianAttackPhase.None, false, false);
            }

            elapsed += System.Math.Max(0f, deltaTime);
            if (!recovering && elapsed + TimingEpsilon < telegraph)
            {
                return new AttackFrame(GuardianAttackPhase.Telegraph, false, false);
            }

            if (!recovering && !targetValid)
            {
                elapsed = telegraph + strike;
                recovering = true;
            }

            if (!recovering && elapsed + TimingEpsilon < telegraph + strike)
            {
                bool query = !queried;
                queried = true;
                return new AttackFrame(
                    hit ? GuardianAttackPhase.HitConfirmed : GuardianAttackPhase.Strike,
                    query,
                    false);
            }

            bool finished = elapsed + TimingEpsilon >= telegraph + strike + recovery;
            if (finished)
            {
                active = false;
            }

            return new AttackFrame(
                hit ? GuardianAttackPhase.HitConfirmed : GuardianAttackPhase.Recovery,
                false,
                finished);
        }
    }
}
