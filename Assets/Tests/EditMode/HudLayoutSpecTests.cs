using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class HudLayoutSpecTests
    {
        [Test]
        public void RuntimeHud_UsesReadable1080pSizingWithoutLargeDebugPanels()
        {
            Assert.That(HudLayoutSpec.ReferenceWidth, Is.EqualTo(1920));
            Assert.That(HudLayoutSpec.ReferenceHeight, Is.EqualTo(1080));
            Assert.That(HudLayoutSpec.BodyFontSize, Is.GreaterThanOrEqualTo(18));
            Assert.That(HudLayoutSpec.ObjectivePanelWidth, Is.InRange(360, 460));
            Assert.That(HudLayoutSpec.ObjectivePanelHeight, Is.LessThanOrEqualTo(150));
            Assert.That(HudLayoutSpec.TutorialPanelWidth, Is.LessThanOrEqualTo(720));
        }

        [Test]
        public void HudUsesFourPersistentInformationZones()
        {
            Assert.That(HudLayoutSpec.PersistentZones, Is.EqualTo(4));
            Assert.That(HudLayoutSpec.ObjectivePanelWidth, Is.LessThanOrEqualTo(360));
            Assert.That(HudLayoutSpec.TutorialPanelHeight, Is.LessThanOrEqualTo(56));
        }
    }
}
