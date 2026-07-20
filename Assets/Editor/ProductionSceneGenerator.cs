#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace EchoesOfTheRuins.Editor
{
    /// <summary>Creates release scenes. Gameplay is assembled at runtime to keep the player scene build-safe.</summary>
    public static class ProductionSceneGenerator
    {
        public const string ProductionScenePath = "Assets/Scenes/ProductionRuins.unity";
        public const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string MaterialFolder = "Assets/Settings/Materials";
        private const string VolumeProfilePath = "Assets/Settings/MoonlitRuinsVolumeProfile.asset";

        [MenuItem("Echoes/Generate Production Ruins Scene")]
        public static void Generate()
        {
            ProductionProjectSetup.Apply();
            EnsureFolder("Assets/Scenes");
            EnsureFolder(MaterialFolder);

            // Do not serialize the generated hierarchy into level1. Unity 6 can produce an unreadable
            // player data file for this large, dynamically-created scene. RuinSceneBootstrap builds the
            // same hierarchy after this intentionally empty scene has loaded in the player.
            Scene gameplayBootstrap = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!EditorSceneManager.SaveScene(gameplayBootstrap, ProductionScenePath))
                throw new IOException($"Could not save {ProductionScenePath}");

            GenerateMainMenuScene();
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(ProductionScenePath, true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Generated build-safe runtime gameplay bootstrap: {ProductionScenePath}");
        }

        private static void GenerateMainMenuScene()
        {
            Scene menu = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/MoonlitCitadelSky.mat");
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.11f, .16f, .28f);
            RenderSettings.ambientEquatorColor = new Color(.045f, .07f, .13f);
            RenderSettings.ambientGroundColor = new Color(.015f, .02f, .04f);
            RenderSettings.ambientIntensity = .8f;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.04f, .07f, .13f);
            RenderSettings.fogDensity = .018f;

            Camera camera = new GameObject("Main Menu Camera").AddComponent<Camera>();
            camera.gameObject.AddComponent<AudioListener>();
            camera.tag = "MainCamera";
            camera.transform.position = new Vector3(0f, 4.2f, -14f);
            camera.transform.rotation = Quaternion.Euler(7f, 0f, 0f);
            camera.fieldOfView = 52f;
            camera.clearFlags = CameraClearFlags.Skybox;
            MoonlitPostProcessing.ConfigureCamera(camera);

            Light moon = new GameObject("Menu Moonlight").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(.55f, .68f, 1f);
            moon.intensity = 1.1f;
            moon.shadows = LightShadows.Soft;
            moon.transform.rotation = Quaternion.Euler(42f, -28f, 0f);

            Material wall = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/Moonlit Ruin Wall.mat");
            Material dark = AssetDatabase.LoadAssetAtPath<Material>($"{MaterialFolder}/Shadow Stone.mat");
            ProductionRuinBuilder.CreateModule("Menu Gothic Portal", QuaterniusRuinAssetId.ArchGothic, new Vector3(-3.5f, 0f, 2f), Quaternion.Euler(0f, 18f, 0f), Vector3.one * 3.4f, wall);
            ProductionRuinBuilder.CreateModule("Menu Broken Colonnade", QuaterniusRuinAssetId.DoubleHoleWall, new Vector3(4.5f, 0f, 3f), Quaternion.Euler(0f, -20f, 0f), Vector3.one * 3.2f, dark);
            CreateMenuFloor(wall);

            GameObject post = new GameObject("Menu Post Processing");
            Volume volume = post.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            new GameObject("Main Menu Controller").AddComponent<MainMenuController>();

            EditorSceneManager.MarkSceneDirty(menu);
            if (!EditorSceneManager.SaveScene(menu, MainMenuScenePath))
                throw new IOException($"Could not save {MainMenuScenePath}");
        }

        private static void CreateMenuFloor(Material material)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Menu Ruin Terrace";
            floor.transform.localScale = new Vector3(5f, 1f, 5f);
            floor.GetComponent<Renderer>().sharedMaterial = material;
        }

        public static void GenerateFromCommandLine()
        {
            Generate();
        }

        private static void PersistGeneratedMaterials()
        {
            var persisted = new Dictionary<Material, Material>();
            foreach (Renderer renderer in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                Material source = renderer.sharedMaterial;
                if (source == null || AssetDatabase.Contains(source)) continue;
                if (!persisted.TryGetValue(source, out Material asset))
                {
                    string path = $"{MaterialFolder}/{Sanitize(source.name)}.mat";
                    asset = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (asset == null)
                    {
                        asset = new Material(source) { name = source.name };
                        AssetDatabase.CreateAsset(asset, path);
                    }
                    else
                    {
                        asset.CopyPropertiesFromMaterial(source);
                        EditorUtility.SetDirty(asset);
                    }
                    persisted.Add(source, asset);
                }
                renderer.sharedMaterial = asset;
            }
        }

        private static void PersistVolumeProfile()
        {
            Volume volume = Object.FindFirstObjectByType<Volume>();
            if (volume == null) return;

            VolumeProfile existing = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (existing == null)
            {
                existing = ScriptableObject.CreateInstance<VolumeProfile>();
                existing.name = "Moonlit Ruins Volume Profile";
                AssetDatabase.CreateAsset(existing, VolumeProfilePath);
            }

            foreach (VolumeComponent component in new List<VolumeComponent>(existing.components))
            {
                if (component != null) Object.DestroyImmediate(component, true);
            }
            existing.components.Clear();

            VolumeProfile generated = volume.profile;
            foreach (VolumeComponent source in generated.components)
            {
                VolumeComponent component = Object.Instantiate(source);
                component.name = source.GetType().Name;
                component.hideFlags = HideFlags.HideInHierarchy;
                existing.components.Add(component);
                AssetDatabase.AddObjectToAsset(component, existing);
            }

            volume.sharedProfile = existing;
            EditorUtility.SetDirty(existing);
            EditorUtility.SetDirty(volume);
        }

        private static void PersistSkybox()
        {
            Material source = RenderSettings.skybox;
            if (source == null || AssetDatabase.Contains(source)) return;
            const string path = "Assets/Settings/MoonlitCitadelSky.mat";
            Material asset = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (asset == null)
            {
                asset = new Material(source) { name = source.name };
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                asset.CopyPropertiesFromMaterial(source);
                EditorUtility.SetDirty(asset);
            }
            RenderSettings.skybox = asset;
        }

        private static void EnsureFolder(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            string parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            string name = Path.GetFileName(assetPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent ?? "Assets", name);
        }

        private static string Sanitize(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars()) value = value.Replace(invalid, '_');
            return value.Replace(" (Instance)", string.Empty);
        }
    }
}
#endif
