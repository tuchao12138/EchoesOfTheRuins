namespace EchoesOfTheRuins
{
    public enum CharacterAssetId
    {
        Explorer,
        Guardian
    }

    /// <summary>Stable Resources paths for the CC0 rigged character models used by gameplay.</summary>
    public static class CharacterAssetCatalog
    {
        public static string GetPath(CharacterAssetId assetId)
        {
            switch (assetId)
            {
                case CharacterAssetId.Explorer: return "Characters/Explorer";
                case CharacterAssetId.Guardian: return "Characters/Guardian";
                default: return string.Empty;
            }
        }

        public static string GetTexturePath(CharacterAssetId assetId)
        {
            switch (assetId)
            {
                case CharacterAssetId.Explorer: return "Characters/Explorer_Texture";
                case CharacterAssetId.Guardian: return "Characters/Guardian_Texture";
                default: return string.Empty;
            }
        }
    }
}
