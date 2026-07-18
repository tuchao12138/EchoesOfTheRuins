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
        private const float FloatMachineEpsilon = 1.1920929e-7f;
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
            if (!recovering && !HasReached(telegraph))
            {
                return new AttackFrame(GuardianAttackPhase.Telegraph, false, false);
            }

            if (!recovering && !queried)
            {
                if (!targetValid)
                {
                    elapsed = System.Math.Max(elapsed, telegraph + strike);
                    recovering = true;
                }
                else
                {
                    queried = true;
                    return new AttackFrame(GuardianAttackPhase.Strike, true, false);
                }
            }

            if (!recovering && !HasReached(telegraph + strike))
            {
                return new AttackFrame(
                    hit ? GuardianAttackPhase.HitConfirmed : GuardianAttackPhase.Strike,
                    false,
                    false);
            }

            bool finished = HasReached(telegraph + strike + recovery);
            if (finished)
            {
                active = false;
            }

            return new AttackFrame(
                hit ? GuardianAttackPhase.HitConfirmed : GuardianAttackPhase.Recovery,
                false,
                finished);
        }

        private bool HasReached(float boundary)
        {
            if (elapsed >= boundary)
            {
                return true;
            }

            float representationTolerance =
                FloatMachineEpsilon * System.Math.Max(1f, System.Math.Abs(boundary));
            return boundary - elapsed <= representationTolerance;
        }
    }
}
