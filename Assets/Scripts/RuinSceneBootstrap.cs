using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void BuildIfNeeded()
        {
            if (Object.FindFirstObjectByType<GameManager>() != null) return;

            Material stone = MakeMaterial("Ruins Stone", Stone);
            Material darkStone = MakeMaterial("Shadow Stone", DarkStone);
            Material accent = MakeMaterial("Energy Cyan", Accent, true);
            Material warning = MakeMaterial("Gate Amber", Warning, true);

            ConfigureMoonlitAtmosphere();
            CreateBox("Ruin Floor", new Vector3(0f, -.5f, 0f), new Vector3(56f, 1f, 56f), darkStone);
            CreateEnvironment(stone, darkStone, accent, warning);

            Transform entryCheckpoint = CreateCheckpoint("Entry Checkpoint", new Vector3(0f, 0f, -22f), accent);
            Transform player = CreatePlayer(entryCheckpoint.position);
            GameManager manager = new GameObject("Game Manager").AddComponent<GameManager>();
            manager.Configure(player, entryCheckpoint);
            TutorialDirector tutorial = manager.gameObject.AddComponent<TutorialDirector>();
            tutorial.Configure(player.GetComponent<PlayerController>());

            BuildNavMesh();
            GuardianAI guardian = CreateGuardian(player, warning);
            CreateHud(guardian);
            CreateCore("Core 1 - Courtyard", new Vector3(0f, 1.2f, -3f), "courtyard-core", accent);
            CreateCore("Core 2 - Side Chamber", new Vector3(-17f, 1.2f, 3f), "side-chamber-core", accent);
            CreateCore("Core 3 - Altar Chamber", new Vector3(16f, 1.2f, 15f), "altar-chamber-core", accent);
            CreateCheckpoint("Courtyard Checkpoint", new Vector3(0f, 0f, 5f), accent);
            CreateExit(new Vector3(0f, 2f, 25f), warning, accent);
            CreateShadowZone("Side Chamber Shadow", new Vector3(-17f, 1.4f, 3f), new Vector3(9f, 2.5f, 10f));
            CreateShadowZone("Courtyard Pillar Shadow", new Vector3(-7f, 1.2f, -7f), new Vector3(3f, 2f, 5f));
        }

        private static void ConfigureMoonlitAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.22f, .28f, .42f);
            RenderSettings.ambientEquatorColor = new Color(.14f, .18f, .28f);
            RenderSettings.ambientGroundColor = new Color(.07f, .08f, .12f);
            RenderSettings.ambientIntensity = .9f;
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(.10f, .14f, .22f);
            RenderSettings.fogDensity = .0045f;

            Light moon = new GameObject("Moonlight").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(.62f, .72f, 1f);
            moon.intensity = 1.05f;
            moon.shadows = LightShadows.Soft;
            moon.shadowStrength = .75f;
            moon.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private static void BuildNavMesh()
        {
            var sources = new List<NavMeshBuildSource>();
            NavMeshBuilder.CollectSources(null, ~0, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), sources);
            var bounds = new Bounds(new Vector3(0f, 2f, 2f), new Vector3(58f, 8f, 58f));
            runtimeNavMesh = NavMeshBuilder.BuildNavMeshData(NavMesh.GetSettingsByID(0), sources, bounds, Vector3.zero, Quaternion.identity);
            if (runtimeNavMesh != null) NavMesh.AddNavMeshData(runtimeNavMesh);
        }

        private static void CreateEnvironment(Material stone, Material darkStone, Material accent, Material warning)
        {
            // Entry corridor and central courtyard.
            CreateBox("Entry Left Wall", new Vector3(-4f, 2.5f, -19f), new Vector3(1f, 5f, 12f), stone);
            CreateBox("Entry Right Wall", new Vector3(4f, 2.5f, -19f), new Vector3(1f, 5f, 12f), stone);
            CreateBox("Courtyard North Wall Left", new Vector3(-8f, 2.5f, 18f), new Vector3(6f, 5f, 1f), stone);
            CreateBox("Courtyard North Wall Right", new Vector3(8f, 2.5f, 18f), new Vector3(6f, 5f, 1f), stone);
            CreateBox("Courtyard West Wall South", new Vector3(-11f, 2.5f, -6f), new Vector3(1f, 5f, 12f), stone);
            CreateBox("Courtyard West Wall North", new Vector3(-11f, 2.5f, 12f), new Vector3(1f, 5f, 12f), stone);
            CreateBox("Courtyard East Wall South", new Vector3(11f, 2.5f, -5f), new Vector3(1f, 5f, 12f), stone);
            CreateBox("Courtyard East Wall North", new Vector3(11f, 2.5f, 8f), new Vector3(1f, 5f, 6f), stone);
            CreateBox("Central Dais", new Vector3(0f, .75f, -3f), new Vector3(6f, 1.5f, 6f), stone);
            CreateCastleProp("Central Altar Ruin", RuinsAssetId.Corner, new Vector3(0f, 0f, -3f), Quaternion.Euler(0f, 45f, 0f), new Vector3(2.2f, 1.25f, 2.2f), darkStone);
            CreatePillar("Courtyard Pillar A", new Vector3(-7f, 2.5f, -7f), stone);
            CreatePillar("Courtyard Pillar B", new Vector3(7f, 2.5f, -7f), stone);
            CreatePillar("Courtyard Pillar C", new Vector3(-7f, 2.5f, 10f), stone);
            CreatePillar("Courtyard Pillar D", new Vector3(7f, 2.5f, 10f), stone);
            CreateCastleProp("Castle Entry Tower Left", RuinsAssetId.Tower, new Vector3(-4.5f, 0f, -22f), Quaternion.identity, new Vector3(2.2f, 2.2f, 2.2f), stone);
            CreateCastleProp("Castle Entry Tower Right", RuinsAssetId.Tower, new Vector3(4.5f, 0f, -22f), Quaternion.identity, new Vector3(2.2f, 2.2f, 2.2f), stone);
            CreateCastleProp("Castle Courtyard Wall", RuinsAssetId.Wall, new Vector3(0f, 0f, 18f), Quaternion.identity, new Vector3(3.1f, 2.2f, 2.2f), stone);
            CreateCastleProp("Castle Pillar A", RuinsAssetId.Pillar, new Vector3(-7f, 0f, -7f), Quaternion.identity, new Vector3(1.8f, 2.2f, 1.8f), stone);
            CreateCastleProp("Castle Pillar B", RuinsAssetId.Pillar, new Vector3(7f, 0f, -7f), Quaternion.identity, new Vector3(1.8f, 2.2f, 1.8f), stone);
            CreateCastleProp("Castle Pillar C", RuinsAssetId.Pillar, new Vector3(-7f, 0f, 10f), Quaternion.identity, new Vector3(1.8f, 2.2f, 1.8f), stone);
            CreateCastleProp("Castle Pillar D", RuinsAssetId.Pillar, new Vector3(7f, 0f, 10f), Quaternion.identity, new Vector3(1.8f, 2.2f, 1.8f), stone);
            CreateCastleProp("Courtyard Rubble", RuinsAssetId.Rocks, new Vector3(-4f, 0f, 5f), Quaternion.Euler(0f, 35f, 0f), new Vector3(1.6f, 1.3f, 1.6f), darkStone);

            // Open side chamber, marked in cool color.
            CreateBox("Side Chamber Back", new Vector3(-22f, 2.5f, 3f), new Vector3(1f, 5f, 12f), darkStone);
            CreateBox("Side Chamber North", new Vector3(-17f, 2.5f, 9f), new Vector3(10f, 5f, 1f), darkStone);
            CreateBox("Side Chamber South", new Vector3(-17f, 2.5f, -3f), new Vector3(10f, 5f, 1f), darkStone);
            CreateBox("Side Chamber Plinth", new Vector3(-17f, .5f, 3f), new Vector3(4f, 1f, 4f), stone);
            CreateCastleProp("Side Chamber Steps", RuinsAssetId.Stairs, new Vector3(-17f, 0f, -1f), Quaternion.identity, new Vector3(2.2f, 1.7f, 2.2f), stone);
            CreateLight("Side Chamber Glow", new Vector3(-17f, 4f, 3f), Accent, 9f, 5f);

            // Raised altar chamber in warm contrast.
            CreateBox("Altar Back Wall", new Vector3(22f, 2.5f, 12f), new Vector3(1f, 5f, 14f), darkStone);
            CreateBox("Altar North Wall", new Vector3(16f, 2.5f, 19f), new Vector3(12f, 5f, 1f), darkStone);
            CreateBox("Altar South Wall", new Vector3(16f, 2.5f, 5f), new Vector3(12f, 5f, 1f), darkStone);
            CreateBox("Altar", new Vector3(16f, 1f, 12f), new Vector3(5f, 2f, 5f), warning);
            CreateLight("Altar Flame", new Vector3(16f, 5f, 12f), Warning, 10f, 3f);
            CreateBox("Exit Approach Left", new Vector3(-7f, 2.5f, 23f), new Vector3(1f, 5f, 10f), stone);
            CreateBox("Exit Approach Right", new Vector3(7f, 2.5f, 23f), new Vector3(1f, 5f, 10f), stone);
            CreateLight("Entry Fill Light", new Vector3(0f, 4f, -19f), new Color(.45f, .62f, 1f), 15f, 2f);
            CreateLight("Courtyard Moonlight", new Vector3(0f, 9f, 0f), new Color(.55f, .70f, 1f), 20f, 2f);
            CreateBrazier("Entry Brazier Left", new Vector3(-3f, 0f, -17f), warning);
            CreateBrazier("Entry Brazier Right", new Vector3(3f, 0f, -17f), warning);
            CreateBrazier("Courtyard Brazier", new Vector3(-8f, 0f, -1f), warning);
        }

        private static Transform CreatePlayer(Vector3 spawn)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.SetPositionAndRotation(new Vector3(spawn.x, 0f, spawn.z), Quaternion.identity);
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.center = new Vector3(0f, .9f, 0f);
            Material cloak = MakeMaterial("Explorer Cloak", new Color(.07f, .18f, .28f));
            Material trim = MakeMaterial("Explorer Trim", new Color(.24f, .68f, .78f), true);
            ExplorerVisual.Create(player.transform, cloak, trim);

            Transform pivot = new GameObject("Camera Pivot").transform;
            pivot.SetParent(player.transform, false);
            pivot.localPosition = new Vector3(0f, 1.6f, 0f);
            Camera camera = new GameObject("Player Camera").AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.transform.SetParent(pivot, false);
            camera.transform.localPosition = new Vector3(.65f, .15f, -4.25f);
            camera.nearClipPlane = .05f;
            camera.fieldOfView = 58f;
            camera.clearFlags = CameraClearFlags.Skybox;
            PlayerController playerController = player.AddComponent<PlayerController>();
            playerController.Configure(pivot);
            CameraFollow follow = pivot.gameObject.AddComponent<CameraFollow>();
            follow.Configure(player.transform, new Vector3(0f, 1.55f, 0f));
            return player.transform;
        }

        private static GuardianAI CreateGuardian(Transform player, Material material)
        {
            GameObject guardian = CreateCapsule("Guardian", new Vector3(6f, 1f, 1f), material);
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
            Transform[] patrol =
            {
                CreateMarker("Guardian Waypoint 1", new Vector3(-6f, 0f, 1f), material),
                CreateMarker("Guardian Waypoint 2", new Vector3(6f, 0f, 1f), material),
                CreateMarker("Guardian Waypoint 3", new Vector3(6f, 0f, 11f), material),
                CreateMarker("Guardian Waypoint 4", new Vector3(-6f, 0f, 11f), material)
            };
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
            GameObject checkpoint = CreateCylinder(name, position + Vector3.up * .05f, new Vector3(2f, .1f, 2f), material);
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

        private static void CreateCore(string name, Vector3 position, string coreId, Material material)
        {
            GameObject core = CreateSphere(name, position, Vector3.one * .8f, material);
            SphereCollider collider = core.GetComponent<SphereCollider>();
            collider.isTrigger = true;
            core.AddComponent<Rigidbody>().isKinematic = true;
            Collectible collectible = core.AddComponent<Collectible>();
            collectible.Configure(coreId);
            core.AddComponent<RuneFloat>();
            CreateLight(name + " Glow", position + Vector3.up, Accent, 7f, 4f);
            GameObject beam = CreateCylinder(name + " Light Beam", position + Vector3.up * 3.5f, new Vector3(.22f, 3.5f, .22f), material);
            beam.GetComponent<Collider>().enabled = false;
        }

        private static void CreateExit(Vector3 position, Material lockedMaterial, Material unlockedMaterial)
        {
            GameObject gate = new GameObject("Sealed Exit Gate");
            gate.transform.position = position;
            BoxCollider trigger = gate.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(5f, 4f, 1f);
            gate.AddComponent<Rigidbody>().isKinematic = true;
            GameObject locked = CreateBox("Locked Gate Bars", position, new Vector3(5f, 4f, .5f), lockedMaterial);
            GameObject opened = CreateBox("Open Exit Beacon", position + Vector3.up * 3f, new Vector3(5f, .25f, .5f), unlockedMaterial);
            CreateCastleProp("Sealed Castle Gate", RuinsAssetId.Gate, position, Quaternion.identity, new Vector3(2.1f, 2.1f, 2.1f), lockedMaterial);
            CreateLight("Exit Beacon", position + Vector3.up * 2f, Accent, 7f, 10f);
            ExitGate exit = gate.AddComponent<ExitGate>();
            exit.Configure(locked, opened);
        }

        private static void CreateBrazier(string name, Vector3 position, Material material)
        {
            CreateCylinder(name + " Base", position + Vector3.up * .35f, new Vector3(.45f, .35f, .45f), material);
            CreateLight(name + " Flame", position + Vector3.up * 1.4f, Warning, 10f, 3f);
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

        private static GameObject CreateBox(string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obj.name = name;
            obj.transform.SetPositionAndRotation(position, Quaternion.identity);
            obj.transform.localScale = scale;
            obj.GetComponent<Renderer>().material = material;
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

        private static GameObject CreateCapsule(string name, Vector3 position, Material material)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            obj.name = name;
            obj.transform.position = position;
            obj.GetComponent<Renderer>().material = material;
            return obj;
        }

        private static void CreatePillar(string name, Vector3 position, Material material) => CreateCylinder(name, position, new Vector3(1f, 2.5f, 1f), material);

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

        private static void CreateLight(string name, Vector3 position, Color color, float range, float intensity)
        {
            Light light = new GameObject(name).AddComponent<Light>();
            light.type = LightType.Point;
            light.transform.position = position;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
        }
    }
}
