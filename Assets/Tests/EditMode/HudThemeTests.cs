using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests.EditMode
{
    public sealed class HudThemeTests
    {
        [Test]
        public void ChaseAlert_UsesRedDetectedCopy()
        {
            HudAlert alert = HudTheme.GetAlert(GuardianState.Chase);
            Assert.That(alert.Message, Is.EqualTo("DETECTED - BREAK LINE OF SIGHT"));
            Assert.That(alert.Color.r, Is.GreaterThan(.9f));
            Assert.That(alert.Color.g, Is.LessThan(.4f));
        }
    }
}
