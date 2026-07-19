namespace EchoesOfTheRuins
{
    public enum DungeonAssetId
    {
        Wall,
        Corner,
        Gate,
        Stairs,
        Room,
        Pillar
    }

    /// <summary>Stable Resources paths for the CC0 Kenney Modular Dungeon Kit.</summary>
    public static class DungeonAssetCatalog
    {
        public static string GetPath(DungeonAssetId assetId)
        {
            switch (assetId)
            {
                case DungeonAssetId.Wall: return "KenneyDungeon/template-wall-detail-a";
                case DungeonAssetId.Corner: return "KenneyDungeon/template-wall-corner";
                case DungeonAssetId.Gate: return "KenneyDungeon/gate-door";
                case DungeonAssetId.Stairs: return "KenneyDungeon/stairs-wide";
                case DungeonAssetId.Room: return "KenneyDungeon/room-large-variation";
                case DungeonAssetId.Pillar: return "KenneyDungeon/template-detail";
                default: return string.Empty;
            }
        }
    }
}
