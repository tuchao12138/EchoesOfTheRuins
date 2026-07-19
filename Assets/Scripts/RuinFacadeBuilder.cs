using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Creates visual-only ruin facades while navigation remains on separate invisible colliders.</summary>
    public static class RuinFacadeBuilder
    {
        public static GameObject CreateFacade(string name, Vector3 position, Quaternion rotation, Vector3 scale, Material material)
        {
            GameObject root = new GameObject(name);
            root.transform.SetPositionAndRotation(position, rotation);
            root.transform.localScale = scale;

            AddModule(root.transform, DungeonAssetId.Wall, Vector3.zero, material);
            AddModule(root.transform, DungeonAssetId.Corner, new Vector3(-.72f, 0f, .08f), material);
            AddModule(root.transform, DungeonAssetId.Corner, new Vector3(.72f, 0f, .08f), material);
            return root;
        }

        private static void AddModule(Transform parent, DungeonAssetId assetId, Vector3 localPosition, Material material)
        {
            GameObject template = Resources.Load<GameObject>(DungeonAssetCatalog.GetPath(assetId));
            if (template == null)
            {
                Debug.LogError($"Missing ruin facade module: {DungeonAssetCatalog.GetPath(assetId)}");
                return;
            }

            GameObject module = Object.Instantiate(template, parent);
            module.transform.localPosition = localPosition;
            module.transform.localRotation = Quaternion.identity;
            foreach (Renderer renderer in module.GetComponentsInChildren<Renderer>(true)) renderer.material = material;
            foreach (Collider collider in module.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        }
    }
}
