using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class MainMenuCopyTests
    {
        [Test]
        public void ControlsCopy_ExplainsEveryRequiredAction()
        {
            string copy = MainMenuCopy.Controls;
            StringAssert.Contains("WASD", copy);
            StringAssert.Contains("SPACE", copy);
            StringAssert.Contains("C", copy);
            StringAssert.Contains("Q", copy);
            StringAssert.Contains("E", copy);
            StringAssert.Contains("ESC", copy);
        }

        [Test]
        public void MenuHeroCopy_DoesNotExposeCourseworkPlaceholder()
        {
            StringAssert.DoesNotContain("COURSEWORK BUILD", MainMenuCopy.Footer);
        }
    }
}
