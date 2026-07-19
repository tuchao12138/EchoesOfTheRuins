using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Creates the continuous outer silhouette that hides the playable map boundary.</summary>
    public static class CitadelBackdropBuilder
    {
        public static GameObject Create(Material material)
        {
            GameObject root = new GameObject("Citadel Backdrop");

            Add(root.transform, "North Curtain", new Vector3(0f, 3.1f, 29.5f), new Vector3(60f, 6.2f, 1.4f), material);
            Add(root.transform, "South Curtain", new Vector3(0f, 3.1f, -29.5f), new Vector3(60f, 6.2f, 1.4f), material);
            Add(root.transform, "East Curtain", new Vector3(29.5f, 3.1f, 0f), new Vector3(1.4f, 6.2f, 60f), material);
            Add(root.transform, "West Curtain", new Vector3(-29.5f, 3.1f, 0f), new Vector3(1.4f, 6.2f, 60f), material);

            Add(root.transform, "North Crown Left", new Vector3(-17f, 7f, 29.5f), new Vector3(18f, 1.6f, 1.5f), material);
            Add(root.transform, "North Crown Right", new Vector3(17f, 7f, 29.5f), new Vector3(18f, 1.6f, 1.5f), material);
            Add(root.transform, "South Crown Left", new Vector3(-17f, 7f, -29.5f), new Vector3(18f, 1.6f, 1.5f), material);
            Add(root.transform, "South Crown Right", new Vector3(17f, 7f, -29.5f), new Vector3(18f, 1.6f, 1.5f), material);
            Add(root.transform, "East Crown Left", new Vector3(29.5f, 7f, -17f), new Vector3(1.5f, 1.6f, 18f), material);
            Add(root.transform, "East Crown Right", new Vector3(29.5f, 7f, 17f), new Vector3(1.5f, 1.6f, 18f), material);
            Add(root.transform, "West Crown Left", new Vector3(-29.5f, 7f, -17f), new Vector3(1.5f, 1.6f, 18f), material);
            Add(root.transform, "West Crown Right", new Vector3(-29.5f, 7f, 17f), new Vector3(1.5f, 1.6f, 18f), material);

            return root;
        }

        private static void Add(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            GameObject wall = MasonryWallBuilder.Create(name, position, size, material);
            wall.transform.SetParent(parent, true);
        }
    }
}
