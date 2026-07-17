# Moonlit Citadel Visual Rebuild Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Deliver a Windows-playable Moonlit Citadel visual pass that replaces the broken magenta HUD and greybox presentation with readable third-person stealth composition.

**Architecture:** Keep game-state, save, score, player movement, and guardian behaviour unchanged. Replace only their presentation adapters: a build-safe HUD, player visual hierarchy, asset catalog, and scene dressing. The current collision greybox remains the navigation source while CC0 castle prefabs provide visual geometry; all runtime assets load through explicit `Resources` paths and fall back safely.

**Tech Stack:** Unity 6.3 LTS, built-in render pipeline for this pass, C#, uGUI, Unity Test Framework EditMode tests, Unity Windows 64-bit BuildPipeline, Kenney Castle Kit (CC0).

## Global Constraints

- Use only Unity built-in content, self-authored materials/effects, and recorded CC0 Kenney Castle Kit models.
- Do not migrate to URP in this pass; verify the existing pipeline rather than introduce a second rendering risk.
- Do not remove existing collection, exit, guardian, save, score, tutorial, or checkpoint behaviours.
- A build fails acceptance if it displays magenta material, a black-only scene, missing text, missing player/core/guardian, or a shader/material/resource exception.
- Stage only intended files. Do not commit `Library`, `Temp`, `Builds`, recordings, or the unused `Assets/ThirdParty/` source package.

---

## File Structure

- `Assets/Scripts/CanvasHud.cs` — compact, build-safe UI presentation; no runtime-generated legacy font dependency.
- `Assets/Scripts/HudTheme.cs` — central colour, copy, and state-to-colour mapping for HUD and guardian alerts.
- `Assets/Scripts/RuntimeMaterialLibrary.cs` — resolves authored material templates and reports safe fallback use.
- `Assets/Scripts/RuinsAssetCatalog.cs` — maps each used Kenney resource to a stable `Resources` path.
- `Assets/Scripts/RuinSceneBootstrap.cs` — creates the moonlit composition, player visual hierarchy, lights, CC0 prop instances, and gameplay adapters.
- `Assets/Scripts/GuardianVisionLight.cs` — maps guardian state to blue/yellow/red vision light feedback.
- `Assets/Resources/KenneyCastle/*.fbx` — selected lightweight CC0 visual props only.
- `Assets/Resources/Materials/*.mat` — explicit runtime scene and HUD material references packaged into the player.
- `Assets/Tests/EditMode/HudThemeTests.cs` — HUD copy/state and safe colour tests.
- `Assets/Tests/EditMode/RuinsAssetCatalogTests.cs` — selected resource paths and null-safe catalog tests.
- `Documentation/ASSET_LICENSES.md` — source, creator, licence, and usage entries.
- `Documentation/TEST_PLAN.md` — manual Windows visual test checklist and screenshots.

## Task 1: Replace the fragile HUD material and encoding path

**Files:**
- Create: `Assets/Scripts/HudTheme.cs`
- Create: `Assets/Tests/EditMode/HudThemeTests.cs`
- Modify: `Assets/Scripts/CanvasHud.cs`
- Modify: `Assets/Editor/BuildCommands.cs`

**Interfaces:**
- Produces `HudTheme.GetAlert(GuardianState state) : HudAlert`.
- Produces `HudAlert.Message : string` and `HudAlert.Color : Color`.
- `CanvasHud.Configure(GuardianAI configuredGuardian)` remains unchanged for `RuinSceneBootstrap`.

- [ ] **Step 1: Write the failing state-to-copy test**

```csharp
[Test]
public void ChaseAlert_UsesRedDetectedCopy()
{
    HudAlert alert = HudTheme.GetAlert(GuardianState.Chase);
    Assert.That(alert.Message, Is.EqualTo("DETECTED — BREAK LINE OF SIGHT"));
    Assert.That(alert.Color.r, Is.GreaterThan(.9f));
    Assert.That(alert.Color.g, Is.LessThan(.4f));
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run:
```powershell
& 'E:\Unity\Editor\Unity.exe' -batchmode -nographics -projectPath 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' -runTests -testPlatform EditMode -testResults 'C:\Users\HP\Desktop\3\TestResults\HudTheme_Red.xml' -logFile 'C:\Users\HP\Desktop\3\TestResults\HudTheme_Red.log'
```

Expected: compilation failure stating `HudTheme` is undefined.

- [ ] **Step 3: Add the theme model and use only ASCII/UTF-8-safe UI strings**

```csharp
public readonly struct HudAlert
{
    public readonly string Message;
    public readonly Color Color;
    public HudAlert(string message, Color color) { Message = message; Color = color; }
}

public static class HudTheme
{
    public static HudAlert GetAlert(GuardianState state) => state switch
    {
        GuardianState.Chase or GuardianState.Capture => new HudAlert("DETECTED — BREAK LINE OF SIGHT", new Color(1f, .22f, .18f)),
        GuardianState.Investigate or GuardianState.Search => new HudAlert("SEARCHING — STAY IN COVER", new Color(1f, .76f, .18f)),
        _ => new HudAlert("SAFE — WATCH THE BLUE VISION CONE", new Color(.38f, .86f, 1f))
    };
}
```

Update `CanvasHud` to create exactly two transparent panels, use a material explicitly created by `BuildCommands.EnsureRuntimeMaterial`, and remove malformed bilingual literals and the oversized bottom controls panel. Keep controls as a temporary tutorial-only line.

- [ ] **Step 4: Run tests and perform an editor play check**

Run the Step 2 command with `HudTheme_Green.xml`. Expected: all EditMode tests pass and no compiler errors. In Unity Play mode, verify objective copy, 0/3 counter, and red/yellow/blue alert copy have normal text and no magenta images.

- [ ] **Step 5: Commit the UI reliability slice**

```powershell
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' add -- Assets/Scripts/HudTheme.cs Assets/Scripts/CanvasHud.cs Assets/Editor/BuildCommands.cs Assets/Tests/EditMode/HudThemeTests.cs Assets/Resources/Materials
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' commit -m 'fix: package readable build-safe stealth hud'
```

## Task 2: Make the player a readable explorer silhouette

**Files:**
- Create: `Assets/Scripts/ExplorerVisual.cs`
- Create: `Assets/Tests/EditMode/ExplorerVisualTests.cs`
- Modify: `Assets/Scripts/RuinSceneBootstrap.cs`

**Interfaces:**
- Produces `ExplorerVisual.Create(Transform player, Material cloak, Material trim) : GameObject`.
- The returned root is named `Explorer Visual` and contains `Cloak`, `Torso`, `Head`, and `Shoulder Light` children.

- [ ] **Step 1: Write a failing hierarchy test**

```csharp
[Test]
public void Create_BuildsNamedExplorerSilhouette()
{
    var root = new GameObject("Player");
    GameObject explorer = ExplorerVisual.Create(root.transform, new Material(Shader.Find("Standard")), new Material(Shader.Find("Standard")));
    Assert.That(explorer.transform.Find("Cloak"), Is.Not.Null);
    Assert.That(explorer.transform.Find("Shoulder Light"), Is.Not.Null);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run the same Unity EditMode command with `ExplorerVisual_Red.xml`. Expected: `ExplorerVisual` is undefined.

- [ ] **Step 3: Implement a compact hooded explorer composition**

Create a child hierarchy made from low-poly Unity primitives: a tapered cloak, torso, hood/head, small backpack and a low-intensity cyan rim light. Disable primitive colliders. Use `RuinSceneBootstrap.CreatePlayer` to replace the single `Explorer Visual` capsule with `ExplorerVisual.Create`, preserve `CharacterController`, and retain the shoulder camera local position `(0.65, 0.15, -4.25)`.

- [ ] **Step 4: Verify silhouette and camera**

Run the EditMode suite. In Play mode, stand in the entry arch and confirm the explorer is visible below the camera without filling more than one third of the vertical frame.

- [ ] **Step 5: Commit**

```powershell
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' add -- Assets/Scripts/ExplorerVisual.cs Assets/Scripts/RuinSceneBootstrap.cs Assets/Tests/EditMode/ExplorerVisualTests.cs
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' commit -m 'feat: add readable hooded explorer silhouette'
```

## Task 3: Dress the collision layout with selected CC0 castle assets

**Files:**
- Modify: `Assets/Scripts/RuinsAssetCatalog.cs`
- Modify: `Assets/Scripts/RuinSceneBootstrap.cs`
- Modify: `Assets/Tests/EditMode/RuinsAssetCatalogTests.cs`
- Add: `Assets/Resources/KenneyCastle/gate.fbx`, `rocks-large.fbx`, `stairs-stone.fbx`, `tower-square-mid-open.fbx`, `wall-corner.fbx`, `wall-pillar.fbx`, `wall.fbx` and Unity `.meta` files
- Modify: `Documentation/ASSET_LICENSES.md`

**Interfaces:**
- `RuinsAssetCatalog.GetPath(RuinsAssetId assetId) : string` returns `KenneyCastle/<file-name-without-extension>`.
- `RuinsAssetCatalog.RequiredVisualAssets : IReadOnlyList<RuinsAssetId>` contains the seven selected prop identifiers.
- `CreateCastleProp(string, RuinsAssetId, Vector3, Quaternion, Vector3, Material) : GameObject` returns `null` when an optional visual prefab is unavailable and must never prevent gameplay bootstrap.

- [ ] **Step 1: Add a failing fallback test**

```csharp
[Test]
public void RequiredVisualAssets_ContainsTheSealedGate()
{
    Assert.That(RuinsAssetCatalog.RequiredVisualAssets, Does.Contain(RuinsAssetId.Gate));
}
```

- [ ] **Step 2: Run it to verify it fails**

Run the EditMode command with `CastleCatalog_Red.xml`. Expected: `RequiredVisualAssets` is undefined.

- [ ] **Step 3: Implement only the selected props and scene composition**

Add `public static readonly RuinsAssetId[] RequiredVisualAssets = { RuinsAssetId.Wall, RuinsAssetId.Corner, RuinsAssetId.Pillar, RuinsAssetId.Tower, RuinsAssetId.Gate, RuinsAssetId.Rocks, RuinsAssetId.Stairs };`. Keep primitive walls/floor as collision and NavMesh sources. Instantiate the selected visual props over them: entry towers at `(-4.5,0,-22)` and `(4.5,0,-22)`, central altar visual at `(0,0,-3)`, pillar visuals at `(-7,0,-7)`, `(7,0,-7)`, `(-7,0,10)`, `(7,0,10)`, side steps at `(-17,0,-1)`, rubble at `(-4,0,5)`, and a gate at the sealed exit. Apply the authored cool-stone or dark-stone materials to child renderers. Keep `Resources.Load<GameObject>` null-safe.

- [ ] **Step 4: Record licence evidence and verify resource paths**

Add the Castle Kit source URL, Kenney creator, CC0 licence, exact models used, and purpose to `Documentation/ASSET_LICENSES.md`. Run EditMode tests; expected: all core, save, guardian, HUD, explorer, and catalog tests pass.

- [ ] **Step 5: Commit only used assets**

```powershell
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' add -- Assets/Resources/KenneyCastle Assets/Scripts/RuinsAssetCatalog.cs Assets/Scripts/RuinSceneBootstrap.cs Assets/Tests/EditMode/RuinsAssetCatalogTests.cs Documentation/ASSET_LICENSES.md .gitignore
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' commit -m 'feat: dress moonlit ruins with licensed castle props'
```

## Task 4: Compose lighting, core, exit, and guardian readability

**Files:**
- Modify: `Assets/Scripts/RuinSceneBootstrap.cs`
- Modify: `Assets/Scripts/GuardianVisionLight.cs`
- Create: `Assets/Tests/EditMode/VisualThemeTests.cs`

**Interfaces:**
- `GuardianVisionLight.GetColor(GuardianState state) : Color` returns patrol blue, investigate/search yellow, and chase/capture red.
- `RuinSceneBootstrap` continues to call `ConfigureMoonlitAtmosphere()` before it creates visual geometry.

- [ ] **Step 1: Write failing colour tests**

```csharp
[Test]
public void PatrolVision_IsBlueAndChaseVision_IsRed()
{
    Assert.That(GuardianVisionLight.GetColor(GuardianState.Patrol).b, Is.GreaterThan(.7f));
    Assert.That(GuardianVisionLight.GetColor(GuardianState.Chase).r, Is.GreaterThan(.9f));
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run the EditMode command with `VisualTheme_Red.xml`. Expected: `GetColor` is missing.

- [ ] **Step 3: Implement moonlit composition values**

Set sky/equator/ground ambient colours to cool blue-grey, fog density no greater than `.0045`, moon intensity near `1.05`, amber braziers at entry/courtyard/altar, cyan core light shafts, and a bright cyan exit beacon only after unlock. Move the guardian out of direct entry sightline and mount its spotlight at head height. Implement `GetColor` and make the spotlight use the same state colour.

- [ ] **Step 4: Verify visual-state alignment**

Run tests. In Play mode, trigger noise with `Q`, then view chase: guardian spotlight must visibly transition blue → yellow → red, while walls still block line of sight.

- [ ] **Step 5: Commit**

```powershell
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' add -- Assets/Scripts/RuinSceneBootstrap.cs Assets/Scripts/GuardianVisionLight.cs Assets/Tests/EditMode/VisualThemeTests.cs
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' commit -m 'feat: compose readable moonlit stealth lighting'
```

## Task 5: Enforce real Windows visual acceptance

**Files:**
- Modify: `Assets/Editor/BuildCommands.cs`
- Modify: `Documentation/TEST_PLAN.md`
- Create: `Documentation/EVIDENCE/README.md`

**Interfaces:**
- `BuildCommands.BuildWindowsPlayer()` continues to build `Assets/Scenes/ProductionRuins.unity` to `Builds/EchoesOfTheRuins.exe`.
- `RuntimeMaterialLibrary.HudTemplatePath : string` returns `Materials/HudRuntime`.
- `TEST_PLAN.md` contains a manual result field for `Pink UI`, `Camera readable`, `Explorer visible`, `Guardian visible`, `Core visible`, and `Exit visible`.

- [ ] **Step 1: Write the failing editor-material assertion**

```csharp
[Test]
public void RuntimeMaterialLibrary_DeclaresDedicatedHudTemplate()
{
    Assert.That(RuntimeMaterialLibrary.HudTemplatePath, Is.EqualTo("Materials/HudRuntime"));
}
```

- [ ] **Step 2: Run tests to verify the template path is absent before implementation**

Run the EditMode command with `BuildVisual_Red.xml`. Expected: compilation failure because `HudTemplatePath` is undefined.

- [ ] **Step 3: Make the build command explicitly create and retain the materials**

Extend `EnsureRuntimeMaterial()` to create a named `RuinsRuntime.mat` and `HudRuntime.mat` under `Assets/Resources/Materials` when missing. Do not call `Shader.Find` from `CanvasHud`. Add the required materials to source control.

- [ ] **Step 4: Build and inspect the actual application**

Run:
```powershell
& 'E:\Unity\Editor\Unity.exe' -batchmode -nographics -projectPath 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' -executeMethod EchoesOfTheRuins.Editor.BuildCommands.BuildWindowsPlayer -quit -logFile 'C:\Users\HP\Desktop\3\TestResults\MoonlitCitadelBuild.log'
Start-Process -FilePath 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild\Builds\EchoesOfTheRuins.exe' -ArgumentList '-force-d3d11'
```

Expected: build reports `Result: Success`; launched window has no magenta panel, shows explorer/route/core/guardian, and Player.log contains no `Exception`, `Shader.Find`, `Resources.Load`, `NullReference`, or `Failed to load` lines.

- [ ] **Step 5: Capture coursework evidence and commit**

Capture entry, altar, cover, patrol cone, side chamber, and unlocked exit images in `Documentation/EVIDENCE/` (or record their accessible external location if files are too large). Update `TEST_PLAN.md` with date, build number, and pass/fail rows.

```powershell
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' add -- Assets/Editor/BuildCommands.cs Assets/Resources/Materials Assets/Tests/EditMode Documentation/TEST_PLAN.md Documentation/EVIDENCE
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' commit -m 'test: verify moonlit citadel Windows presentation'
```

## Task 6: Coursework regression and delivery checkpoint

**Files:**
- Modify: `Documentation/REPORT_OUTLINE.md`
- Modify: `Documentation/TEST_PLAN.md`
- Modify: `Documentation/ASSET_LICENSES.md`

- [ ] **Step 1: Run the full EditMode suite**

Run the Unity EditMode command with `MoonlitCitadel_Final.xml`. Expected: all tests pass with `failed="0"`.

- [ ] **Step 2: Complete three manual gameplay runs**

Perform and log: fresh run with no alert, capture then continue from checkpoint, quit/reopen and restore saved progress. Expected: each reaches 3/3, unlocks exit, and reaches the end screen.

- [ ] **Step 3: Update report evidence map**

Add one row each for character/camera, stealth cone, echo stone, checkpoint/save, core/exit, and final result. Each row must name its screenshot/video and the report section it proves.

- [ ] **Step 4: Commit coursework evidence only after the three runs pass**

```powershell
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' add -- Documentation/REPORT_OUTLINE.md Documentation/TEST_PLAN.md Documentation/ASSET_LICENSES.md Documentation/EVIDENCE
git -C 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild' commit -m 'docs: record moonlit citadel coursework evidence'
```

## Plan self-review

- Spec coverage: Tasks 1 and 5 prevent magenta UI; Task 2 replaces the capsule; Task 3 implements licensed architecture; Task 4 sets the blue/amber/cyan/gold stealth language; Tasks 5–6 produce Windows and coursework evidence.
- Placeholders: no deferred requirements or undefined path names are used.
- Type consistency: `HudTheme.GetAlert`, `ExplorerVisual.Create`, `RuinsAssetCatalog.GetPath`, and `GuardianVisionLight.GetColor` are introduced before dependent steps and use the same signatures throughout.
