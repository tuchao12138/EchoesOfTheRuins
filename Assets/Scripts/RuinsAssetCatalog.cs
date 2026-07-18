using System.Collections.Generic;

namespace EchoesOfTheRuins
{
    public enum RuinsAssetId
    {
        Wall,
        Corner,
        Pillar,
        Tower,
        Gate,
        Rocks,
        Stairs
    }

    /// <summary>Single source of truth for lightweight CC0 castle prefabs stored in Resources.</summary>
    public static class RuinsAssetCatalog
    {
        private static readonly RuinsAssetId[] VisualAssets =
        {
            RuinsAssetId.Wall,
            RuinsAssetId.Corner,
            RuinsAssetId.Pillar,
            RuinsAssetId.Tower,
            RuinsAssetId.Gate,
            RuinsAssetId.Rocks,
            RuinsAssetId.Stairs
        };

        public static IReadOnlyList<RuinsAssetId> RequiredVisualAssets => VisualAssets;

        public static string GetPath(RuinsAssetId assetId)
        {
            switch (assetId)
            {
                case RuinsAssetId.Wall: return "KenneyCastle/wall";
                case RuinsAssetId.Corner: return "KenneyCastle/wall-corner";
                case RuinsAssetId.Pillar: return "KenneyCastle/wall-pillar";
                case RuinsAssetId.Tower: return "KenneyCastle/tower-square-mid-open";
                case RuinsAssetId.Gate: return "KenneyCastle/gate";
                case RuinsAssetId.Rocks: return "KenneyCastle/rocks-large";
                case RuinsAssetId.Stairs: return "KenneyCastle/stairs-stone";
                default: return string.Empty;
            }
        }
    }
}
