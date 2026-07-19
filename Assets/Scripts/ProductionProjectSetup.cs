#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EchoesOfTheRuins
{
    /// <summary>Idempotent Unity 6.3 URP configuration used by editor, CI and coursework builds.</summary>
    public static class ProductionProjectSetup
    {
        public const string PipelineAssetPath = "Assets/Settings/MoonlitRuinsPipeline.asset";
        public const string RendererAssetPath = "Assets/Settings/MoonlitRuinsRenderer.asset";

        [MenuItem("Echoes/Apply URP Project Setup")]
        public static void Apply()
        {
            EnsureFolder("Assets/Settings");
            UniversalRendererData renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererAssetPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.name = "Moonlit Ruins Renderer";
                AssetDatabase.CreateAsset(renderer, RendererAssetPath);
            }

            UniversalRenderPipelineAsset pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "Moonlit Ruins Pipeline";
                AssetDatabase.CreateAsset(pipeline, PipelineAssetPath);
            }

            pipeline.supportsHDR = true;
            pipeline.msaaSampleCount = 4;
            pipeline.renderScale = 1f;
            pipeline.shadowDistance = 55f;
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64,
                new[] { GraphicsDeviceType.Direct3D11 });
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(pipeline);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            AssetDatabase.CreateFolder("Assets", "Settings");
        }
    }
}
#endif
