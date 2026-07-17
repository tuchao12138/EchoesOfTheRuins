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
    }
}
