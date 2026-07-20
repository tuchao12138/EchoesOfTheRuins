using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace EchoesOfTheRuins
{
    /// <summary>Builds the coursework graybox at runtime, so the default empty scene is playable.</summary>
    public static class RuinSceneBootstrap
    {
        private static readonly Color Stone = new Color(.34f, .39f, .50f);
        private static readonly Color DarkStone = new Color(.18f, .22f, .31f);
        private static readonly Color Accent = new Color(.05f, .84f, 1f);
        private static readonly Color Warning = new Color(1f, .52f, .18f);
        private static NavMeshData runtimeNavMesh;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterSceneLoadHandler()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildIfNeeded()
        {
            if (Object.FindFirstObjectByType<MainMenuController>() != null) return;
            if (Object.FindFirstObjectByType<GameManager>() != null)
            {
                UpgradeExistingProductionScene();
                return;
            }
            BuildProductionScene();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BuildIfNeeded();
        }

        private static void UpgradeExistingProductionScene()
        {
            QualitySettings.vSyncCount = 1;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            Application.targetFrameRate = MotionClarityProfile.TargetFrameRate;

            GameObject floor = GameObject.Find("Ruin Floor");
            Renderer renderer = floor == null ? null : floor.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.mainTextureScale = Vector2.one * MotionClarityProfile.FloorTextureTiling;
                Texture texture = renderer.material.mainTexture;
                if (texture is Texture2D floorTexture)
                {
                    floorTexture.filterMode = FilterMode.Trilinear;
                    floorTexture.anisoLevel = MotionClarityProfile.AnisotropicLevel;
                }
            }

            if (Object.FindFirstObjectByType<RelicCollectible>() != null) return;
            Material relic = MakeMaterial("Relic Amber", Warning, true);
            CreateRelic("Moon Tablet Relic", new Vector3(-18f, 1.1f, 8f), "moon-tablet", relic);
            CreateRelic("Guardian Sigil Relic", new Vector3(18f, 1.1f, 8f), "guardian-sigil", relic);
        }

        /// <summary>
        /// Creates the complete production hierarchy. The editor scene generator calls this once and saves
        /// the result, while the runtime hook remains as a safe fallback for older prototype scenes.
        /// </summary>
        public static GameObject BuildProductionScene()
        {
            GameManager existing = Object.FindFirstObjectByType<GameManager>();
            if (existing != null) return existing.gameObject;

            ProductionSceneLayout layout = ProductionSceneLayout.CreateDefault();

            QualitySettings.vSyncCount = 1;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            Application.targetFrameRate = MotionClarityProfile.TargetFrameRate;

            Material stone = MakeMaterial("Ruins Stone", Stone);
            Material darkStone = MakeMaterial("Shadow Stone", DarkStone);
            Material floorStone = MakeMaterial("Moonlit Rune Stone Floor", Color.white);
            Material wallStone = MakeMaterial("Moonlit Ruin Wall", Color.white);
            Texture2D floorTexture = Resources.Load<Texture2D>("Textures/MoonlitRuneStone");
            if (floorTexture != null)
            {
                floorStone.mainTexture = floorTexture;
                floorStone.mainTextureScale = Vector2.one * MotionClarityProfile.FloorTextureTiling;
                floorTexture.filterMode = FilterMode.Trilinear;
                floorTexture.anisoLevel = MotionClarityProfile.AnisotropicLevel;
            }
            Texture2D wallTexture = Resources.Load<Texture2D>("Textures/MoonlitRuinWall");
            if (wallTexture != null)
            {
                wallStone.mainTexture = wallTexture;
                wallStone.mainTextureScale = new Vector2(1.2f, 1.2f);
                stone.mainTexture = wallTexture;
                stone.mainTextureScale = new Vector2(1.35f, 1.35f);
                darkStone.mainTexture = wallTexture;
                darkStone.mainTextureScale = new Vector2(1.35f, 1.35f);
            }
            Material accent = MakeMaterial("Energy Cyan", Accent, true);
            Material warning = MakeMaterial("Gate Amber", Warning, true);

            ConfigureMoonlitAtmosphere();
            MoonlitPostProcessing.Configure(new GameObject("Moonlit Post Processing"));
            CreateBox("Ruin Floor", new Vector3(0f, -.5f, 1f), new Vector3(72f, 1f, 120f), floorStone);
            CreateEnvironment(stone, darkStone, wallStone, accent, warning);
            CreateExtendedJourney(wallStone, darkStone, warning);

            Transform entryCheckpoint = CreateCheckpoint("Entry Checkpoint", layout.PlayerSpawn, accent);
            Transform player = CreatePlayer(entryCheckpoint.position);
            GameManager manager = new GameObject("Game Manager").AddComponent<GameManager>();
            manager.Configure(player, entryCheckpoint);
            TutorialDirector tutorial = manager.gameObject.AddComponent<TutorialDirector>();
            tutorial.Configure(player.GetComponent<PlayerController>());

            BuildNavMesh();
            var guardians = new List<GuardianAI>();
            foreach (GuardianRoute route in layout.Guardians)
            {
                GuardianAI guardian = CreateGuardian(player, route);
                guardians.Add(guardian);
                manager.TrackGuardian(guardian);
            }
            CreateHud(guardians[0]);
            AudioDirector audio = new GameObject("Citadel Audio Director").AddComponent<AudioDirector>();
            audio.Configure(guardians);
            int coreNumber = 1;
            Transform firstCoreTarget = null;
            foreach (CorePlacement core in layout.Cores)
            {
                Transform coreTarget = CreateCore($"Core {coreNumber++} - {core.Zone}", core.Position, core.Id, accent);
                if (core.Zone == RuinZone.Altar)
                {
                    Light beacon = CreateLight("Altar Core Cyan Beacon", core.Position + Vector3.up * 6f, Accent, 16f, 4.5f);
                    beacon.transform.SetParent(coreTarget, true);
                }
                if (firstCoreTarget == null) firstCoreTarget = coreTarget;
            }
            CreateRelic("Moon Tablet Relic", new Vector3(-20f, 1.1f, 10f), "moon-tablet", warning);
            CreateRelic("Guardian Sigil Relic", new Vector3(20f, 1.1f, 40f), "guardian-sigil", warning);
            CreateCheckpoint("Courtyard Checkpoint", new Vector3(0f, 0f, -14f), accent);
            CreateCheckpoint("Altar Checkpoint", new Vector3(0f, 0f, 20f), accent);
            Transform exitTarget = CreateExit(layout.ExitPosition, warning, accent);
            WorldObjectiveMarker marker = new GameObject("World Objective Marker").AddComponent<WorldObjectiveMarker>();
            ObjectiveDirector objectives = manager.gameObject.AddComponent<ObjectiveDirector>();
            objectives.Configure(
                player.GetComponent<PlayerController>(),
                firstCoreTarget,
                exitTarget,
                guardians.Count > 0 ? guardians[0].transform : null,
                marker,
                Camera.main);
            CreateShadowZone("Side Chamber Shadow", new Vector3(-17f, 1.4f, 3f), new Vector3(9f, 2.5f, 18f));
            CreateShadowZone("Courtyard Pillar Shadow", new Vector3(-7f, 1.2f, -27f), new Vector3(3f, 2f, 8f));
            return manager.gameObject;
        }

        private static void CreateExtendedJourney(Material wallStone, Material darkStone, Material warning)
        {
            CreateMasonryBarrier("Safe Entry West Wall", new Vector3(-5.3f, 2.6f, -41f), new Vector3(1f, 5.2f, 25f), wallStone);
            CreateMasonryBarrier("Safe Entry East Wall", new Vector3(5.3f, 2.6f, -41f), new Vector3(1f, 5.2f, 25f), wallStone);
            ProductionRuinBuilder.CreateModule("Safe Entry Arch", QuaterniusRuinAssetId.ArchGothic,
                new Vector3(0f, 0f, -42f), Quaternion.identity, Vector3.one * 1.8f, wallStone);

            CreateMasonryBarrier("North Processional West", new Vector3(-12f, 2.8f, 30f), new Vector3(1f, 5.6f, 24f), darkStone);
            CreateMasonryBarrier("North Processional East", new Vector3(12f, 2.8f, 30f), new Vector3(1f, 5.6f, 24f), darkStone);
            CreateMasonryBarrier("Altar Perimeter West", new Vector3(-23f, 2.8f, 33f), new Vector3(1f, 5.6f, 22f), wallStone);
            CreateMasonryBarrier("Altar Perimeter East", new Vector3(23f, 2.8f, 33f), new Vector3(1f, 5.6f, 22f), wallStone);
            CreateMasonryBarrier("Altar North Broken Wall", new Vector3(0f, 2.6f, 43f), new Vector3(34f, 5.2f, 1f), wallStone);
            ProductionRuinBuilder.CreateModule("Altar Monumental Arch", QuaterniusRuinAssetId.ArchGothic,
                new Vector3(0f, 0f, 43f), Quaternion.identity, Vector3.one * 2.4f, wallStone);

            CreateMasonryBarrier("Exit Causeway West", new Vector3(-7f, 2.8f, 50f), new Vector3(1f, 5.6f, 14f), darkStone);
            CreateMasonryBarrier("Exit Causeway East", new Vector3(7f, 2.8f, 50f), new Vector3(1f, 5.6f, 14f), darkStone);
            ProductionRuinBuilder.CreateModule("North Gate Monument", QuaterniusRuinAssetId.ArchGothic,
                new Vector3(0f, 0f, 55f), Quaternion.identity, Vector3.one * 2.8f, warning);

            for (int index = 0; index < 6; index++)
            {
                float z = -48f + index * 19f;
                CreateBrazier($"Journey Brazier West {index + 1}", new Vector3(-4.1f, 0f, z), wallStone, warning);
                CreateBrazier($"Journey Brazier East {index + 1}", new Vector3(4.1f, 0f, z), wallStone, warning);
            }
        }

        private static void CreateMasonryBarrier(string name, Vector3 position, Vector3 size, Material material)
        {
            MasonryWallBuilder.Create(name, position, size, material);
            CreateCollisionBox(name + " Collision", position, size);
        }

        private static void ConfigureMoonlitAtmosphere()
        {
            MoonlitLightingProfile lighting = MoonlitLightingProfile.Readable;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = lighting.AmbientSky;
            RenderSettings.ambientEquatorColor = lighting.AmbientEquator;
            RenderSettings.ambientGroundColor = lighting.AmbientGround;
            RenderSettings.ambientIntensity = lighting.AmbientIntensity;
            RenderSettings.fog = true;
            RenderSettings.fogColor = lighting.FogColor;
            RenderSettings.fogDensity = .0045f;

            Shader skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                Material sky = new Material(skyShader) { name = "Moonlit Citadel Sky" };
                sky.SetColor("_SkyTint", new Color(.10f, .16f, .30f));
                sky.SetColor("_GroundColor", new Color(.025f, .035f, .07f));
                sky.SetFloat("_AtmosphereThickness", .48f);
                sky.SetFloat("_Exposure", .42f);
                sky.SetFloat("_SunDisk", 0f);
                RenderSettings.skybox = sky;
            }

            Light moon = new GameObject("Moonlight").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(.62f, .72f, 1f);
            moon.intensity = lighting.MoonIntensity;
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = .75f;
            moon.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private static void BuildNavMesh()
        {
            var sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(null, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
            var bounds = new Bounds(new Vector3(0f, 2f, 1f), new Vector3(74f, 10f, 122f));
            runtimeNavMesh = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0), sources, bounds, Vector3.zero, Quaternion.identity);
            if (runtimeNavMesh != null) NavMesh.AddNavMeshData(runtimeNavMesh);
        }

        private static void CreateEnvironment(Material stone, Material darkStone, Material wallStone, Material accent, Material warning)
        {
            // Entry corridor and central courtyard.
            CreateBox("Entry Left Wall", new Vector3(-4f, 2.5f, -19f), new Vector3(1f, 5f, 12f), stone, false);
            CreateBox("Entry Right Wall", new Vector3(4f, 2.5f, -19f), new Vector3(1f, 5f, 12f), stone, false);
            CreateBox("Courtyard North Wall Left", new Vector3(-8f, 2.5f, 18f), new Vector3(6f, 5f, 1f), stone, false);
            CreateBox("Courtyard North Wall Right", new Vector3(8f, 2.5f, 18f), new Vector3(6f, 5f, 1f), stone, false);
            CreateBox("Courtyard West Wall South", new Vector3(-11f, 2.5f, -6f), new Vector3(1f, 5f, 12f), stone, false);
            CreateBox("Courtyard West Wall North", new Vector3(-11f, 2.5f, 12f), new Vector3(1f, 5f, 12f), stone, false);
            CreateBox("Courtyard East Wall South", new Vector3(11f, 2.5f, -5f), new Vector3(1f, 5f, 12f), stone, false);
            CreateBox("Courtyard East Wall North", new Vector3(11f, 2.5f, 8f), new Vector3(1f, 5f, 6f), stone, false);
            RuinPropVisualBuilder.CreateRitualDais("Central Ritual Dais", new Vector3(0f, 0f, -3f), 3.2f, stone);
            CreateCollisionBox("Central Dais Collision", new Vector3(0f, .35f, -3f), new Vector3(5.7f, .7f, 5.7f));
            ProductionRuinBuilder.CreateModule("Central Altar Broken Wall", QuaterniusRuinAssetId.OvergrownWall, new Vector3(0f, 0f, -3f), Quaternion.Euler(0f, 45f, 0f), Vector3.one * 1.8f, wallStone);
            CreatePillar("Courtyard Pillar A", new Vector3(-7f, 0f, -7f), stone);
            CreatePillar("Courtyard Pillar B", new Vector3(7f, 0f, -7f), stone);
            CreatePillar("Courtyard Pillar C", new Vector3(-7f, 0f, 10f), stone);
            CreatePillar("Courtyard Pillar D", new Vector3(7f, 0f, 10f), stone);
            ProductionRuinBuilder.CreateModule("Entry Gothic Arch", QuaterniusRuinAssetId.ArchGothic, new Vector3(0f, 0f, -15.5f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.7f, wallStone);
            MasonryWallBuilder.Create("Entry Monumental Lintel", new Vector3(0f, 4.45f, -15.5f), new Vector3(9f, 1.15f, 1.25f), wallStone);
            ProductionRuinBuilder.CreateModule("Entry Broken Wall Left", QuaterniusRuinAssetId.BrokenWall, new Vector3(-4.25f, 0f, -18f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 1.8f, wallStone);
            ProductionRuinBuilder.CreateModule("Entry Broken Wall Right", QuaterniusRuinAssetId.OvergrownWall, new Vector3(4.25f, 0f, -18f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 1.8f, wallStone);
            ProductionRuinBuilder.CreateModule("Courtyard North Arch", QuaterniusRuinAssetId.ArchRound, new Vector3(0f, 0f, 17.8f), Quaternion.identity, Vector3.one * 2.3f, wallStone);
            ProductionRuinBuilder.CreateModule("Courtyard West Window Wall", QuaterniusRuinAssetId.DoubleHoleWall, new Vector3(-11.2f, 0f, 1f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 2.1f, wallStone);
            ProductionRuinBuilder.CreateModule("Courtyard East Window Wall", QuaterniusRuinAssetId.DoubleWindow, new Vector3(11.2f, 0f, 1f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 2.1f, wallStone);
            CreateCastleProp("Entry Rubble West", RuinsAssetId.Rocks, new Vector3(-3.45f, 0f, -15.7f), Quaternion.Euler(0f, 35f, 0f), Vector3.one * .75f, darkStone);
            CreateCastleProp("Entry Rubble East", RuinsAssetId.Rocks, new Vector3(3.45f, 0f, -15.7f), Quaternion.Euler(0f, -35f, 0f), Vector3.one * .75f, darkStone);

            // Open side chamber, marked in cool color.
            CreateBox("Side Chamber Back", new Vector3(-22f, 2.5f, 3f), new Vector3(1f, 5f, 12f), darkStone, false);
            CreateBox("Side Chamber North", new Vector3(-17f, 2.5f, 9f), new Vector3(10f, 5f, 1f), darkStone, false);
            CreateBox("Side Chamber South", new Vector3(-17f, 2.5f, -3f), new Vector3(10f, 5f, 1f), darkStone, false);
            CreateBox("Side Chamber Plinth", new Vector3(-17f, .5f, 3f), new Vector3(4f, 1f, 4f), stone);
            ProductionRuinBuilder.CreateModule("Side Chamber Stone Frame", QuaterniusRuinAssetId.ArchRound, new Vector3(-17f, 0f, -1f), Quaternion.identity, Vector3.one * 1.5f, wallStone);
            ProductionRuinBuilder.CreateModule("Side Chamber Broken Arch", QuaterniusRuinAssetId.BrokenWall, new Vector3(-22.2f, 0f, 3f), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 2.2f, wallStone);
            CreateLight("Side Chamber Glow", new Vector3(-17f, 4f, 3f), Accent, 9f, 1.5f);

            // Raised altar chamber in warm contrast.
            CreateBox("Altar Back Wall", new Vector3(22f, 2.5f, 12f), new Vector3(1f, 5f, 14f), darkStone, false);
            CreateBox("Altar North Wall", new Vector3(16f, 2.5f, 19f), new Vector3(12f, 5f, 1f), darkStone, false);
            CreateBox("Altar South Wall", new Vector3(16f, 2.5f, 5f), new Vector3(12f, 5f, 1f), darkStone, false);
            RuinPropVisualBuilder.CreateRitualDais("Altar Ritual Dais", new Vector3(16f, 0f, 12f), 2.8f, stone);
            CreateCollisionBox("Altar Dais Collision", new Vector3(16f, .35f, 12f), new Vector3(5f, .7f, 5f));
            ProductionRuinBuilder.CreateModule("Altar Chamber Arch", QuaterniusRuinAssetId.ArchGothic, new Vector3(21.8f, 0f, 12f), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 2.1f, wallStone);
            CreateLight("Altar Flame", new Vector3(16f, 5f, 12f), Warning, 10f, 1.5f);
            CreateBox("Exit Approach Left", new Vector3(-7f, 2.5f, 23f), new Vector3(1f, 5f, 10f), stone);
            CreateBox("Exit Approach Right", new Vector3(7f, 2.5f, 23f), new Vector3(1f, 5f, 10f), stone);
            MoonlitLightingProfile lighting = MoonlitLightingProfile.Readable;
            CreateLight("Entry Fill Light", new Vector3(0f, 4f, -19f), new Color(.52f, .68f, 1f), 17f, lighting.EntryFillIntensity);
            CreateLight("Courtyard Moonlight", new Vector3(0f, 9f, 0f), new Color(.58f, .73f, 1f), 22f, lighting.CourtyardFillIntensity);
            CreateBrazier("Entry Brazier Left", new Vector3(-3f, 0f, -17f), stone, warning);
            CreateBrazier("Entry Brazier Right", new Vector3(3f, 0f, -17f), stone, warning);
            CreateBrazier("Courtyard Brazier", new Vector3(-8f, 0f, -1f), stone, warning);
            CourtyardSetDressingBuilder.Create(stone, darkStone);
            CreatePerimeterArchitecture(wallStone, darkStone);
        }

        private static void CreatePerimeterArchitecture(Material wallStone, Material darkStone)
        {
            CitadelBackdropBuilder.Create(darkStone);
            int index = 0;
            for (int z = -20; z <= 20; z += 8)
            {
                ProductionRuinBuilder.CreateModule($"Backdrop West Wall {++index}", QuaterniusRuinAssetId.DoubleHoleWall,
                    new Vector3(-26f, 0f, z), Quaternion.Euler(0f, 90f, 0f), Vector3.one * 3.1f, darkStone);
                ProductionRuinBuilder.CreateModule($"Backdrop East Wall {++index}", QuaterniusRuinAssetId.BrokenWall,
                    new Vector3(26f, 0f, z), Quaternion.Euler(0f, -90f, 0f), Vector3.one * 3.1f, darkStone);
            }

            for (int x = -20; x <= 20; x += 8)
            {
                ProductionRuinBuilder.CreateModule($"Backdrop North Wall {++index}", QuaterniusRuinAssetId.OvergrownWall,
                    new Vector3(x, 0f, 27f), Quaternion.identity, Vector3.one * 3.25f, darkStone);
                ProductionRuinBuilder.CreateModule($"Backdrop South Wall {++index}", QuaterniusRuinAssetId.Wall,
                    new Vector3(x, 0f, -27f), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 3.25f, darkStone);
            }

            Vector3[] towerPositions =
            {
                new Vector3(-25f, 0f, -24f), new Vector3(25f, 0f, -24f),
                new Vector3(-25f, 0f, 24f), new Vector3(25f, 0f, 24f)
            };
            for (int tower = 0; tower < towerPositions.Length; tower++)
                CreateCastleProp($"Backdrop Citadel Tower {tower + 1}", RuinsAssetId.Tower, towerPositions[tower],
                    Quaternion.Euler(0f, 45f + tower * 90f, 0f), Vector3.one * 4.3f, wallStone);
        }

        private static Transform CreatePlayer(Vector3 spawn)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.SetPositionAndRotation(new Vector3(spawn.x, 0f, spawn.z), Quaternion.identity);
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.center = new Vector3(0f, .9f, 0f);
            CreateCharacterVisual(player.transform, CharacterAssetId.Explorer, "Explorer", .92f, 0f);

            Transform pivot = new GameObject("Camera Pivot").transform;
            pivot.SetParent(player.transform, false);
            pivot.localPosition = new Vector3(0f, 1.6f, 0f);
            Camera camera = new GameObject("Player Camera").AddComponent<Camera>();
            camera.gameObject.AddComponent<AudioListener>();
            camera.tag = "MainCamera";
            camera.transform.SetParent(pivot, false);
            Vector3 cameraOffset = new Vector3(1.05f, .4f, -5.75f);
            camera.transform.localPosition = cameraOffset;
            camera.nearClipPlane = .05f;
            camera.fieldOfView = 63f;
            camera.clearFlags = CameraClearFlags.Skybox;
            MoonlitPostProcessing.ConfigureCamera(camera);
            PlayerController playerController = player.AddComponent<PlayerController>();
            playerController.Configure(pivot);
            player.AddComponent<PlayerInteractor>();
            CameraFollow follow = pivot.gameObject.AddComponent<CameraFollow>();
            follow.Configure(player.transform, new Vector3(0f, 1.55f, 0f));
            ThirdPersonCameraCollision collision = camera.gameObject.AddComponent<ThirdPersonCameraCollision>();
            collision.Configure(pivot, cameraOffset);
            return player.transform;
        }

        private static GuardianAI CreateGuardian(Transform player, GuardianRoute route)
        {
            GameObject guardian = new GameObject(route.Id);
            guardian.transform.position = route.Spawn;
            CreateCharacterVisual(guardian.transform, CharacterAssetId.Guardian, "Guardian Warrior", .94f, 0f);
            UnityEngine.AI.NavMeshAgent agent = guardian.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.radius = .4f;
            agent.height = 1.8f;
            GuardianAI ai = guardian.AddComponent<GuardianAI>();
            Light visionLight = new GameObject("Guardian Vision Light").AddComponent<Light>();
            visionLight.type = LightType.Spot;
            visionLight.color = new Color(1f, .80f, .25f);
            visionLight.range = 10f;
            visionLight.spotAngle = 75f;
            visionLight.intensity = 4f;
            visionLight.transform.SetParent(guardian.transform, false);
            visionLight.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            visionLight.transform.localRotation = Quaternion.Euler(6f, 0f, 0f);
            visionLight.gameObject.AddComponent<GuardianVisionLight>();
            Transform[] patrol = new Transform[route.Waypoints.Count];
            for (int index = 0; index < route.Waypoints.Count; index++)
                patrol[index] = CreateMarker($"{route.Id} Waypoint {index + 1}", route.Waypoints[index]);
            ai.Configure(player, patrol);
            return ai;
        }

        private static void CreateHud(GuardianAI guardian)
        {
            CanvasHud ui = new GameObject("Gameplay HUD").AddComponent<CanvasHud>();
            ui.Configure(guardian);
        }

        private static Transform CreateCheckpoint(string name, Vector3 position, Material material)
        {
            GameObject checkpoint = new GameObject(name);
            checkpoint.transform.position = position;
            LandmarkVisualBuilder.CreateCheckpoint(checkpoint.transform, material);
            SphereCollider collider = checkpoint.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.center = new Vector3(0f, 1f, 0f);
            collider.radius = 1.5f;
            checkpoint.AddComponent<Rigidbody>().isKinematic = true;
            CheckpointTrigger trigger = checkpoint.AddComponent<CheckpointTrigger>();
            Transform spawn = CreateMarker(name + " Spawn", position + new Vector3(0f, .05f, 1.5f));
            spawn.rotation = Quaternion.identity;
            trigger.Configure(spawn);
            return spawn;
        }

        private static Transform CreateCore(string name, Vector3 position, string coreId, Material material)
        {
            GameObject core = new GameObject(name);
            core.transform.position = position;
            LandmarkVisualBuilder.CreateCore(core.transform, material, material);
            SphereCollider collider = core.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = .8f;
            core.AddComponent<Rigidbody>().isKinematic = true;
            Collectible collectible = core.AddComponent<Collectible>();
            collectible.Configure(coreId);
            core.AddComponent<RuneFloat>();
            return core.transform;
        }

        private static Transform CreateRelic(string name, Vector3 position, string relicId, Material material)
        {
            GameObject relic = new GameObject(name);
            relic.transform.position = position;
            relic.transform.localScale = Vector3.one * .58f;
            LandmarkVisualBuilder.CreateCore(relic.transform, material, material);
            SphereCollider collider = relic.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 1.15f;
            relic.AddComponent<Rigidbody>().isKinematic = true;
            RelicCollectible collectible = relic.AddComponent<RelicCollectible>();
            collectible.Configure(relicId);
            relic.AddComponent<RuneFloat>();
            return relic.transform;
        }

        private static Transform CreateExit(Vector3 position, Material lockedMaterial, Material unlockedMaterial)
        {
            GameObject gate = new GameObject("Sealed Exit Gate");
            gate.transform.position = position;
            BoxCollider trigger = gate.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(5f, 4f, 1f);
            gate.AddComponent<Rigidbody>().isKinematic = true;
            GameObject locked = new GameObject("Locked Rune Seal");
            locked.transform.position = position;
            LandmarkVisualBuilder.CreateGateBarrier(locked.transform, lockedMaterial);
            GameObject opened = new GameObject("Opened Rune Seal");
            opened.transform.position = position;
            LandmarkVisualBuilder.CreateCheckpoint(opened.transform, unlockedMaterial);
            ProductionRuinBuilder.CreateModule("Sealed Gothic Gate", QuaterniusRuinAssetId.ArchGothic, position, Quaternion.identity, Vector3.one * 1.5f, lockedMaterial);
            Light beacon = CreateLight("Exit Beacon", position + Vector3.up * 2f, Warning, 7f, 1.2f);
            beacon.transform.SetParent(gate.transform, true);
            ExitGate exit = gate.AddComponent<ExitGate>();
            exit.Configure(locked, opened, beacon);
            return gate.transform;
        }

        private static void CreateBrazier(string name, Vector3 position, Material stone, Material fire)
        {
            RuinPropVisualBuilder.CreateBrazier(name, position, stone, fire);
        }

        private static void CreateShadowZone(string name, Vector3 position, Vector3 size)
        {
            GameObject zone = new GameObject(name);
            zone.transform.position = position;
            BoxCollider collider = zone.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = size;
            zone.AddComponent<Rigidbody>().isKinematic = true;
            zone.AddComponent<ShadowZone>();
        }

        private static Material MakeMaterial(string name, Color color, bool emissive = false)
        {
            return RuntimeMaterialLibrary.Create(name, color, emissive);
        }

        private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material, bool visible = true)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetPositionAndRotation(position, Quaternion.identity);
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().material = material;
            obj.GetComponent<Renderer>().enabled = visible;
            if (!visible) MasonryWallBuilder.Create(name + " Masonry", position, scale, material);
            return obj;
        }

        private static GameObject CreateCollisionBox(string name, Vector3 position, Vector3 scale)
        {
            GameObject obj = new GameObject(name);
            obj.transform.position = position;
            BoxCollider collider = obj.AddComponent<BoxCollider>();
            collider.size = scale;
            return obj;
        }

        private static GameObject CreateSphere(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = name;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().material = material;
            return obj;
        }

        private static GameObject CreateCylinder(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            obj.name = name;
            obj.transform.position = position;
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().material = material;
            return obj;
        }

        private static void CreatePillar(string name, Vector3 position, Material material) =>
            ProductionRuinBuilder.CreateModule(name, QuaterniusRuinAssetId.SquareColumn, position, Quaternion.identity, Vector3.one * 1.6f, material);

        private static GameObject CreateCastleProp(string name, RuinsAssetId assetId, Vector3 position, Quaternion rotation, Vector3 scale, Material material)
        {
            GameObject template = Resources.Load<GameObject>(RuinsAssetCatalog.GetPath(assetId));
            if (template == null) return null;

            GameObject prop = Object.Instantiate(template, position, rotation);
            prop.name = name;
            prop.transform.localScale = scale;
            foreach (Renderer renderer in prop.GetComponentsInChildren<Renderer>()) renderer.material = material;
            return prop;
        }

        private static GameObject CreateDungeonProp(string name, DungeonAssetId assetId, Vector3 position, Quaternion rotation, Vector3 scale, Material material)
        {
            GameObject template = Resources.Load<GameObject>(DungeonAssetCatalog.GetPath(assetId));
            if (template == null) return null;

            GameObject prop = Object.Instantiate(template, position, rotation);
            prop.name = name;
            prop.transform.localScale = scale;
            foreach (Renderer renderer in prop.GetComponentsInChildren<Renderer>()) renderer.material = material;
            return prop;
        }

        private static void CreateCharacterVisual(Transform root, CharacterAssetId assetId, string visualName, float scale, float yaw)
        {
            GameObject template = Resources.Load<GameObject>(CharacterAssetCatalog.GetPath(assetId));
            if (template == null)
            {
                Debug.LogError($"Missing rigged character asset: {CharacterAssetCatalog.GetPath(assetId)}");
                return;
            }

            GameObject visual = Object.Instantiate(template, root);
            visual.name = visualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            visual.transform.localScale = Vector3.one * scale;
            Texture2D texture = Resources.Load<Texture2D>(CharacterAssetCatalog.GetTexturePath(assetId));
            Material characterMaterial = RuntimeMaterialLibrary.CreateTextured(visualName + " Material", texture);
            foreach (Renderer renderer in visual.GetComponentsInChildren<Renderer>(true)) renderer.material = characterMaterial;
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            CharacterMotionAnimator motion = root.GetComponent<CharacterMotionAnimator>() ?? root.gameObject.AddComponent<CharacterMotionAnimator>();
            motion.Configure(visual.transform, CharacterAssetCatalog.GetPath(assetId));
        }

        private static void CreateProductionCharacterVisual(Transform root, string visualName, float scale, float yaw)
        {
            string path = QuaterniusRuinAssetCatalog.GetPath(QuaterniusRuinAssetId.AnimatedExplorer);
            GameObject template = Resources.Load<GameObject>(path);
            if (template == null)
            {
                Debug.LogError($"Missing production character asset: {path}");
                return;
            }

            GameObject visual = Object.Instantiate(template, root);
            visual.name = visualName;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            visual.transform.localScale = Vector3.one * scale;
            foreach (Collider collider in visual.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        }

        private static Transform CreateMarker(string name, Vector3 position, Material markerMaterial = null)
        {
            GameObject marker = new GameObject(name);
            marker.transform.position = position;
            if (markerMaterial != null)
            {
                GameObject visibleMarker = CreateSphere(name + " Marker", position + Vector3.up * .35f, Vector3.one * .45f, markerMaterial);
                visibleMarker.GetComponent<Collider>().enabled = false;
            }
            return marker.transform;
        }

        private static Light CreateLight(string name, Vector3 position, Color color, float range, float intensity)
        {
            Light light = new GameObject(name).AddComponent<Light>();
            light.type = LightType.Point;
            light.transform.position = position;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            return light;
        }
    }
}
