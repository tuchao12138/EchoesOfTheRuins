using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ThreatCoordinatorTests
    {
        [Test]
        public void HighestSuspicionGuardianWinsAndAttackBreaksTies()
        {
            var model = new ThreatModel();
            model.Update("A", GuardianState.Investigate, .7f, GuardianAttackPhase.None, Vector3.left);
            model.Update("B", GuardianState.Chase, .7f, GuardianAttackPhase.Telegraph, Vector3.right);
            Assert.That(model.Current.GuardianId, Is.EqualTo("B"));
            Assert.That(model.Current.AttackPhase, Is.EqualTo(GuardianAttackPhase.Telegraph));
        }

        [Test]
        public void AttackPriorityWinsBeforeAiPriorityAndSuspicion()
        {
            var model = new ThreatModel();
            model.Update("searching", GuardianState.Search, 1f, GuardianAttackPhase.None, Vector3.forward);
            model.Update("striking", GuardianState.Patrol, 0f, GuardianAttackPhase.Strike, Vector3.left);
            Assert.That(model.Current.GuardianId, Is.EqualTo("striking"));
        }

        [Test]
        public void UpdateOnlyPublishesWhenVisibleThreatChanges()
        {
            var model = new ThreatModel();
            int changes = 0;
            model.Changed += _ => changes++;
            model.Update("A", GuardianState.Patrol, .1f, GuardianAttackPhase.None, Vector3.forward);
            model.Update("A", GuardianState.Patrol, .1f, GuardianAttackPhase.None, Vector3.forward);
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void RemovingSelectedGuardianPublishesRemainingGuardian()
        {
            var model = new ThreatModel();
            model.Update("patrol", GuardianState.Patrol, .2f, GuardianAttackPhase.None, Vector3.forward);
            model.Update("attacker", GuardianState.Chase, .8f, GuardianAttackPhase.Telegraph, Vector3.left);

            model.Remove("attacker");

            Assert.That(model.Current.GuardianId, Is.EqualTo("patrol"));
            Assert.That(model.Current.AttackPhase, Is.EqualTo(GuardianAttackPhase.None));
        }

        [Test]
        public void RemovingLastGuardianClearsThreatSnapshot()
        {
            var model = new ThreatModel();
            model.Update("only", GuardianState.Chase, .9f, GuardianAttackPhase.None, Vector3.forward);

            model.Remove("only");

            Assert.That(model.Current.GuardianId, Is.Empty);
            Assert.That(model.Current.Suspicion, Is.Zero);
        }
    }
}
