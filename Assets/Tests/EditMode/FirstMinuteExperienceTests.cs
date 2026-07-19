using System.Linq;
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
            Assert.That(objectives.CurrentObjective, Is.EqualTo("COLLECT ENERGY CORES  2 / 3"));
            objectives.SetCollectedCores(3);
            Assert.That(objectives.CurrentObjective, Is.EqualTo("REACH THE UNSEALED EXIT"));
        }

        [Test]
        public void TutorialCopy_IsReadableEnglishForEveryStage()
        {
            foreach (TutorialStage stage in System.Enum.GetValues(typeof(TutorialStage)))
            {
                string copy = TutorialCopy.Get(stage);
                Assert.That(copy, Is.Not.Empty);
                Assert.That(copy.All(character => character <= 127), Is.True, $"{stage} contains a broken font character");
            }
        }
    }
}
