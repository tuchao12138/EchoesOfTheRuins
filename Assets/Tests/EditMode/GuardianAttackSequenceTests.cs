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
        public void TargetLostAfterHitQueryCommitsPermanentlyToRecovery()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();

            AttackFrame strike = attack.Tick(.61f, true);
            AttackFrame lost = attack.Tick(.01f, false);
            AttackFrame reacquired = attack.Tick(.1f, true);
            AttackFrame finished = attack.Tick(.5f, true);

            Assert.That(strike.ShouldQueryHit, Is.True);
            Assert.That(lost.Phase, Is.EqualTo(GuardianAttackPhase.Recovery));
            Assert.That(lost.ShouldQueryHit, Is.False);
            Assert.That(reacquired.Phase, Is.EqualTo(GuardianAttackPhase.Recovery));
            Assert.That(reacquired.ShouldQueryHit, Is.False);
            Assert.That(finished.Phase, Is.EqualTo(GuardianAttackPhase.Recovery));
            Assert.That(finished.ShouldQueryHit, Is.False);
            Assert.That(finished.SequenceFinished, Is.True);
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

            AttackFrame strike = attack.Tick(.75f, true);
            AttackFrame finished = attack.Tick(.6f, true);

            Assert.That(strike.Phase, Is.EqualTo(GuardianAttackPhase.Strike));
            Assert.That(strike.ShouldQueryHit, Is.True);
            Assert.That(strike.SequenceFinished, Is.False);
            Assert.That(finished.Phase, Is.EqualTo(GuardianAttackPhase.Recovery));
            Assert.That(finished.SequenceFinished, Is.True);
        }

        [Test]
        public void HitchPastWholeSequenceStillQueriesExactlyOnce()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();

            AttackFrame hitch = attack.Tick(2f, true);
            AttackFrame afterHitch = attack.Tick(0f, true);

            Assert.That(hitch.Phase, Is.EqualTo(GuardianAttackPhase.Strike));
            Assert.That(hitch.ShouldQueryHit, Is.True);
            Assert.That(hitch.SequenceFinished, Is.False);
            Assert.That(afterHitch.ShouldQueryHit, Is.False);
            Assert.That(afterHitch.SequenceFinished, Is.True);
        }

        [Test]
        public void JustBeforeTelegraphBoundaryRemainsTelegraph()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();

            AttackFrame before = attack.Tick(.599999f, true);
            AttackFrame boundary = attack.Tick(.000001f, true);

            Assert.That(before.Phase, Is.EqualTo(GuardianAttackPhase.Telegraph));
            Assert.That(before.ShouldQueryHit, Is.False);
            Assert.That(boundary.ShouldQueryHit, Is.True);
        }

        [Test]
        public void JustBeforeStrikeBoundaryRemainsStrike()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();
            attack.Tick(.6f, true);

            AttackFrame before = attack.Tick(.149999f, true);

            Assert.That(before.Phase, Is.EqualTo(GuardianAttackPhase.Strike));
            Assert.That(before.ShouldQueryHit, Is.False);
        }

        [Test]
        public void JustBeforeRecoveryBoundaryDoesNotFinish()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();
            attack.Tick(.6f, true);
            attack.Tick(.15f, true);

            AttackFrame before = attack.Tick(.599999f, true);

            Assert.That(before.Phase, Is.EqualTo(GuardianAttackPhase.Recovery));
            Assert.That(before.SequenceFinished, Is.False);
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
