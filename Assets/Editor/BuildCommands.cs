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
            if (!AssetDatabase.IsValidFolder(resourcesFolder)) AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder(materialsFolder)) AssetDatabase.CreateFolder(resourcesFolder, "Materials");
            Shader ruinsShader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            Shader hudShader = Shader.Find("UI/Default") ?? Shader.Find("Sprites/Default");
            if (ruinsShader == null || hudShader == null) throw new System.InvalidOperationException("No build-safe shader was found for a runtime material.");
            CreateMaterialIfMissing("Assets/Resources/Materials/RuinsRuntime.mat", "Ruins Runtime Template", ruinsShader);
            CreateMaterialIfMissing("Assets/Resources/Materials/HudRuntime.mat", "HUD Runtime Template", hudShader);
            AssetDatabase.SaveAssets();
        }

        private static void CreateMaterialIfMissing(string path, string materialName, Shader shader)
        {
            Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null)
            {
                if (existing.shader != shader)
                {
                    existing.shader = shader;
                    EditorUtility.SetDirty(existing);
                }
                return;
            }
            var material = new Material(shader) { name = materialName };
            AssetDatabase.CreateAsset(material, path);
        }
    }
}
