using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class RunMetricsTrackerTests
    {
        [Test]
        public void GuardianState_CountsOnlyNewChaseTransitions()
        {
            var tracker = new RunMetricsTracker();

            tracker.RecordGuardianState("guardian-a", GuardianState.Investigate);
            tracker.RecordGuardianState("guardian-a", GuardianState.Chase);
            tracker.RecordGuardianState("guardian-a", GuardianState.Chase);
            tracker.RecordGuardianState("guardian-a", GuardianState.Search);
            tracker.RecordGuardianState("guardian-a", GuardianState.Chase);

            Assert.That(tracker.Alerts, Is.EqualTo(2));
        }

        [Test]
        public void EchoAndCaptureEvents_AreRecordedForScore()
        {
            var tracker = new RunMetricsTracker();

            tracker.RecordEchoStone();
            tracker.RecordEchoStone();
            tracker.RecordCapture();

            Assert.That(tracker.EchoStonesUsed, Is.EqualTo(2));
            Assert.That(tracker.Captures, Is.EqualTo(1));
        }
    }
}
