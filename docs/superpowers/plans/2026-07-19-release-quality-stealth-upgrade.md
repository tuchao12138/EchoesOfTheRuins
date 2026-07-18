# Echoes of the Ruins Release-Quality Stealth Upgrade Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a release-candidate 8–12 minute third-person stealth slice with readable objectives, camera-relative locomotion, semantic animation, telegraphed guardian attacks, coherent HUD/world guidance, save migration, and verified Windows flows.

**Architecture:** Keep the existing `CharacterController`, static `ProductionRuins` scene, NavMesh, save service, and score systems. Add pure deterministic models for locomotion, attack timing, objectives, and threat selection; MonoBehaviours translate Unity input/perception into those models and publish events; animation, HUD, audio, VFX, and checkpoint reset consume events without owning gameplay decisions.

**Tech Stack:** Unity 6.3 LTS (`6000.3.20f1`), C# 9, URP, AI Navigation/NavMesh, Unity Playables animation graph, NUnit Unity Test Framework, local JSON v4, Windows x64 D3D11.

## Global Constraints

- Target Unity version is `6000.3.20f1`; renderer is URP; Windows build uses x64 D3D11.
- The release contains one 8–12 minute map and two guardians.
- Guardians perform nonlethal attack/capture; the player has no weapon, health combat, block, or kill action.
- Do not add multiplayer, accounts, servers, cloud saves, online leaderboards, a second map, or procedural generation.
- Use only bundled CC0 assets, Unity-licensed functionality, and project-authored code/materials/audio; update `Documentation/ASSET_LICENSES.md` for every used asset.
- Community GitHub projects are architecture/UX references only; do not copy their code, scenes, or art.
- `CanvasHud` is display-only; `ObjectiveTracker`, `GuardianBrain`, `GuardianAttackSequence`, and `PlayerLocomotionModel` own decisions.
- Every task follows red → green → full regression → focused commit. Do not claim a behavior from component-presence tests.
- Preserve unrelated dirty work. Stage only the files listed by the active task.

## Verification Environment

Use the isolated Unity harness so the user's open editor does not lock the worktree:

```powershell
$Source = 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild'
$Harness = 'C:\Users\HP\Desktop\3\.codex-unity-test-harness'
$Unity = 'E:\Unity\Editor\Unity.exe'
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
Copy-Item -Path "$Source\Packages\*" -Destination "$Harness\Packages" -Recurse -Force
Copy-Item -Path "$Source\ProjectSettings\*" -Destination "$Harness\ProjectSettings" -Recurse -Force
```

EditMode/PlayMode commands intentionally omit `-quit`; Unity Test Framework exits when the run completes:

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testResults "$Harness\Evidence\TestResults-EditMode.xml" `
  -logFile "$Harness\Logs\release-editmode.log"

& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform PlayMode `
  -testResults "$Harness\Evidence\TestResults-PlayMode.xml" `
  -logFile "$Harness\Logs\release-playmode.log"
```

## File Responsibility Map

- `Assets/Scripts/Systems/PlayerLocomotionModel.cs`: deterministic movement direction, speed, turn, vertical state, and locomotion state.
- `Assets/Scripts/PlayerController.cs`: Unity input adapter and `CharacterController` actuator.
- `Assets/Scripts/CharacterAnimationSet.cs`: resolves named animation roles from embedded CC0 clips.
- `Assets/Scripts/CharacterMotionAnimator.cs`: Unity Playables graph and semantic clip blending.
- `Assets/Scripts/Systems/GuardianBrain.cs`: perception/suspicion/AI state decisions.
- `Assets/Scripts/Systems/GuardianAttackSequence.cs`: deterministic Telegraph/Strike/Recovery timing and single-hit protection.
- `Assets/Scripts/GuardianAI.cs`: NavMesh movement, perception, attack hit query, and public events.
- `Assets/Scripts/PlayerHitResponse.cs`: input lock, hit reaction, screen feedback, and delayed checkpoint reset.
- `Assets/Scripts/GuardianVisionCone.cs`: visible cone mesh/color and obstruction clipping.
- `Assets/Scripts/Systems/ObjectiveModels.cs`: objective stages, data, progression, and save migration rules.
- `Assets/Scripts/ObjectiveDirector.cs`: scene event adapter and world target ownership.
- `Assets/Scripts/Systems/ThreatModel.cs`: pure, deterministic selection of the highest current threat from both guardians.
- `Assets/Scripts/ThreatCoordinator.cs`: scene adapter that subscribes to both guardians and publishes the selected `ThreatSnapshot`.
- `Assets/Scripts/WorldObjectiveMarker.cs`: target pulse, distance, and off-screen direction.
- `Assets/Scripts/CanvasHud.cs`: renders objective, threat, interaction, tutorial, resources, hit, and results.
- `Assets/Scripts/TutorialDirector.cs`: delegates first-minute progression to `ObjectiveDirector`; no duplicate objective state.
- `Assets/Scripts/Systems/SaveData.cs`, `SaveDataMigrator.cs`, and `SaveService.cs`: v4 objective/tutorial persistence and safe v3 migration.
- `Assets/Editor/ProductionSceneGenerator.cs`: binds production models, animation drivers, cones, presenters, director, and HUD.

---

### Task 1: Promote the bundled KayKit characters and validate semantic animation clips

**Files:**
- Copy: `Assets/ThirdParty/KayKitAdventurers/addons/kaykit_character_pack_adventures/Characters/fbx/RogueHooded.fbx` → `Assets/Resources/Characters/Explorer.fbx`
- Copy: `Assets/ThirdParty/KayKitAdventurers/addons/kaykit_character_pack_adventures/Characters/fbx/Knight.fbx` → `Assets/Resources/Characters/Guardian.fbx`
- Copy: `Assets/ThirdParty/KayKitAdventurers/addons/kaykit_character_pack_adventures/Characters/fbx/rogue_texture.png` → `Assets/Resources/Characters/Explorer_Texture.png`
- Copy: `Assets/ThirdParty/KayKitAdventurers/addons/kaykit_character_pack_adventures/Characters/fbx/knight_texture.png` → `Assets/Resources/Characters/Guardian_Texture.png`
- Create: `Assets/Scripts/CharacterAnimationSet.cs`
- Modify: `Assets/Scripts/CharacterAssetCatalog.cs`
- Modify: `Assets/Tests/EditMode/CharacterAssetCatalogTests.cs`
- Create: `Assets/Tests/EditMode/CharacterAnimationSetTests.cs`
- Modify: `Documentation/ASSET_LICENSES.md`

**Interfaces:**
- Produces: `AnimationRole`, `CharacterAnimationSet.Resolve(AnimationRole)`, and resource paths `Characters/Explorer` / `Characters/Guardian`.
- Consumes: embedded FBX clips and KayKit CC0 license already present under `Assets/ThirdParty/KayKitAdventurers/LICENSE.txt`.

- [ ] **Step 1: Write failing resource and semantic-role tests**

```csharp
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class CharacterAnimationSetTests
    {
        [TestCase(CharacterAssetId.Explorer)]
        [TestCase(CharacterAssetId.Guardian)]
        public void ProductionCharacter_ResolvesEveryRequiredRole(CharacterAssetId id)
        {
            AnimationClip[] clips = Resources.LoadAll<AnimationClip>(CharacterAssetCatalog.GetPath(id));
            CharacterAnimationSet set = CharacterAnimationSet.Create(id, clips);
            AnimationRole[] required = id == CharacterAssetId.Explorer
                ? new[] { AnimationRole.Idle, AnimationRole.Walk, AnimationRole.Run, AnimationRole.Sprint,
                    AnimationRole.Crouch, AnimationRole.Jump, AnimationRole.Throw, AnimationRole.Hit }
                : new[] { AnimationRole.Idle, AnimationRole.Walk, AnimationRole.Run,
                    AnimationRole.Attack, AnimationRole.Hit };

            Assert.That(required.All(role => set.Resolve(role) != null), Is.True,
                string.Join(", ", required.Where(role => set.Resolve(role) == null)));
        }
    }
}
```

Update `CharacterAssetCatalogTests.Resources_UseStablePathsForRiggedCharacters` to expect `Characters/Explorer` and `Characters/Guardian`.

- [ ] **Step 2: Run the focused tests and confirm red**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.CharacterAnimationSetTests `
  -testResults "$Harness\Evidence\character-animation-red.xml" `
  -logFile "$Harness\Logs\character-animation-red.log"
```

Expected: compile failure because `CharacterAnimationSet` and `AnimationRole` do not exist, or assertion failure because the promoted resource paths do not exist.

- [ ] **Step 3: Copy the CC0 files and implement the animation resolver**

```powershell
$Pack = "$Source\Assets\ThirdParty\KayKitAdventurers\addons\kaykit_character_pack_adventures\Characters\fbx"
$Characters = "$Source\Assets\Resources\Characters"
Copy-Item -LiteralPath "$Pack\RogueHooded.fbx" -Destination "$Characters\Explorer.fbx" -Force
Copy-Item -LiteralPath "$Pack\Knight.fbx" -Destination "$Characters\Guardian.fbx" -Force
Copy-Item -LiteralPath "$Pack\rogue_texture.png" -Destination "$Characters\Explorer_Texture.png" -Force
Copy-Item -LiteralPath "$Pack\knight_texture.png" -Destination "$Characters\Guardian_Texture.png" -Force
```

Create `CharacterAnimationSet.cs` with the full alias table:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EchoesOfTheRuins
{
    public enum AnimationRole { Idle, Walk, Run, Sprint, Crouch, Jump, Fall, Throw, Hit, Attack }

    public sealed class CharacterAnimationSet
    {
        private static readonly IReadOnlyDictionary<AnimationRole, string[]> Aliases =
            new Dictionary<AnimationRole, string[]>
            {
                [AnimationRole.Idle] = new[] { "Idle_A", "Idle" },
                [AnimationRole.Walk] = new[] { "Walking_A", "Walk" },
                [AnimationRole.Run] = new[] { "Running_A", "Run" },
                [AnimationRole.Sprint] = new[] { "Running_B", "Running_A", "Run" },
                [AnimationRole.Crouch] = new[] { "Walking_C", "Sneaking", "Walking_A" },
                [AnimationRole.Jump] = new[] { "Jump_Full_Short", "Jump_Start", "Jump" },
                [AnimationRole.Fall] = new[] { "Jump_Idle", "Falling", "Jump_Full_Short" },
                [AnimationRole.Throw] = new[] { "Throw", "Interact" },
                [AnimationRole.Hit] = new[] { "Hit_A", "Hit_B", "RecieveHit" },
                [AnimationRole.Attack] = new[] { "1H_Melee_Attack_Chop", "Sword_Attack", "Dagger_Attack" }
            };

        private readonly Dictionary<AnimationRole, AnimationClip> clips;
        private CharacterAnimationSet(Dictionary<AnimationRole, AnimationClip> clips) => this.clips = clips;

        public static CharacterAnimationSet Create(CharacterAssetId _, IEnumerable<AnimationClip> source)
        {
            AnimationClip[] available = source.Where(clip => clip != null && !clip.name.Contains("preview", StringComparison.OrdinalIgnoreCase)).ToArray();
            var resolved = new Dictionary<AnimationRole, AnimationClip>();
            foreach (KeyValuePair<AnimationRole, string[]> pair in Aliases)
            {
                resolved[pair.Key] = pair.Value
                    .Select(alias => available.FirstOrDefault(clip =>
                        string.Equals(clip.name, alias, StringComparison.OrdinalIgnoreCase) ||
                        clip.name.EndsWith("|" + alias, StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault(clip => clip != null);
            }
            return new CharacterAnimationSet(resolved);
        }

        public AnimationClip Resolve(AnimationRole role) => clips.TryGetValue(role, out AnimationClip clip) ? clip : null;
    }
}
```

Update `CharacterAssetCatalog` to return the promoted paths and textures. Record KayKit as CC0 with its local license path and usage.

- [ ] **Step 4: Run tests and inspect actual clip resolution**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.CharacterAnimationSetTests `
  -testResults "$Harness\Evidence\character-animation-green.xml" `
  -logFile "$Harness\Logs\character-animation-green.log"
```

Expected: both parameterized cases pass. If Unity reports the same semantic clip under a prefixed take name, extend only the relevant alias list and rerun; do not restore longest-clip selection.

- [ ] **Step 5: Commit only the asset promotion, catalog, tests, and license record**

```powershell
git add -- Assets/Resources/Characters Assets/Scripts/CharacterAnimationSet.cs Assets/Scripts/CharacterAssetCatalog.cs Assets/Tests/EditMode/CharacterAssetCatalogTests.cs Assets/Tests/EditMode/CharacterAnimationSetTests.cs Documentation/ASSET_LICENSES.md
git commit -m "feat: promote semantic character animation assets"
```

---

### Task 2: Add deterministic camera-relative locomotion and remove side-sliding

**Files:**
- Create: `Assets/Scripts/Systems/PlayerLocomotionModel.cs`
- Create: `Assets/Tests/EditMode/PlayerLocomotionModelTests.cs`
- Modify: `Assets/Scripts/PlayerController.cs`

**Interfaces:**
- Consumes: camera yaw, two-dimensional input, grounded flag, sprint/crouch/jump flags.
- Produces: `LocomotionFrame`, `LocomotionState`, and `PlayerController.LocomotionChanged`.

- [ ] **Step 1: Write failing pure locomotion tests**

```csharp
using NUnit.Framework;
using UnityEngine;

namespace EchoesOfTheRuins.Tests
{
    public sealed class PlayerLocomotionModelTests
    {
        [Test]
        public void CameraYawNinety_ForwardInputMovesWorldRight()
        {
            var model = new PlayerLocomotionModel();
            LocomotionFrame frame = model.Tick(.1f,
                new PlayerInputFrame(Vector2.up, 90f, false, false, false, true));
            Assert.That(frame.WorldDirection.x, Is.EqualTo(1f).Within(.02f));
            Assert.That(Mathf.Abs(frame.WorldDirection.z), Is.LessThan(.02f));
        }

        [Test]
        public void SpeedAcceleratesAndBrakesWithoutInstantChanges()
        {
            var model = new PlayerLocomotionModel(acceleration: 10f, braking: 12f);
            float first = model.Tick(.1f, new PlayerInputFrame(Vector2.up, 0f, false, false, false, true)).Speed;
            float second = model.Tick(.1f, new PlayerInputFrame(Vector2.up, 0f, false, false, false, true)).Speed;
            float braking = model.Tick(.1f, new PlayerInputFrame(Vector2.zero, 0f, false, false, false, true)).Speed;
            Assert.That(first, Is.GreaterThan(0f).And.LessThan(second));
            Assert.That(braking, Is.GreaterThan(0f).And.LessThan(second));
        }

        [Test]
        public void LateralInputFacesMovementDirection()
        {
            var model = new PlayerLocomotionModel(turnDegreesPerSecond: 720f);
            LocomotionFrame frame = model.Tick(.5f,
                new PlayerInputFrame(Vector2.right, 0f, false, false, false, true));
            Assert.That(Mathf.DeltaAngle(frame.FacingYaw, 90f), Is.EqualTo(0f).Within(1f));
        }
    }
}
```

- [ ] **Step 2: Run focused tests and confirm missing types**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.PlayerLocomotionModelTests `
  -testResults "$Harness\Evidence\locomotion-red.xml" `
  -logFile "$Harness\Logs\locomotion-red.log"
```

Expected: compile failure for `PlayerLocomotionModel`, `PlayerInputFrame`, and `LocomotionFrame`.

- [ ] **Step 3: Implement the pure movement model**

```csharp
using UnityEngine;

namespace EchoesOfTheRuins
{
    public enum LocomotionState { Idle, Walk, Run, Sprint, Crouch, Jump, Fall, Throw, Hit, Locked }

    public readonly struct PlayerInputFrame
    {
        public readonly Vector2 Move;
        public readonly float CameraYaw;
        public readonly bool Sprint;
        public readonly bool Crouch;
        public readonly bool JumpPressed;
        public readonly bool Grounded;
        public PlayerInputFrame(Vector2 move, float cameraYaw, bool sprint, bool crouch, bool jumpPressed, bool grounded)
        { Move = Vector2.ClampMagnitude(move, 1f); CameraYaw = cameraYaw; Sprint = sprint; Crouch = crouch; JumpPressed = jumpPressed; Grounded = grounded; }
    }

    public readonly struct LocomotionFrame
    {
        public readonly Vector3 WorldDirection;
        public readonly float Speed;
        public readonly float FacingYaw;
        public readonly LocomotionState State;
        public LocomotionFrame(Vector3 worldDirection, float speed, float facingYaw, LocomotionState state)
        { WorldDirection = worldDirection; Speed = speed; FacingYaw = facingYaw; State = state; }
    }

    public sealed class PlayerLocomotionModel
    {
        private readonly float walkSpeed, runSpeed, sprintSpeed, crouchSpeed, acceleration, braking, turnSpeed;
        private float speed, facingYaw;
        public PlayerLocomotionModel(float walkSpeed = 2f, float runSpeed = 4.5f, float sprintSpeed = 7f,
            float crouchSpeed = 2.2f, float acceleration = 12f, float braking = 16f, float turnDegreesPerSecond = 600f)
        { this.walkSpeed = walkSpeed; this.runSpeed = runSpeed; this.sprintSpeed = sprintSpeed; this.crouchSpeed = crouchSpeed; this.acceleration = acceleration; this.braking = braking; turnSpeed = turnDegreesPerSecond; }

        public LocomotionFrame Tick(float dt, PlayerInputFrame input)
        {
            Quaternion yaw = Quaternion.Euler(0f, input.CameraYaw, 0f);
            Vector3 direction = yaw * new Vector3(input.Move.x, 0f, input.Move.y);
            if (direction.sqrMagnitude > 1f) direction.Normalize();
            float magnitude = input.Move.magnitude;
            float target = magnitude < .05f ? 0f : input.Crouch ? crouchSpeed : input.Sprint ? sprintSpeed :
                magnitude < .55f ? walkSpeed * magnitude / .55f : runSpeed;
            speed = Mathf.MoveTowards(speed, target, (target > speed ? acceleration : braking) * Mathf.Max(0f, dt));
            if (direction.sqrMagnitude > .0025f)
                facingYaw = Mathf.MoveTowardsAngle(facingYaw, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg, turnSpeed * dt);
            LocomotionState state = !input.Grounded ? LocomotionState.Fall : input.JumpPressed ? LocomotionState.Jump :
                input.Crouch && speed > .05f ? LocomotionState.Crouch : speed < .05f ? LocomotionState.Idle :
                input.Sprint ? LocomotionState.Sprint : magnitude < .55f ? LocomotionState.Walk : LocomotionState.Run;
            return new LocomotionFrame(direction.normalized, speed, facingYaw, state);
        }
    }
}
```

- [ ] **Step 4: Replace root-rotation movement in `PlayerController`**

Add fields/events:

```csharp
private PlayerLocomotionModel locomotion;
private bool inputLocked;
public event System.Action<LocomotionFrame> LocomotionChanged;
public void SetInputLocked(bool value) => inputLocked = value;
```

In `Awake`, initialize `locomotion`. In `Update`, rotate the camera pivot from mouse input, read `Vector2 moveInput`, call `Tick`, rotate the player toward `frame.FacingYaw`, and move by `frame.WorldDirection * frame.Speed`. Do not call `transform.Rotate` from mouse input. Preserve vertical velocity, crouch collider, jump, shadow, and echo-stone behavior; ignore gameplay input while locked.

```csharp
float cameraYaw = cameraPivot != null ? cameraPivot.eulerAngles.y : transform.eulerAngles.y;
Vector2 moveInput = inputLocked ? Vector2.zero : new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
bool grounded = controller.isGrounded;
bool jumpPressed = !inputLocked && Input.GetButtonDown("Jump") && grounded;
LocomotionFrame frame = locomotion.Tick(Time.deltaTime,
    new PlayerInputFrame(moveInput, cameraYaw, !inputLocked && Input.GetKey(KeyCode.LeftShift), crouching, jumpPressed, grounded));
transform.rotation = Quaternion.Euler(0f, frame.FacingYaw, 0f);
Vector3 velocity = frame.WorldDirection * frame.Speed;
velocity.y = verticalVelocity;
controller.Move(velocity * Time.deltaTime);
LocomotionChanged?.Invoke(frame);
```

- [ ] **Step 5: Run focused and existing controller-related tests**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.PlayerLocomotionModelTests `
  -testResults "$Harness\Evidence\locomotion-green.xml" `
  -logFile "$Harness\Logs\locomotion-green.log"
```

Expected: all locomotion tests pass with no compile errors.

- [ ] **Step 6: Commit locomotion only**

```powershell
git add -- Assets/Scripts/Systems/PlayerLocomotionModel.cs Assets/Scripts/PlayerController.cs Assets/Tests/EditMode/PlayerLocomotionModelTests.cs
git commit -m "feat: add camera-relative locomotion states"
```

---

### Task 3: Replace longest-clip sampling with semantic Playables animation

**Files:**
- Modify: `Assets/Scripts/CharacterMotionAnimator.cs`
- Modify: `Assets/Tests/EditMode/CharacterMotionAnimatorTests.cs`
- Modify: `Assets/Scripts/PlayerController.cs`
- Modify: `Assets/Scripts/GuardianAI.cs`

**Interfaces:**
- Consumes: `CharacterAnimationSet`, `LocomotionFrame`, and `GuardianState`.
- Produces: `Play(AnimationRole role, float blendSeconds, bool restart)` and `CurrentRole`.

- [ ] **Step 1: Replace the old “has any clip” test with semantic transition tests**

```csharp
[TestCase(CharacterAssetId.Explorer, AnimationRole.Idle)]
[TestCase(CharacterAssetId.Explorer, AnimationRole.Run)]
[TestCase(CharacterAssetId.Explorer, AnimationRole.Sprint)]
[TestCase(CharacterAssetId.Guardian, AnimationRole.Attack)]
public void Configure_ProvidesNamedRole(CharacterAssetId id, AnimationRole role)
{
    GameObject root = new GameObject("Character Root");
    GameObject visual = Object.Instantiate(Resources.Load<GameObject>(CharacterAssetCatalog.GetPath(id)), root.transform);
    CharacterMotionAnimator driver = root.AddComponent<CharacterMotionAnimator>();
    try
    {
        driver.Configure(visual.transform, id);
        Assert.That(driver.HasRole(role), Is.True);
        driver.Play(role, .1f, true);
        Assert.That(driver.CurrentRole, Is.EqualTo(role));
    }
    finally { Object.DestroyImmediate(root); }
}
```

- [ ] **Step 2: Run the focused test and confirm API failure**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.CharacterMotionAnimatorTests `
  -testResults "$Harness\Evidence\motion-semantic-red.xml" `
  -logFile "$Harness\Logs\motion-semantic-red.log"
```

Expected: compile failure because `Configure(Transform, CharacterAssetId)`, `HasRole`, `Play`, and `CurrentRole` are absent.

- [ ] **Step 3: Implement a Playables mixer in `CharacterMotionAnimator`**

Use `PlayableGraph`, `AnimationMixerPlayable`, `AnimationClipPlayable`, and `AnimationPlayableOutput`. `Configure` adds/gets the visual `Animator`, creates one mixer input per resolved clip, starts Idle at weight 1, and sets `animator.applyRootMotion = false`. `Play` records the target role and blend duration. `Update` advances blend elapsed time and sets mixer weights so the previous role decreases from 1→0 while the target increases 0→1. `OnDestroy` destroys the graph.

The public API must be exactly:

```csharp
public AnimationRole CurrentRole { get; private set; } = AnimationRole.Idle;
public void Configure(Transform targetVisual, CharacterAssetId assetId);
public bool HasRole(AnimationRole role);
public void Play(AnimationRole role, float blendSeconds = .12f, bool restart = false);
```

Map player locomotion with a single method:

```csharp
private static AnimationRole ToAnimationRole(LocomotionState state) => state switch
{
    LocomotionState.Walk => AnimationRole.Walk,
    LocomotionState.Run => AnimationRole.Run,
    LocomotionState.Sprint => AnimationRole.Sprint,
    LocomotionState.Crouch => AnimationRole.Crouch,
    LocomotionState.Jump => AnimationRole.Jump,
    LocomotionState.Fall => AnimationRole.Fall,
    LocomotionState.Throw => AnimationRole.Throw,
    LocomotionState.Hit => AnimationRole.Hit,
    _ => AnimationRole.Idle
};
```

Guardian mapping is `Patrol/Investigate/Search → Walk`, `Chase → Run`, attack phases → Attack, capture feedback → Hit/Idle as appropriate.

- [ ] **Step 4: Run semantic animation tests**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.CharacterMotionAnimatorTests `
  -testResults "$Harness\Evidence\motion-semantic-green.xml" `
  -logFile "$Harness\Logs\motion-semantic-green.log"
```

Expected: all player and guardian semantic roles resolve and transition.

- [ ] **Step 5: Commit animation driver integration**

```powershell
git add -- Assets/Scripts/CharacterMotionAnimator.cs Assets/Scripts/PlayerController.cs Assets/Scripts/GuardianAI.cs Assets/Tests/EditMode/CharacterMotionAnimatorTests.cs
git commit -m "feat: blend semantic locomotion animations"
```

---

### Task 4: Add deterministic guardian telegraph, strike, recovery, and single-hit capture

**Files:**
- Create: `Assets/Scripts/Systems/GuardianAttackSequence.cs`
- Create: `Assets/Tests/EditMode/GuardianAttackSequenceTests.cs`
- Modify: `Assets/Scripts/Systems/GuardianBrain.cs`
- Modify: `Assets/Tests/EditMode/GuardianBrainTests.cs`

**Interfaces:**
- Consumes: chase state, attack-range eligibility, target validity, and time step.
- Produces: `GuardianAttackPhase`, `AttackFrame.ShouldQueryHit`, `AttackFrame.SequenceFinished`, and AI states `AttackTelegraph`, `Strike`, `Recovery`, `Capture`.

- [ ] **Step 1: Write failing attack timing tests**

```csharp
using NUnit.Framework;

namespace EchoesOfTheRuins.Tests
{
    public sealed class GuardianAttackSequenceTests
    {
        [Test]
        public void TelegraphCannotHitBeforePointSixSeconds()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();
            AttackFrame frame = attack.Tick(.59f, true);
            Assert.That(frame.Phase, Is.EqualTo(GuardianAttackPhase.Telegraph));
            Assert.That(frame.ShouldQueryHit, Is.False);
        }

        [Test]
        public void StrikeQueriesHitExactlyOnce()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();
            AttackFrame first = attack.Tick(.61f, true);
            AttackFrame second = attack.Tick(.01f, true);
            Assert.That(first.ShouldQueryHit, Is.True);
            Assert.That(second.ShouldQueryHit, Is.False);
        }

        [Test]
        public void InvalidTargetDuringTelegraphProducesMiss()
        {
            var attack = new GuardianAttackSequence(.6f, .15f, .6f);
            attack.Begin();
            AttackFrame frame = attack.Tick(.61f, false);
            Assert.That(frame.ShouldQueryHit, Is.False);
            Assert.That(frame.Phase, Is.EqualTo(GuardianAttackPhase.Recovery));
        }
    }
}
```

Update the old GuardianBrain capture test so attack range first returns `AttackTelegraph`, not `Capture`.

- [ ] **Step 2: Run tests and confirm missing attack sequence/state**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.GuardianAttackSequenceTests `
  -testResults "$Harness\Evidence\guardian-attack-red.xml" `
  -logFile "$Harness\Logs\guardian-attack-red.log"
```

Expected: compile failure for attack types.

- [ ] **Step 3: Implement the pure attack sequence**

```csharp
namespace EchoesOfTheRuins
{
    public enum GuardianAttackPhase { None, Telegraph, Strike, Recovery, HitConfirmed }

    public readonly struct AttackFrame
    {
        public readonly GuardianAttackPhase Phase;
        public readonly bool ShouldQueryHit;
        public readonly bool SequenceFinished;
        public AttackFrame(GuardianAttackPhase phase, bool shouldQueryHit, bool sequenceFinished)
        { Phase = phase; ShouldQueryHit = shouldQueryHit; SequenceFinished = sequenceFinished; }
    }

    public sealed class GuardianAttackSequence
    {
        private readonly float telegraph, strike, recovery;
        private float elapsed;
        private bool active, queried, hit;
        public GuardianAttackSequence(float telegraphSeconds = .6f, float strikeSeconds = .15f, float recoverySeconds = .6f)
        { telegraph = telegraphSeconds; strike = strikeSeconds; recovery = recoverySeconds; }
        public void Begin() { elapsed = 0f; active = true; queried = false; hit = false; }
        public void ConfirmHit() => hit = true;
        public AttackFrame Tick(float dt, bool targetValid)
        {
            if (!active) return new AttackFrame(GuardianAttackPhase.None, false, false);
            elapsed += System.Math.Max(0f, dt);
            if (elapsed < telegraph) return new AttackFrame(GuardianAttackPhase.Telegraph, false, false);
            if (!targetValid) { elapsed = telegraph + strike; return new AttackFrame(GuardianAttackPhase.Recovery, false, false); }
            if (elapsed < telegraph + strike)
            {
                bool query = !queried;
                queried = true;
                return new AttackFrame(hit ? GuardianAttackPhase.HitConfirmed : GuardianAttackPhase.Strike, query, false);
            }
            bool finished = elapsed >= telegraph + strike + recovery;
            if (finished) active = false;
            return new AttackFrame(hit ? GuardianAttackPhase.HitConfirmed : GuardianAttackPhase.Recovery, false, finished);
        }
    }
}
```

Extend `GuardianState` with `AttackTelegraph`, `Strike`, and `Recovery`. Rename the old `GuardianPerception.InCaptureRange` input to `InAttackRange` at its declaration and at every constructor call. `GuardianBrain` may enter `AttackTelegraph` only from Chase when `InAttackRange` is true; it must never produce `Capture` solely from distance. Preserve `CanDetect`, `HasLineOfSight`, `InInvestigateRange`, and the existing suspicion inputs so the transition contract remains explicit rather than inferred from distance.

- [ ] **Step 4: Run attack and brain tests**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.Guardian `
  -testResults "$Harness\Evidence\guardian-attack-green.xml" `
  -logFile "$Harness\Logs\guardian-attack-green.log"
```

Expected: all suspicion and attack timing tests pass.

- [ ] **Step 5: Commit deterministic guardian attack logic**

```powershell
git add -- Assets/Scripts/Systems/GuardianAttackSequence.cs Assets/Scripts/Systems/GuardianBrain.cs Assets/Tests/EditMode/GuardianAttackSequenceTests.cs Assets/Tests/EditMode/GuardianBrainTests.cs
git commit -m "feat: add telegraphed guardian attack state"
```

---

### Task 5: Integrate guardian attack, vision cone, player hit, and checkpoint reset

**Files:**
- Modify: `Assets/Scripts/GuardianAI.cs`
- Create: `Assets/Scripts/GuardianVisionCone.cs`
- Create: `Assets/Scripts/PlayerHitResponse.cs`
- Modify: `Assets/Scripts/GameManager.cs`
- Modify: `Assets/Scripts/AudioDirector.cs`
- Create: `Assets/Tests/EditMode/GuardianVisionConeTests.cs`
- Create: `Assets/Tests/PlayMode/GuardianAttackPlayModeTests.cs`

**Interfaces:**
- Consumes: `GuardianAttackSequence`, `PlayerController.SetInputLocked`, NavMesh, wall mask, player collider.
- Produces: `GuardianAttackChanged(string, GuardianAttackPhase)`, `PlayerStruck(string)`, and delayed `GameManager.ResetPlayerToCheckpoint`.

- [ ] **Step 1: Write failing PlayMode attack behavior**

```csharp
[UnityTest]
public IEnumerator GuardianAttack_TelegraphsBeforePlayerReset()
{
    yield return SceneManager.LoadSceneAsync("ProductionRuins", LoadSceneMode.Single);
    yield return null;
    GuardianAI guardian = Object.FindFirstObjectByType<GuardianAI>();
    PlayerController player = Object.FindFirstObjectByType<PlayerController>();
    Vector3 checkpoint = player.transform.position;
    guardian.DebugBeginAttack(player.transform);
    yield return new WaitForSeconds(.3f);
    Assert.That(Vector3.Distance(player.transform.position, checkpoint), Is.LessThan(.01f));
    Assert.That(guardian.AttackPhase, Is.EqualTo(GuardianAttackPhase.Telegraph));
}
```

Add pure cone tests asserting Patrol/Investigate/Chase map to blue/amber/red and that generated cone vertices stay inside configured angle/range.

- [ ] **Step 2: Run the focused PlayMode test and confirm missing API**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform PlayMode `
  -testFilter EchoesOfTheRuins.Tests.GuardianAttackPlayModeTests `
  -testResults "$Harness\Evidence\guardian-integration-red.xml" `
  -logFile "$Harness\Logs\guardian-integration-red.log"
```

Expected: compile failure for `DebugBeginAttack` and `AttackPhase`.

- [ ] **Step 3: Integrate attack timing and hit query in `GuardianAI`**

Add a `GuardianAttackSequence`, expose `AttackPhase`, stop the NavMeshAgent during Telegraph/Strike/Recovery, rotate toward the player, and query exactly once when `ShouldQueryHit` is true:

```csharp
bool HasClearMeleePath()
{
    Vector3 origin = transform.position + Vector3.up;
    Vector3 target = player.position + Vector3.up;
    Vector3 delta = target - origin;
    if (delta.magnitude > 1.7f) return false;
    return !Physics.Raycast(origin, delta.normalized, delta.magnitude, obstacleMask, QueryTriggerInteraction.Ignore);
}
```

Use `Physics.OverlapSphere` with a player layer or direct distance plus the clear-path query; on hit, call `ConfirmHit`, publish `PlayerStruck`, and let `PlayerHitResponse` control reset timing. Remove the existing same-frame reset block.

- [ ] **Step 4: Implement `PlayerHitResponse` and cone presentation**

`PlayerHitResponse` subscribes to guardian `PlayerStruck`, locks input, tells the player animator to play Hit, activates a full-screen red vignette, waits 1.0 unscaled seconds, calls `ResetPlayerToCheckpoint`, unlocks input, and clears the vignette. A `handlingHit` boolean guarantees one reset per strike.

`GuardianVisionCone` owns a mesh, cone color, and material. Every frame it builds radial segments to `visualRange`, raycasts each segment against the obstacle mask, and shortens that vertex at the first obstruction. `SetState` maps state and suspicion to blue/amber/red without changing AI logic.

- [ ] **Step 5: Run cone, attack, and checkpoint tests**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.GuardianVisionConeTests `
  -testResults "$Harness\Evidence\guardian-cone-green.xml" `
  -logFile "$Harness\Logs\guardian-cone-green.log"
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform PlayMode `
  -testFilter EchoesOfTheRuins.Tests.GuardianAttackPlayModeTests `
  -testResults "$Harness\Evidence\guardian-integration-green.xml" `
  -logFile "$Harness\Logs\guardian-integration-green.log"
```

Expected: telegraph precedes hit/reset, wall obstruction produces a miss, and reset occurs once.

- [ ] **Step 6: Commit guardian integration**

```powershell
git add -- Assets/Scripts/GuardianAI.cs Assets/Scripts/GuardianVisionCone.cs Assets/Scripts/PlayerHitResponse.cs Assets/Scripts/GameManager.cs Assets/Scripts/AudioDirector.cs Assets/Tests/EditMode/GuardianVisionConeTests.cs Assets/Tests/PlayMode/GuardianAttackPlayModeTests.cs
git commit -m "feat: present guardian attacks and player hit response"
```

---

### Task 6: Unify objectives, first-minute progression, world targets, and save v4

**Files:**
- Create: `Assets/Scripts/Systems/ObjectiveModels.cs`
- Create: `Assets/Scripts/ObjectiveDirector.cs`
- Create: `Assets/Scripts/WorldObjectiveMarker.cs`
- Modify: `Assets/Scripts/TutorialDirector.cs`
- Modify: `Assets/Scripts/Systems/SaveData.cs`
- Create: `Assets/Scripts/Systems/SaveDataMigrator.cs`
- Modify: `Assets/Scripts/Systems/SaveService.cs`
- Modify: `Assets/Scripts/GameManager.cs`
- Modify: `Assets/Tests/EditMode/FirstMinuteExperienceTests.cs`
- Modify: `Assets/Tests/EditMode/SaveAndScoreServiceTests.cs`
- Create: `Assets/Tests/EditMode/ObjectiveTrackerTests.cs`

**Interfaces:**
- Produces: `ObjectiveStage`, `ObjectiveData`, `ObjectiveTracker.Advance`, `ObjectiveChanged`.
- Consumes: movement, sprint, observation trigger, crouch-in-shadow, echo use, core count, exit unlock, and save v4.

- [ ] **Step 1: Write failing objective and migration tests**

```csharp
[Test]
public void NewRunProgressesThroughFirstMinuteInOrder()
{
    var tracker = new ObjectiveTracker();
    Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Briefing));
    tracker.Notify(ObjectiveSignal.BriefingFinished);
    Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Move));
    tracker.Notify(ObjectiveSignal.MovedAndSprinted);
    Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.Observe));
    tracker.Notify(ObjectiveSignal.ObservedGuardian);
    tracker.Notify(ObjectiveSignal.CrouchedInShadow);
    tracker.Notify(ObjectiveSignal.UsedEchoStone);
    Assert.That(tracker.Current.Stage, Is.EqualTo(ObjectiveStage.CollectCores));
}

[TestCase(0, "Start Checkpoint", ObjectiveStage.Briefing)]
[TestCase(1, "Courtyard Checkpoint", ObjectiveStage.CollectCores)]
[TestCase(3, "Altar Checkpoint", ObjectiveStage.ReachExit)]
public void V3MigrationDerivesSafeObjective(int cores, string checkpoint, ObjectiveStage expected)
{
    SaveData migrated = SaveDataMigrator.MigrateFromV3(cores, checkpoint, false);
    Assert.That(migrated.Version, Is.EqualTo(4));
    Assert.That(migrated.ObjectiveStage, Is.EqualTo(expected.ToString()));
}
```

- [ ] **Step 2: Run tests and confirm missing objective types/v4 fields**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.ObjectiveTrackerTests `
  -testResults "$Harness\Evidence\objective-red.xml" `
  -logFile "$Harness\Logs\objective-red.log"
```

Expected: compile failure for `ObjectiveTracker`, `ObjectiveStage`, and migration APIs.

- [ ] **Step 3: Implement objective models and v4 migration**

Use fixed stages `Briefing, Move, Observe, Hide, Distract, CollectCores, ReachExit, Complete`. `ObjectiveData` contains stage, title, body, progress current/required, world position, distance visibility, and input lock. `ObjectiveTracker.Notify` accepts only the signal valid for the current stage and publishes one `Changed` event.

Set `SaveData.CurrentVersion = 4` and add `string ObjectiveStage`. Migration rule is exact: start checkpoint + zero cores → Briefing; other zero-to-two cores → CollectCores; three cores → ReachExit; completed → Complete. Preserve settings, best scores, recent runs, checkpoint, cores, and relics.

- [ ] **Step 4: Implement `ObjectiveDirector` and world marker**

`ObjectiveDirector` subscribes to player, guardian observation trigger, core count, echo use, and exit events; it owns the tracker and saves stage changes. During Briefing it locks input, moves a dedicated reveal camera from exit to first core to player over six seconds, then unlocks input and advances.

`WorldObjectiveMarker` receives the current target transform, renders a pulse and distance, clamps off-screen targets to screen edges, and hides within 8 metres. Missing targets disable distance/arrow but keep HUD copy.

Replace `TutorialDirector` progression with a thin adapter to `ObjectiveDirector`; remove its independent `TutorialProgress` ownership while keeping eight-second safe-entry timing and explicit `SAFE ENTRY` event.

- [ ] **Step 5: Run objective, save, and first-minute tests**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests `
  -testResults "$Harness\Evidence\objective-green.xml" `
  -logFile "$Harness\Logs\objective-green.log"
```

Expected: ordered progression and all v3→v4 cases pass; no settings or history data is lost.

- [ ] **Step 6: Commit objective/save integration**

```powershell
git add -- Assets/Scripts/Systems/ObjectiveModels.cs Assets/Scripts/ObjectiveDirector.cs Assets/Scripts/WorldObjectiveMarker.cs Assets/Scripts/TutorialDirector.cs Assets/Scripts/Systems/SaveData.cs Assets/Scripts/Systems/SaveDataMigrator.cs Assets/Scripts/Systems/SaveService.cs Assets/Scripts/GameManager.cs Assets/Tests/EditMode/FirstMinuteExperienceTests.cs Assets/Tests/EditMode/SaveAndScoreServiceTests.cs Assets/Tests/EditMode/ObjectiveTrackerTests.cs
git commit -m "feat: unify objectives and migrate saves to v4"
```

---

### Task 7: Aggregate two-guardian threat and rebuild the HUD hierarchy

**Files:**
- Create: `Assets/Scripts/Systems/ThreatModel.cs`
- Create: `Assets/Scripts/ThreatCoordinator.cs`
- Create: `Assets/Tests/EditMode/ThreatCoordinatorTests.cs`
- Modify: `Assets/Scripts/HudLayoutSpec.cs`
- Modify: `Assets/Scripts/CanvasHud.cs`
- Modify: `Assets/Tests/EditMode/HudLayoutSpecTests.cs`
- Modify: `Assets/Tests/PlayMode/ProductionFlowPlayModeTests.cs`

**Interfaces:**
- Consumes: all guardian alert/attack events, `ObjectiveChanged`, interactor prompt, echo count, safe-entry, hit, victory.
- Produces: `ThreatSnapshot` for the single most dangerous guardian.

- [ ] **Step 1: Write failing threat-selection and layout tests**

```csharp
[Test]
public void HighestSuspicionGuardianWinsAndAttackBreaksTies()
{
    var model = new ThreatModel();
    model.Update("A", GuardianState.Investigate, .7f, GuardianAttackPhase.None, Vector3.left);
    model.Update("B", GuardianState.Chase, .7f, GuardianAttackPhase.Telegraph, Vector3.right);
    Assert.That(model.Current.GuardianId, Is.EqualTo("B"));
    Assert.That(model.Current.AttackPhase, Is.EqualTo(GuardianAttackPhase.Telegraph));
}

[Test]
public void HudUsesFourPersistentInformationZones()
{
    Assert.That(HudLayoutSpec.PersistentZones, Is.EqualTo(4));
    Assert.That(HudLayoutSpec.ObjectivePanelWidth, Is.LessThanOrEqualTo(360));
    Assert.That(HudLayoutSpec.TutorialPanelHeight, Is.LessThanOrEqualTo(56));
}
```

- [ ] **Step 2: Run tests and confirm missing coordinator/new layout constants**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests.ThreatCoordinatorTests `
  -testResults "$Harness\Evidence\hud-red.xml" `
  -logFile "$Harness\Logs\hud-red.log"
```

- [ ] **Step 3: Implement threat aggregation**

`ThreatModel.Update` stores one snapshot per guardian. Sort by attack priority (`HitConfirmed > Strike > Telegraph > Recovery > None`), then AI priority (`Recovery > Strike > AttackTelegraph > Chase > Search > Investigate > Patrol`), then suspicion. `ThreatCoordinator` is a `MonoBehaviour`: it owns one `ThreatModel`, subscribes to every generated guardian, forwards their snapshots, and publishes `Changed` only when the selected guardian or visible values change.

- [ ] **Step 4: Rebuild `CanvasHud` as display-only**

Create exactly four persistent zones:

1. left-top objective line + core progress;
2. top-centre threat bar + state + direction;
3. centre-lower interaction prompt;
4. right-bottom three `Image` pips for echo stones.

Remove the repeated game title and text-character pips. The tutorial is a transient 56-pixel strip and hides after completion. Bind `CanvasHud` to `ObjectiveDirector` and `ThreatCoordinator`, not a single `GuardianAI`. Telegraph flashes the threat bar and edge direction red; safe entry shows `SAFE ENTRY` with a countdown; color states always include text.

- [ ] **Step 5: Add PlayMode assertions for coherent restored state**

Extend `ProductionFlowPlayModeTests` to load a 3/3 save and assert objective text is `REACH THE UNSEALED EXIT` while the tutorial strip is hidden. Load a new-game save and assert Briefing is visible while exit objective is not.

- [ ] **Step 6: Run HUD tests**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests `
  -testResults "$Harness\Evidence\hud-green.xml" `
  -logFile "$Harness\Logs\hud-green.log"
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform PlayMode `
  -testFilter EchoesOfTheRuins.Tests.ProductionFlowPlayModeTests `
  -testResults "$Harness\Evidence\hud-playmode-green.xml" `
  -logFile "$Harness\Logs\hud-playmode-green.log"
```

- [ ] **Step 7: Commit HUD and threat integration**

```powershell
git add -- Assets/Scripts/Systems/ThreatModel.cs Assets/Scripts/ThreatCoordinator.cs Assets/Scripts/HudLayoutSpec.cs Assets/Scripts/CanvasHud.cs Assets/Tests/EditMode/ThreatCoordinatorTests.cs Assets/Tests/EditMode/HudLayoutSpecTests.cs Assets/Tests/PlayMode/ProductionFlowPlayModeTests.cs
git commit -m "feat: present coherent objectives and guardian threat"
```

---

### Task 8: Bind the release scene, visual attack feedback, menu polish, and audio

**Files:**
- Modify: `Assets/Editor/ProductionSceneGenerator.cs`
- Modify: `Assets/Scripts/ProductionSceneLayout.cs`
- Modify: `Assets/Scripts/RuntimeMaterialLibrary.cs`
- Modify: `Assets/Scripts/ProceduralAudioLibrary.cs`
- Modify: `Assets/Scripts/AudioDirector.cs`
- Modify: `Assets/Scripts/MainMenuController.cs`
- Modify: `Assets/Tests/EditMode/StaticProductionSceneTests.cs`
- Modify: `Assets/Tests/EditMode/ProceduralAudioLibraryTests.cs`
- Modify: `Assets/Tests/EditMode/MainMenuCopyTests.cs`

**Interfaces:**
- Consumes: all components from Tasks 1–7.
- Produces: regenerated `MainMenu.unity`, `ProductionRuins.unity`, and complete build-safe references.

- [ ] **Step 1: Add failing static-scene wiring tests**

Assert the generated production scene contains:

```csharp
Assert.That(Object.FindObjectsByType<GuardianVisionCone>(FindObjectsSortMode.None), Has.Length.EqualTo(2));
Assert.That(Object.FindFirstObjectByType<PlayerHitResponse>(), Is.Not.Null);
Assert.That(Object.FindFirstObjectByType<ObjectiveDirector>(), Is.Not.Null);
Assert.That(Object.FindFirstObjectByType<ThreatCoordinator>(), Is.Not.Null);
Assert.That(Object.FindFirstObjectByType<WorldObjectiveMarker>(), Is.Not.Null);
Assert.That(Object.FindObjectsByType<GuardianAI>(FindObjectsSortMode.None).All(g => g.NavigationReady), Is.True);
```

Add audio tests requiring non-empty clips for `AttackTelegraph`, `SwordStrike`, `PlayerHit`, `ObjectivePulse`, and `ExitUnlocked`. Update menu-copy test to reject visible `COURSEWORK BUILD` hero copy.

- [ ] **Step 2: Run scene/audio/menu tests and confirm red**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests `
  -testResults "$Harness\Evidence\scene-release-red.xml" `
  -logFile "$Harness\Logs\scene-release-red.log"
```

- [ ] **Step 3: Bind all release components in the scene generator**

Instantiate Explorer and Guardian resource models, configure semantic animators, add a `GuardianVisionCone` to both guardians, add `PlayerHitResponse` to the player, create one `ObjectiveDirector`, `ThreatCoordinator`, `WorldObjectiveMarker`, and event-driven `CanvasHud`, and assign obstacle/player layers explicitly. Add the six-second reveal-camera path using the exit, first core, and player transforms.

Create an attack weapon glow and a short ground arc from real mesh/material/particle components. Do not use debug cylinders, text glyph icons, or permanent beams. Regenerate the scene and bake NavMesh after all colliders are final.

- [ ] **Step 4: Add distinct procedural feedback audio and menu motion**

Use the existing procedural audio generator to create separate envelopes/frequencies for telegraph, strike, hit, objective pulse, and exit unlock. Route them through `AudioDirector`. Add slow moonlight sweep and a guardian/ruin silhouette to the menu background; retain a small version label but remove the coursework label from primary visual hierarchy.

- [ ] **Step 5: Generate static scenes and rerun focused tests**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness `
  -executeMethod EchoesOfTheRuins.Editor.ProductionSceneGenerator.GenerateFromCommandLine `
  -logFile "$Harness\Logs\generate-release-scenes.log" -quit
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testFilter EchoesOfTheRuins.Tests `
  -testResults "$Harness\Evidence\scene-release-green.xml" `
  -logFile "$Harness\Logs\scene-release-green.log"
```

Expected: static wiring, NavMesh readiness, audio, and menu-copy tests pass.

- [ ] **Step 6: Commit generated scene integration**

```powershell
git add -- Assets/Scenes/MainMenu.unity Assets/Scenes/ProductionRuins.unity Assets/Editor/ProductionSceneGenerator.cs Assets/Scripts/ProductionSceneLayout.cs Assets/Scripts/RuntimeMaterialLibrary.cs Assets/Scripts/ProceduralAudioLibrary.cs Assets/Scripts/AudioDirector.cs Assets/Scripts/MainMenuController.cs Assets/Tests/EditMode/StaticProductionSceneTests.cs Assets/Tests/EditMode/ProceduralAudioLibraryTests.cs Assets/Tests/EditMode/MainMenuCopyTests.cs
git commit -m "feat: assemble release-quality stealth presentation"
```

---

### Task 9: Full regression, Windows release candidate, evidence, and coursework handoff

**Files:**
- Modify: `Assets/Tests/PlayMode/ProductionFlowPlayModeTests.cs`
- Modify: `Assets/Editor/BuildCommands.cs`
- Modify: `PLAYTEST.md`
- Modify: `Documentation/TEST_PLAN.md`
- Modify: `Documentation/REPORT_OUTLINE.md`
- Modify: `README.md`
- Create/update: `Evidence/TestResults-EditMode.xml`
- Create/update: `Evidence/TestResults-PlayMode.xml`
- Create: `Evidence/release-main-menu.png`
- Create: `Evidence/release-first-minute.png`
- Create: `Evidence/release-guardian-telegraph.png`
- Create: `Evidence/release-exit-unlocked.png`
- Create: `Evidence/release-results.png`

**Interfaces:**
- Consumes: completed release scene and all prior tests.
- Produces: verified Windows x64 D3D11 build, real XML results, screenshots, playtest records, and updated coursework documentation.

- [ ] **Step 1: Add final PlayMode behavior assertions**

Add tests that prove:

- new game begins at Briefing with 0/3 cores;
- movement input produces a non-Idle locomotion state and body yaw follows movement;
- both guardians are on NavMesh and cannot detect through a wall;
- echo stone produces Investigate;
- attack telegraph occurs before hit and reset;
- three unique cores unlock the exit and set ReachExit;
- completing the exit shows the result panel and records a run.

- [ ] **Step 2: Run complete EditMode and PlayMode suites**

```powershell
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
Copy-Item -Path "$Source\Packages\*" -Destination "$Harness\Packages" -Recurse -Force
Copy-Item -Path "$Source\ProjectSettings\*" -Destination "$Harness\ProjectSettings" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode `
  -testResults "$Harness\Evidence\TestResults-EditMode.xml" `
  -logFile "$Harness\Logs\release-editmode.log"
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform PlayMode `
  -testResults "$Harness\Evidence\TestResults-PlayMode.xml" `
  -logFile "$Harness\Logs\release-playmode.log"
```

Expected: zero failed, skipped, or inconclusive tests. Copy the two XML files to the worktree `Evidence` folder only after inspecting their `<test-run result="Passed">` attributes.

- [ ] **Step 3: Build the Windows release candidate**

```powershell
& $Unity -batchmode -nographics -projectPath $Harness `
  -executeMethod EchoesOfTheRuins.Editor.BuildCommands.BuildWindowsPlayer `
  -logFile "$Harness\Logs\build-release-candidate.log" -quit
```

Expected: `C:\Users\HP\Desktop\3\.codex-unity-test-harness\Builds\EchoesOfTheRuins.exe` exists; build log contains no compiler, shader, missing scene, NavMesh, or serialization errors. Copy the completed build to the worktree `Builds` only after verification.

- [ ] **Step 4: Run three manual Windows flows and record evidence**

Update `PLAYTEST.md` with date, build hash, tester, result, defects, and evidence path for:

1. New Game → first-minute tutorial → no-alarm completion.
2. Guardian Telegraph → Strike → checkpoint reset → completion.
3. Partial core collection → Quit → Continue → restored objective → completion.

Capture the five named screenshots at 1920×1080. Reject screenshots with debug UI, clipped text, missing cone, wrong objective/tutorial combination, black/pink materials, or visible scene edge.

- [ ] **Step 5: Update README, test plan, report outline, and asset table**

README must state the exact Unity version, controls, core loop, build method, latest verified test counts, save path, license links, and screenshot previews. The report outline must map Prototype A to locomotion/objective/exit and Prototype B to NavMesh/suspicion/echo/attack/checkpoint. Keep the report body within eight pages and put test tables, license details, and commit history in appendices.

- [ ] **Step 6: Run final repository checks**

```powershell
git diff --check
git status --short
git log --oneline -12
Get-ChildItem -Recurse -File Library,Temp,Logs,Builds -ErrorAction SilentlyContinue | Measure-Object
```

Expected: no whitespace errors; generated Library/Temp/Logs/Builds files are not staged; task changes are intentionally scoped.

- [ ] **Step 7: Commit release evidence and documentation**

```powershell
git add -- Assets/Tests/PlayMode/ProductionFlowPlayModeTests.cs Assets/Editor/BuildCommands.cs PLAYTEST.md Documentation/TEST_PLAN.md Documentation/REPORT_OUTLINE.md Documentation/ASSET_LICENSES.md README.md Evidence/TestResults-EditMode.xml Evidence/TestResults-PlayMode.xml Evidence/release-main-menu.png Evidence/release-first-minute.png Evidence/release-guardian-telegraph.png Evidence/release-exit-unlocked.png Evidence/release-results.png
git commit -m "test: verify Windows stealth release candidate"
```

Do not publish or push until the user confirms the GitHub repository target and the verified local branch contains only intended commits.

## Plan Completion Gate

The implementation is complete only when Tasks 1–9 are committed, the full EditMode and PlayMode suites pass from the isolated harness, the Windows build completes, the three manual flows are recorded, and the five release screenshots meet the visual acceptance checks. A compile-success or scene-load-only result is not sufficient.
