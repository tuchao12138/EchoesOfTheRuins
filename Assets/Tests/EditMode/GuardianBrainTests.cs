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

        [Test]
        public void Tick_VisualExposureBuildsSuspicionBeforeChase()
        {
            var brain = new GuardianBrain(detectionSeconds: 2f);

            GuardianState first = brain.Tick(1f, new GuardianPerception(.5f, false, false));
            GuardianState second = brain.Tick(3f, new GuardianPerception(.5f, false, false));

            Assert.That(first, Is.EqualTo(GuardianState.Investigate));
            Assert.That(brain.Suspicion, Is.EqualTo(1f).Within(.001f));
            Assert.That(second, Is.EqualTo(GuardianState.Chase));
        }

        [Test]
        public void Tick_LowerVisibilityBuildsSuspicionMoreSlowly()
        {
            var exposed = new GuardianBrain(detectionSeconds: 2f);
            var hidden = new GuardianBrain(detectionSeconds: 2f);

            exposed.Tick(1f, new GuardianPerception(1f, false, false));
            hidden.Tick(1f, new GuardianPerception(.25f, false, false));

            Assert.That(exposed.Suspicion, Is.GreaterThan(hidden.Suspicion));
        }

        [Test]
        public void Tick_NoVisualContactDecaysSuspicion()
        {
            var brain = new GuardianBrain(detectionSeconds: 2f, suspicionDecaySeconds: 1f);
            brain.Tick(1f, new GuardianPerception(1f, false, false));
            float before = brain.Suspicion;

            brain.Tick(.25f, new GuardianPerception(0f, false, false));

            Assert.That(brain.Suspicion, Is.LessThan(before));
        }
    }
}
