using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Authored decorative clusters that break up the courtyard without changing gameplay collision.</summary>
    public static class CourtyardSetDressingBuilder
    {
        public static GameObject Create(Material stone, Material shadowStone)
        {
            GameObject root = new GameObject("Courtyard Set Dressing");

            Add(root, "Collapsed Column West", QuaterniusRuinAssetId.SquareColumn,
                new Vector3(-8.8f, .45f, -3.8f), Quaternion.Euler(0f, 25f, 82f), Vector3.one * 1.15f, stone);
            Add(root, "Collapsed Column East", QuaterniusRuinAssetId.RoundColumn,
                new Vector3(8.4f, .4f, 4.8f), Quaternion.Euler(0f, -35f, 86f), Vector3.one, shadowStone);
            Add(root, "Broken Wall Cover West", QuaterniusRuinAssetId.BrokenWall,
                new Vector3(-9.2f, 0f, 5f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.22f, stone);
            Add(root, "Overgrown Wall Cover East", QuaterniusRuinAssetId.OvergrownWall,
                new Vector3(9.1f, 0f, 8.2f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.18f, shadowStone);
            Add(root, "North Window Ruin West", QuaterniusRuinAssetId.DoubleWindow,
                new Vector3(-5.6f, 0f, 14.9f), Quaternion.identity, Vector3.one * 1.28f, stone);
            Add(root, "North Window Ruin East", QuaterniusRuinAssetId.DoubleHoleWall,
                new Vector3(5.6f, 0f, 14.9f), Quaternion.identity, Vector3.one * 1.28f, stone);
            Add(root, "Shadow Gallery Marker", QuaterniusRuinAssetId.ArchRound,
                new Vector3(-12.2f, 0f, 1.2f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.12f, shadowStone);
            Add(root, "Altar Approach Marker", QuaterniusRuinAssetId.ArchGothic,
                new Vector3(12.1f, 0f, 6.8f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.18f, stone);

            return root;
        }

        private static void Add(GameObject root, string name, QuaterniusRuinAssetId assetId,
            Vector3 position, Quaternion rotation, Vector3 scale, Material material)
        {
            GameObject module = ProductionRuinBuilder.CreateModule(name, assetId, position, rotation, scale, material);
            if (module != null) module.transform.SetParent(root.transform, true);
        }
    }
}
