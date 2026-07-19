using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class DungeonAssetCatalogTests
    {
        [TestCase(DungeonAssetId.Wall, "KenneyDungeon/template-wall-detail-a")]
        [TestCase(DungeonAssetId.Corner, "KenneyDungeon/template-wall-corner")]
        [TestCase(DungeonAssetId.Gate, "KenneyDungeon/gate-door")]
        [TestCase(DungeonAssetId.Stairs, "KenneyDungeon/stairs-wide")]
        public void Resources_UseStablePathsForDungeonModules(DungeonAssetId assetId, string expectedPath)
        {
            Assert.That(DungeonAssetCatalog.GetPath(assetId), Is.EqualTo(expectedPath));
        }
    }
}
