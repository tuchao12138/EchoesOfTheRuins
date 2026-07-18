using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class GuardianAttackSequenceTests
    {
        [Test]
        public void TelegraphCannotHitBeforePointSixSeconds()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();

            AttackFrame frame = attack.Tick(.59f, true);

            Assert.That(frame.Phase, Is.EqualTo(GuardianAttackPhase.Telegraph));
            Assert.That(frame.ShouldQueryHit, Is.False);
        }

        [Test]
        public void StrikeQueriesHitExactlyOnce()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();

            AttackFrame first = attack.Tick(.61f, true);
            AttackFrame second = attack.Tick(.01f, true);

            Assert.That(first.ShouldQueryHit, Is.True);
            Assert.That(second.ShouldQueryHit, Is.False);
        }

        [Test]
        public void InvalidTargetDuringTelegraphProducesMiss()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();

            AttackFrame frame = attack.Tick(.61f, false);

            Assert.That(frame.ShouldQueryHit, Is.False);
            Assert.That(frame.Phase, Is.EqualTo(GuardianAttackPhase.Recovery));
        }

        [Test]
        public void RecoveryReportsWhenSequenceFinishes()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();

            AttackFrame recovery = attack.Tick(.75f, true);
            AttackFrame finished = attack.Tick(.6f, true);

            Assert.That(recovery.Phase, Is.EqualTo(GuardianAttackPhase.Recovery));
            Assert.That(recovery.SequenceFinished, Is.False);
            Assert.That(finished.SequenceFinished, Is.True);
        }

        [Test]
        public void ConfirmedHitDoesNotRequestAnotherHitQuery()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();
            AttackFrame strike = attack.Tick(.61f, true);

            attack.ConfirmHit();
            AttackFrame confirmed = attack.Tick(.01f, true);

            Assert.That(strike.ShouldQueryHit, Is.True);
            Assert.That(confirmed.Phase, Is.EqualTo(GuardianAttackPhase.HitConfirmed));
            Assert.That(confirmed.ShouldQueryHit, Is.False);
        }
    }
}
