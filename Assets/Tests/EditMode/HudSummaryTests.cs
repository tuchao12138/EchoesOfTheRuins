using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class HudSummaryTests
    {
        [Test]
        public void ResultCopyUsesAReadableEnglishEscapeHeading()
        {
            RunStats stats = new RunStats();
            string result = HudCopy.ResultSummary("A", 1200, stats);

            Assert.That(result, Does.StartWith("ESCAPED THE RUINS"));
            Assert.That(result, Does.Contain("RELICS  0/2"));
            Assert.That(result, Does.Not.Contain("成功"));
        }
    }
}
