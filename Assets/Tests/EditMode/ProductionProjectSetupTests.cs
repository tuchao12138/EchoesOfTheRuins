#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace EchoesOfTheRuins.Tests
{
    public sealed class ProductionProjectSetupTests
    {
        [Test]
        public void Apply_CreatesAndAssignsUrpPipelineAsset()
        {
            ProductionProjectSetup.Apply();

            UniversalRenderPipelineAsset asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(ProductionProjectSetup.PipelineAssetPath);
            Assert.That(asset, Is.Not.Null);
            Assert.That(GraphicsSettings.defaultRenderPipeline, Is.SameAs(asset));
            Assert.That(asset.supportsHDR, Is.True);
            Assert.That(asset.msaaSampleCount, Is.EqualTo(4));
        }

        [Test]
        public void Apply_UsesStableD3D11ForWindowsCourseworkBuild()
        {
            ProductionProjectSetup.Apply();

            Assert.That(PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64), Is.False);
            Assert.That(PlayerSettings.GetGraphicsAPIs(BuildTarget.StandaloneWindows64),
                Is.EqualTo(new[] { GraphicsDeviceType.Direct3D11 }));
        }
    }
}
#endif
