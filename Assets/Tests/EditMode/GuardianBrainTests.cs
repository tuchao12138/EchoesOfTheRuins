using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class GuardianBrainTests
    {
        [Test]
        public void Tick_HeardEcho_TransitionsFromPatrolToInvestigate()
        {
            var brain = new GuardianBrain();

            var state = brain.Tick(0.1f, new GuardianPerception(false, true, false));

            Assert.That(state, Is.EqualTo(GuardianState.Investigate));
        }

        [Test]
        public void Tick_VisiblePlayer_TransitionsToChase()
        {
            var brain = new GuardianBrain();

            var state = brain.Tick(0.1f, new GuardianPerception(true, false, false));

            Assert.That(state, Is.EqualTo(GuardianState.Chase));
        }

        [Test]
        public void Tick_LostSight_SearchesThenReturnsToPatrol()
        {
            var brain = new GuardianBrain(searchDuration: 1f);
            brain.Tick(0.1f, new GuardianPerception(true, false, false));

            Assert.That(brain.Tick(0.1f, new GuardianPerception(false, false, false)), Is.EqualTo(GuardianState.Search));
            Assert.That(brain.Tick(1.1f, new GuardianPerception(false, false, false)), Is.EqualTo(GuardianState.Patrol));
        }

        [Test]
        public void Tick_AttackRangeFromChase_TransitionsToTelegraphInsteadOfCapture()
        {
            var brain = new GuardianBrain();
            brain.Tick(0.1f, new GuardianPerception(true, false, false));

            var state = brain.Tick(0.1f, new GuardianPerception(true, false, true));

            Assert.That(state, Is.EqualTo(GuardianState.AttackTelegraph));
        }

        [Test]
        public void Tick_AttackRangeFromPatrol_DoesNotSkipToCaptureOrTelegraph()
        {
            var brain = new GuardianBrain();

            var state = brain.Tick(0.1f, new GuardianPerception(false, false, true));

            Assert.That(state, Is.EqualTo(GuardianState.Patrol));
        }
    }
}
