using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class RunFlowAndClarityTests
    {
        [Test]
        public void ThreeCoresEnterEscapeButDoNotCompleteRun()
        {
            var flow = new RunFlowState();

            flow.BeginCoreHunt();
            flow.UpdateCoreProgress(3, 3);

            Assert.That(flow.Phase, Is.EqualTo(RunPhase.Escape));
            Assert.That(flow.HasCompleted, Is.False);
        }

        [Test]
        public void EscapeCanOnlyCompleteAfterAllCores()
        {
            var flow = new RunFlowState();
            flow.BeginCoreHunt();

            Assert.That(flow.TryCompleteEscape(), Is.False);

            flow.UpdateCoreProgress(3, 3);

            Assert.That(flow.TryCompleteEscape(), Is.True);
            Assert.That(flow.TryCompleteEscape(), Is.False);
            Assert.That(flow.Phase, Is.EqualTo(RunPhase.Complete));
        }

        [Test]
        public void ExitPromptExplainsLockedAndUnlockedStates()
        {
            Assert.That(ExitGatePresentation.GetPrompt(false, 2), Does.Contain("2").And.Contain("CORES"));
            Assert.That(ExitGatePresentation.GetPrompt(true, 0), Does.Contain("ESCAPE").And.Contain("E"));
        }

        [Test]
        public void CoreActivationResetsWhenGuardianChases()
        {
            var activation = new CoreActivationModel(2f);

            Assert.That(activation.Tick(1.25f, true, false), Is.False);
            Assert.That(activation.Progress01, Is.GreaterThan(.5f));
            activation.Tick(.1f, true, true);

            Assert.That(activation.Progress01, Is.Zero);
            Assert.That(activation.Tick(2f, true, false), Is.True);
        }

        [Test]
        public void MotionClarityDefaultsAvoidTemporalSmearing()
        {
            Assert.That(MotionClarityProfile.UseTemporalAntialiasing, Is.False);
            Assert.That(MotionClarityProfile.UseMotionBlur, Is.False);
            Assert.That(MotionClarityProfile.UseFilmGrain, Is.False);
            Assert.That(MotionClarityProfile.FloorTextureTiling, Is.EqualTo(7f));
            Assert.That(MotionClarityProfile.TargetFrameRate, Is.EqualTo(60));
            Assert.That(MotionClarityProfile.AnisotropicLevel, Is.GreaterThanOrEqualTo(8));
        }

        [Test]
        public void GuardianPressureEscalatesWithoutMakingEscapeAnInstantFailure()
        {
            GuardianDifficultyProfile hunt = GuardianDifficultyProfile.For(RunPhase.CoreHunt, 1);
            GuardianDifficultyProfile escape = GuardianDifficultyProfile.For(RunPhase.Escape, 3);

            Assert.That(escape.PatrolSpeedMultiplier, Is.GreaterThan(hunt.PatrolSpeedMultiplier));
            Assert.That(escape.ChaseSpeedMultiplier, Is.GreaterThan(hunt.ChaseSpeedMultiplier));
            Assert.That(escape.DetectionRangeMultiplier, Is.GreaterThan(hunt.DetectionRangeMultiplier));
            Assert.That(escape.SuspicionGainMultiplier, Is.GreaterThan(hunt.SuspicionGainMultiplier));
            Assert.That(escape.SuspicionDecayMultiplier, Is.LessThan(hunt.SuspicionDecayMultiplier));
        }

        [Test]
        public void RelicsAreOptionalAndOnlyCountOnce()
        {
            var relics = new RelicProgressState();

            Assert.That(relics.Collect("moon-tablet"), Is.True);
            Assert.That(relics.Collect("moon-tablet"), Is.False);
            Assert.That(relics.Count, Is.EqualTo(1));
        }
    }
}
