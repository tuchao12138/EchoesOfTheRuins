using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class FirstMinuteExperienceTests
    {
        [Test]
        public void Tutorial_AdvancesOnlyWhenCurrentActionIsCompleted()
        {
            var tutorial = new TutorialProgress();

            Assert.That(tutorial.Stage, Is.EqualTo(TutorialStage.Objective));
            tutorial.NotifyEchoStoneUsed();
            Assert.That(tutorial.Stage, Is.EqualTo(TutorialStage.Objective));

            tutorial.ContinueFromObjective();
            Assert.That(tutorial.Stage, Is.EqualTo(TutorialStage.Movement));
            tutorial.NotifyMoved();
            Assert.That(tutorial.Stage, Is.EqualTo(TutorialStage.Shadow));
            tutorial.NotifyCrouchedInShadow();
            Assert.That(tutorial.Stage, Is.EqualTo(TutorialStage.EchoStone));
            tutorial.NotifyEchoStoneUsed();
            Assert.That(tutorial.Stage, Is.EqualTo(TutorialStage.Awareness));
            tutorial.ContinueFromAwareness();
            Assert.That(tutorial.Stage, Is.EqualTo(TutorialStage.Complete));
        }

        [Test]
        public void SpawnProtection_PreventsDetectionForEightSeconds()
        {
            var protection = new SpawnProtection(8f);

            Assert.That(protection.CanBeDetected, Is.False);
            protection.Tick(7.9f);
            Assert.That(protection.CanBeDetected, Is.False);
            protection.Tick(0.1f);
            Assert.That(protection.CanBeDetected, Is.True);
        }

        [Test]
        public void ObjectiveTracker_ChangesFromCoresToExitAtThreeOfThree()
        {
            var objectives = new ObjectiveTrackerModel(3);

            objectives.SetCollectedCores(2);
            Assert.That(objectives.CurrentObjective, Does.Contain("2/3"));
            objectives.SetCollectedCores(3);
            Assert.That(objectives.CurrentObjective, Does.Contain("出口"));
        }
    }
}
