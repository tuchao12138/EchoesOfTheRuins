using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Instantiates authored CC0 ruin meshes as visuals while gameplay colliders stay separate.</summary>
    public static class ProductionRuinBuilder
    {
        public static GameObject CreateModule(
            string name,
            QuaterniusRuinAssetId assetId,
            Vector3 position,
            Quaternion rotation,
            Vector3 scale,
            Material material)
        {
            GameObject template = Resources.Load<GameObject>(QuaterniusRuinAssetCatalog.GetPath(assetId));
            if (template == null)
            {
                Debug.LogError($"Missing production ruin module: {QuaterniusRuinAssetCatalog.GetPath(assetId)}");
                return null;
            }

            GameObject module = Object.Instantiate(template, position, rotation);
            module.name = name;
            module.transform.localScale = scale;
            foreach (Renderer renderer in module.GetComponentsInChildren<Renderer>(true)) renderer.material = material;
            foreach (Collider collider in module.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            return module;
        }
    }
}
