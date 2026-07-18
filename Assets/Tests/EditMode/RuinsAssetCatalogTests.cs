using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class RuinsAssetCatalogTests
    {
        [Test]
        public void RequiredVisualAssets_ContainsTheSealedGate()
        {
            Assert.That(RuinsAssetCatalog.RequiredVisualAssets, Does.Contain(RuinsAssetId.Gate));
        }

        [TestCase(RuinsAssetId.Wall, "KenneyCastle/wall")]
        [TestCase(RuinsAssetId.Corner, "KenneyCastle/wall-corner")]
        [TestCase(RuinsAssetId.Pillar, "KenneyCastle/wall-pillar")]
        [TestCase(RuinsAssetId.Tower, "KenneyCastle/tower-square-mid-open")]
        [TestCase(RuinsAssetId.Gate, "KenneyCastle/gate")]
        public void CastleResources_UseStableResourcesPaths(RuinsAssetId assetId, string expectedPath)
        {
            Assert.That(RuinsAssetCatalog.GetPath(assetId), Is.EqualTo(expectedPath));
        }
    }
}
