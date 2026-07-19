using UnityEngine;

namespace EchoesOfTheRuins
{
    /// <summary>Keeps a shader-referencing material inside Resources so player builds never rely on stripped Shader.Find calls.</summary>
    public static class RuntimeMaterialLibrary
    {
        private const string TemplatePath = "Materials/RuinsRuntime";

        public static Material Create(string materialName, Color color, bool emissive)
        {
            Material template = Resources.Load<Material>(TemplatePath);
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            Material material;
            if (template != null)
            {
                material = Object.Instantiate(template);
                if (urpLit != null) material.shader = urpLit;
            }
            else
            {
                Shader shader = urpLit ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default") ?? Shader.Find("Hidden/InternalErrorShader");
                material = new Material(shader);
            }

            material.name = materialName;
            material.color = color;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (emissive && material.HasProperty("_EmissionColor"))
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * 1.5f);
            }
            return material;
        }

        /// <summary>Creates a build-safe URP material and explicitly binds an imported texture.</summary>
        public static Material CreateTextured(string materialName, Texture texture)
        {
            Material material = Create(materialName, Color.white, false);
            material.mainTexture = texture;
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            return material;
        }
    }
}
