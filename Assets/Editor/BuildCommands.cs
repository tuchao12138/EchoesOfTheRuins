using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoesOfTheRuins.Editor
{
    public static class BuildCommands
    {
        [MenuItem("Echoes/Build Windows Player")]
        public static void BuildWindowsPlayer()
        {
            const string scene = "Assets/Scenes/ProductionRuins.unity";
            if (!File.Exists(scene))
            {
                Debug.LogError("Production scene is missing: " + scene);
                return;
            }

            EnsureRuntimeMaterial();

            Directory.CreateDirectory("Builds");
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { scene },
                locationPathName = "Builds/EchoesOfTheRuins.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("Windows build failed: " + report.summary.result);
                return;
            }

            Debug.Log("Windows build complete: " + report.summary.outputPath);
        }

        private static void EnsureRuntimeMaterial()
        {
            const string resourcesFolder = "Assets/Resources";
            const string materialsFolder = "Assets/Resources/Materials";
            const string materialPath = "Assets/Resources/Materials/RuinsRuntime.mat";
            if (!AssetDatabase.IsValidFolder(resourcesFolder)) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(materialsFolder)) AssetDatabase.CreateFolder(resourcesFolder, "Materials");
            if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) != null) return;

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            if (shader == null) throw new System.InvalidOperationException("No build-safe shader was found for the runtime ruins material.");
            var material = new Material(shader) { name = "Ruins Runtime Template" };
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
        }
    }
}
