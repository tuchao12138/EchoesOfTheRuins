using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class HudCopyTests
    {
        [Test]
        public void MissionCopyIsEnglishAndExplainsTheExitInteraction()
        {
            Assert.That(HudCopy.EscapeObjective, Does.Contain("CYAN BEACON"));
            Assert.That(HudCopy.EscapeObjective, Does.Contain("PRESS E"));
            Assert.That(HudCopy.InteractionInterrupted, Does.Contain("BREAK LINE OF SIGHT"));
        }
    }
}
