namespace EchoesOfTheRuins
{
    public enum QuaterniusRuinAssetId
    {
        Wall,
        ArchGothic,
        ArchRound,
        BrokenWall,
        OvergrownWall,
        DoubleHoleWall,
        SquareColumn,
        RoundColumn,
        DoubleWindow,
        AnimatedExplorer
    }

    /// <summary>Stable Resources paths for the Quaternius Ultimate Modular Ruins CC0 pack.</summary>
    public static class QuaterniusRuinAssetCatalog
    {
        public static string GetPath(QuaterniusRuinAssetId assetId)
        {
            switch (assetId)
            {
                case QuaterniusRuinAssetId.Wall: return "QuaterniusRuins/Wall";
                case QuaterniusRuinAssetId.ArchGothic: return "QuaterniusRuins/Arch_Gothic";
                case QuaterniusRuinAssetId.ArchRound: return "QuaterniusRuins/Arch_Round";
                case QuaterniusRuinAssetId.BrokenWall: return "QuaterniusRuins/Wall_ArchRound_Broken";
                case QuaterniusRuinAssetId.OvergrownWall: return "QuaterniusRuins/Wall_ArchRound_Overgrown";
                case QuaterniusRuinAssetId.DoubleHoleWall: return "QuaterniusRuins/Wall_Double_Hole";
                case QuaterniusRuinAssetId.SquareColumn: return "QuaterniusRuins/Column_Square";
                case QuaterniusRuinAssetId.RoundColumn: return "QuaterniusRuins/Column_Round";
                case QuaterniusRuinAssetId.DoubleWindow: return "QuaterniusRuins/Window_Open_Double";
                case QuaterniusRuinAssetId.AnimatedExplorer: return "QuaterniusRuins/Character_Animated";
                default: return string.Empty;
            }
        }
    }
}
