using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class QuaterniusRuinAssetCatalogTests
    {
        [TestCase(QuaterniusRuinAssetId.Wall, "QuaterniusRuins/Wall")]
        [TestCase(QuaterniusRuinAssetId.ArchGothic, "QuaterniusRuins/Arch_Gothic")]
        [TestCase(QuaterniusRuinAssetId.BrokenWall, "QuaterniusRuins/Wall_ArchRound_Broken")]
        [TestCase(QuaterniusRuinAssetId.AnimatedExplorer, "QuaterniusRuins/Character_Animated")]
        public void Resources_UseStablePathsForProductionRuinModules(QuaterniusRuinAssetId assetId, string expectedPath)
        {
            Assert.That(QuaterniusRuinAssetCatalog.GetPath(assetId), Is.EqualTo(expectedPath));
        }
    }
}
