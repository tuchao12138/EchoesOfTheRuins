# Polished Ruins Visual Rebuild Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace prototype-looking visible walls and character presentation with a readable, polished normal-proportion moonlit-ruins scene while preserving the stealth game systems.

**Architecture:** Keep invisible primitive colliders as the single source of truth for player movement and runtime NavMesh. Add a dedicated `RuinFacadeBuilder` responsible only for assembling visible ruin modules; `RuinSceneBootstrap` supplies route coordinates and materials but no longer creates visible structural cubes. Keep `CharacterAssetCatalog` as the sole mapping from player/guardian role to a verified `Resources` character mesh and texture.

**Tech Stack:** Unity 6.3 LTS, C#, UnityEngine, Unity Test Framework, runtime `Resources` loading, CC0 Quaternius/Kenney assets and a self-made moonlit stone texture.

## Global Constraints

- Target Unity 6.3 LTS and Windows 64-bit only.
- Preserve `GameManager`, `GuardianAI`, `PlayerController`, `Collectible`, `ExitGate`, `CanvasHud`, checkpoints and local JSON save behaviour.
- Visible architecture must never use the renderer of a collider-only cube.
- Use only self-made or recorded permissive-license assets; retain licence provenance in `Documentation/ASSET_LICENSES.md`.
- Do not introduce multiplayer, combat, online services, a second map, or a new package dependency.

---

## File Structure

- `Assets/Scripts/RuinFacadeBuilder.cs` — builds visible wall, arch, pillar-base and rubble arrangements from imported modular meshes.
- `Assets/Scripts/RuinSceneBootstrap.cs` — owns gameplay coordinates, invisible collision boxes, materials, lighting and calls into `RuinFacadeBuilder`.
- `Assets/Scripts/CharacterAssetCatalog.cs` — maps `Explorer` and `Guardian` to normal-proportion resource paths and texture paths.
- `Assets/Scripts/RuntimeMaterialLibrary.cs` — creates safe materials and binds character/floor textures.
- `Assets/Tests/EditMode/RuinFacadeBuilderTests.cs` — proves the facade builder creates visible architecture without colliders.
- `Assets/Tests/EditMode/CharacterAssetCatalogTests.cs` — proves each role resolves both model and texture paths.
- `Documentation/ASSET_LICENSES.md` — stores source, author, licence and purpose for every visual asset.

### Task 1: Define verified resource mappings

**Files:**
- Modify: `Assets/Scripts/CharacterAssetCatalog.cs`
- Modify: `Assets/Tests/EditMode/CharacterAssetCatalogTests.cs`
- Modify: `Documentation/ASSET_LICENSES.md`

**Interfaces:**
- Produces: `CharacterAssetCatalog.GetPath(CharacterAssetId)` and `CharacterAssetCatalog.GetTexturePath(CharacterAssetId)`, both returning non-empty `Resources` paths.
- Consumes: `CharacterAssetId.Explorer` and `CharacterAssetId.Guardian` from the existing catalog.

- [ ] **Step 1: Write the failing resource-path test**

```csharp
[TestCase(CharacterAssetId.Explorer)]
[TestCase(CharacterAssetId.Guardian)]
public void EveryCharacterRole_HasAModelAndTexturePath(CharacterAssetId assetId)
{
    Assert.That(CharacterAssetCatalog.GetPath(assetId), Is.Not.Empty);
    Assert.That(CharacterAssetCatalog.GetTexturePath(assetId), Is.Not.Empty);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run in Unity Test Runner: `Window > General > Test Runner > EditMode > CharacterAssetCatalogTests`.

Expected: FAIL because `GetTexturePath` does not yet exist.

- [ ] **Step 3: Implement explicit model and texture resolution**

```csharp
public static string GetTexturePath(CharacterAssetId assetId)
{
    return assetId == CharacterAssetId.Explorer
        ? "Characters/Rogue_Texture"
        : "Characters/Warrior_Texture";
}
```

Keep `GetPath` mapped to `Characters/Rogue` and `Characters/Warrior` until a higher-quality, normal-proportion resource is successfully imported and verified in the Unity Inspector. Do not switch to the downloaded KayKit chibi pack.

- [ ] **Step 4: Record the current visual assets**

Add entries for Quaternius Low Poly RPG Characters, Kenney Modular Dungeon, the self-made `MoonlitRuneStone.png`, including source URL, CC0/self-made status and in-game use.

- [ ] **Step 5: Run the test to verify it passes**

Run the same Unity Test Runner selection.

Expected: PASS for both roles.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/CharacterAssetCatalog.cs Assets/Tests/EditMode/CharacterAssetCatalogTests.cs Documentation/ASSET_LICENSES.md
git commit -m "docs: verify visual asset mappings"
```

### Task 2: Add a facade-only modular architecture builder

**Files:**
- Create: `Assets/Scripts/RuinFacadeBuilder.cs`
- Create: `Assets/Tests/EditMode/RuinFacadeBuilderTests.cs`
- Modify: `Assets/Scripts/RuinSceneBootstrap.cs`

**Interfaces:**
- Produces: `RuinFacadeBuilder.CreateFacade(string, Vector3, Quaternion, Vector3, Material)` returning a root object that contains visible renderers and no colliders.
- Consumes: `DungeonAssetCatalog.GetPath(DungeonAssetId)` and a supplied stone material.

- [ ] **Step 1: Write the failing facade test**

```csharp
[Test]
public void CreateFacade_CreatesVisibleRenderersWithoutNavigationColliders()
{
    Material material = RuntimeMaterialLibrary.Create("test", Color.gray, false);
    GameObject facade = RuinFacadeBuilder.CreateFacade("test", Vector3.zero, Quaternion.identity, Vector3.one, material);

    Assert.That(facade.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(0));
    Assert.That(facade.GetComponentsInChildren<Collider>(true), Has.None.Matches<Collider>(collider => collider.enabled));

    Object.DestroyImmediate(facade);
    Object.DestroyImmediate(material);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run in Unity Test Runner: `EditMode > RuinFacadeBuilderTests`.

Expected: FAIL because `RuinFacadeBuilder` does not exist.

- [ ] **Step 3: Implement the facade builder**

```csharp
public static GameObject CreateFacade(string name, Vector3 position, Quaternion rotation, Vector3 scale, Material material)
{
    GameObject root = new GameObject(name);
    root.transform.SetPositionAndRotation(position, rotation);
    root.transform.localScale = scale;
    AddModule(root.transform, DungeonAssetId.Wall, Vector3.zero, material);
    AddModule(root.transform, DungeonAssetId.Corner, new Vector3(-1.4f, 0f, 0f), material);
    AddModule(root.transform, DungeonAssetId.Corner, new Vector3(1.4f, 0f, 0f), material);
    return root;
}

private static void AddModule(Transform parent, DungeonAssetId assetId, Vector3 localPosition, Material material)
{
    GameObject template = Resources.Load<GameObject>(DungeonAssetCatalog.GetPath(assetId));
    if (template == null) return;
    GameObject module = Object.Instantiate(template, parent);
    module.transform.localPosition = localPosition;
    foreach (Renderer renderer in module.GetComponentsInChildren<Renderer>(true)) renderer.material = material;
    foreach (Collider collider in module.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
}
```

Keep module geometry as the visible layer only. Never add the root or imported modules to the NavMesh source.

- [ ] **Step 4: Replace visible structural calls in the bootstrap**

Replace each visible `CreateDungeonProp(... DungeonAssetId.Wall ...)` call for the entry and courtyard with `RuinFacadeBuilder.CreateFacade(...)`. Keep the existing `CreateBox(..., visible: false)` calls unchanged; those are the navigation layer.

- [ ] **Step 5: Run the test to verify it passes**

Run `EditMode > RuinFacadeBuilderTests`.

Expected: PASS; test root has at least one renderer and no enabled colliders.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/RuinFacadeBuilder.cs Assets/Tests/EditMode/RuinFacadeBuilderTests.cs Assets/Scripts/RuinSceneBootstrap.cs
git commit -m "feat: replace visible collision walls with ruin facades"
```

### Task 3: Compose the entry route and visual landmarks

**Files:**
- Modify: `Assets/Scripts/RuinSceneBootstrap.cs`
- Modify: `Assets/Scripts/DungeonAssetCatalog.cs`
- Modify: `Assets/Tests/EditMode/DungeonAssetCatalogTests.cs`

**Interfaces:**
- Produces: a safe entry containing a lit checkpoint, two facade walls, a distant cyan core landmark, warm braziers, and a guarded courtyard route.
- Consumes: `RuinFacadeBuilder.CreateFacade`, existing `CreateBrazier`, `CreateCore` and invisible collision boxes.

- [ ] **Step 1: Write a failing catalog test for route modules**

```csharp
[TestCase(DungeonAssetId.Wall)]
[TestCase(DungeonAssetId.Corner)]
[TestCase(DungeonAssetId.Stairs)]
[TestCase(DungeonAssetId.Gate)]
public void RouteModule_HasAResourcesPath(DungeonAssetId assetId)
{
    Assert.That(DungeonAssetCatalog.GetPath(assetId), Does.StartWith("KenneyDungeon/"));
}
```

- [ ] **Step 2: Run the test to verify it passes before composition work**

Run `EditMode > DungeonAssetCatalogTests`.

Expected: PASS. This protects resource names before visual placement is changed.

- [ ] **Step 3: Build a layered entry composition**

In `CreateEnvironment`, leave collision boxes invisible and place:

```csharp
RuinFacadeBuilder.CreateFacade("Entry West Facade", new Vector3(-4.25f, 0f, -18f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1.8f, 1.8f, 2.5f), darkStone);
RuinFacadeBuilder.CreateFacade("Entry East Facade", new Vector3(4.25f, 0f, -18f), Quaternion.Euler(0f, -90f, 0f), new Vector3(1.8f, 1.8f, 2.5f), darkStone);
CreateDungeonProp("Entry Threshold", DungeonAssetId.Stairs, new Vector3(0f, 0f, -16.5f), Quaternion.identity, new Vector3(1.35f, 1.0f, 1.1f), stone);
CreateBrazier("Entry Brazier Left", new Vector3(-3f, 0f, -17f), warning);
CreateBrazier("Entry Brazier Right", new Vector3(3f, 0f, -17f), warning);
```

Then place a dark facade behind the Core 1 dais and keep the cyan light beam visible above the core. Do not move the core ID or checkpoint positions.

- [ ] **Step 4: Play-test the visual route**

In Unity, stop Play, wait for recompilation, then press Play. Verify from spawn: player, warm braziers, cyan objective and a route around cover are visible; no visible cube wall remains.

- [ ] **Step 5: Commit**

```powershell
git add Assets/Scripts/RuinSceneBootstrap.cs Assets/Scripts/DungeonAssetCatalog.cs Assets/Tests/EditMode/DungeonAssetCatalogTests.cs
git commit -m "feat: compose readable moonlit ruins entry"
```

### Task 4: Refine character material and camera presentation

**Files:**
- Modify: `Assets/Scripts/RuinSceneBootstrap.cs`
- Modify: `Assets/Scripts/RuntimeMaterialLibrary.cs`
- Modify: `Assets/Tests/EditMode/RuntimeMaterialLibraryTests.cs`

**Interfaces:**
- Produces: player and guardian meshes with an explicitly bound texture; a camera positioned behind and above the explorer at a 5.75-unit shoulder distance.
- Consumes: `CharacterAssetCatalog.GetPath`, `CharacterAssetCatalog.GetTexturePath`, `RuntimeMaterialLibrary.CreateTextured`.

- [ ] **Step 1: Write a texture safety regression test**

```csharp
[Test]
public void CreateTextured_AllowsAnAbsentTextureWithoutCreatingAnErrorShader()
{
    Material material = RuntimeMaterialLibrary.CreateTextured("fallback", null);
    Assert.That(material.shader.name, Is.Not.EqualTo("Hidden/InternalErrorShader"));
    Object.DestroyImmediate(material);
}
```

- [ ] **Step 2: Run the baseline material tests**

Run `EditMode > RuntimeMaterialLibraryTests`.

Expected: PASS. This establishes that refactoring character texture lookup cannot regress material fallback behaviour.

- [ ] **Step 3: Make texture binding role-driven**

```csharp
Texture2D texture = Resources.Load<Texture2D>(CharacterAssetCatalog.GetTexturePath(assetId));
Material characterMaterial = RuntimeMaterialLibrary.CreateTextured(visualName + " Material", texture);
```

In `CreateCharacterVisual`, load textures through `CharacterAssetCatalog.GetTexturePath(assetId)` rather than an inline conditional. Keep the camera at `new Vector3(1.05f, .4f, -5.75f)` and FOV 63.

- [ ] **Step 4: Run material and catalog tests**

Run `EditMode > RuntimeMaterialLibraryTests` and `EditMode > CharacterAssetCatalogTests`.

Expected: all selected tests PASS.

- [ ] **Step 5: Commit**

```powershell
git add Assets/Scripts/RuinSceneBootstrap.cs Assets/Scripts/RuntimeMaterialLibrary.cs Assets/Tests/EditMode/RuntimeMaterialLibraryTests.cs
git commit -m "fix: make character presentation asset-driven"
```

### Task 5: Regression, evidence and final visual gate

**Files:**
- Modify: `Documentation/ASSET_LICENSES.md`
- Create: `Documentation/TEST_RESULTS.md`

**Interfaces:**
- Consumes: the completed scene, existing game systems and EditMode test suites.
- Produces: recorded visual evidence and a test result table for the coursework appendix.

- [ ] **Step 1: Run all EditMode tests**

Run in Unity: `Window > General > Test Runner > EditMode > Run All`.

Expected: all tests pass. If a test fails, record the failing name and fix it before continuing.

- [ ] **Step 2: Run three focused PlayMode checks**

1. Start a new game; verify first core collection updates HUD from `0/3` to `1/3`.
2. Let the guardian detect and capture the player; verify respawn at checkpoint.
3. Collect three cores; verify exit unlocks and victory screen appears.

- [ ] **Step 3: Capture visual evidence**

Capture six PNGs: entry composition, courtyard guard route, shadow-side-room core, echo-stone investigate moment, unlocked exit and results screen. Store them outside `Assets` in `Documentation/Evidence/`.

- [ ] **Step 4: Record test results**

Create `Documentation/TEST_RESULTS.md` with columns `ID`, `Scenario`, `Expected`, `Observed`, `Status`, `Evidence`. Include the three PlayMode flows and the entry composition acceptance criteria.

- [ ] **Step 5: Commit**

```powershell
git add Documentation/ASSET_LICENSES.md Documentation/TEST_RESULTS.md Documentation/Evidence
git commit -m "test: record polished ruins visual evidence"
```

## Plan Self-Review

- **Spec coverage:** Task 1 covers verified assets and licence evidence; Task 2 separates visible facades from navigation colliders; Task 3 delivers entry composition and landmarks; Task 4 covers character and camera material clarity; Task 5 covers gameplay regression, screenshot proof and report evidence.
- **Placeholder scan:** No task depends on an unlicensed or undefined asset. The unavailable chibi asset is explicitly excluded.
- **Type consistency:** `CreateFacade` is defined in Task 2 and consumed in Task 3. `GetTexturePath` is defined in Task 1 and consumed in Task 4. `CreateTextured` is defined in the existing material library and made null-safe in Task 4.
