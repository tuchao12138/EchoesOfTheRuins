using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ObjectiveTrackerTests
    {
        [Test]
        public void NewRunProgressesThroughFirstMinuteInOrder()
        {
            var tracker = new ObjectiveTracker();

            Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Briefing));
            tracker.Notify(ObjectiveSignal.BriefingFinished);
            Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Move));
            tracker.Notify(ObjectiveSignal.MovedAndSprinted);
            Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Observe));
            tracker.Notify(ObjectiveSignal.ObservedGuardian);
            Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Hide));
            tracker.Notify(ObjectiveSignal.CrouchedInShadow);
            Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Distract));
            tracker.Notify(ObjectiveSignal.UsedEchoStone);
            Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.CollectCores));
        }

        [Test]
        public void IgnoreInvalidSignalsAndPublishOnlyOnValidAdvance()
        {
            var tracker = new ObjectiveTracker();
            int changes = 0;
            tracker.Changed += _ => changes++;

            tracker.Notify(ObjectiveSignal.UsedEchoStone);
            Assert.That(changes, Is.EqualTo(0));
            tracker.Notify(ObjectiveSignal.BriefingFinished);
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void Tracker_ExposesOnlyTheChangedEventContract()
        {
            Assert.That(typeof(ObjectiveTracker).GetEvent("Changed"), Is.Not.Null);
            Assert.That(typeof(ObjectiveTracker).GetEvent("ObjectiveChanged"), Is.Null);
        }

        [TestCase(0, "CORE I", "COURTYARD")]
        [TestCase(1, "CORE II", "SHADOW GALLERY")]
        [TestCase(2, "CORE III", "ALTAR")]
        public void CoreObjectives_NameTheNextRouteDestination(int collectedCores, string coreNumber, string destination)
        {
            var tracker = new ObjectiveTracker(ObjectiveStage.CollectCores, collectedCores);

            Assert.That(tracker.Current.Title, Does.Contain(coreNumber));
            Assert.That(tracker.Current.Title, Does.Contain(destination));
            Assert.That(tracker.Current.TargetLabel, Does.Contain(destination));
        }

        [TestCase(0, "entrance", ObjectiveStage.Briefing)]
        [TestCase(1, "courtyard", ObjectiveStage.CollectCores)]
        [TestCase(3, "altar", ObjectiveStage.ReachExit)]
        public void V3MigrationDerivesSafeObjective(int cores, string checkpoint, ObjectiveStage expected)
        {
            SaveData migrated = SaveDataMigrator.MigrateFromV3(cores, checkpoint, false);

            Assert.That(migrated.Version, Is.EqualTo(4));
            Assert.That(migrated.ObjectiveStage, Is.EqualTo(expected.ToString()));
        }
    }
}
