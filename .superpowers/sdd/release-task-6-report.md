# Task 6 objective/save report

## Scope implemented

- Added the single authoritative `ObjectiveTracker`, its stage/signal/data model, stage-change events, and ordered first-minute contract tests.
- Added `ObjectiveDirector` route reveal/input lock, player/core/exit/victory hooks, saved objective stages, and `WorldObjectiveMarker` target/distance behavior.
- Converted save schema to v4 and added a lossless v2/v3-to-v4 migration path. It preserves existing save fields and derives the conservative objective stage from cores/checkpoint/completion.
- Converted `TutorialDirector` to a safe-entry-only adapter; it no longer owns `TutorialProgress` state and emits explicit `SAFE ENTRY` logging while retaining eight seconds of detection immunity.

## TDD and verification evidence

- RED tests were added before objective/migration implementation in `Assets/Tests/EditMode/ObjectiveTrackerTests.cs`; they cover ordered signals, invalid signals/no event, and the specified v3 migration cases.
- Static validation: `git diff --check` for all Task 6 files completed with no whitespace errors.
- Unity was deliberately not invoked in this session. The supplied executable/harness are external to the permitted worktree flow and the parent explicitly directed no workaround launch.

## Required manual Unity verification

```powershell
$Unity = 'E:\Unity\Editor\Unity.exe'
$Harness = 'C:\Users\HP\Desktop\3\.codex-unity-test-harness'
$Source = 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild'
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode -testFilter EchoesOfTheRuins.Tests -testResults "$Harness\Evidence\objective-green.xml" -logFile "$Harness\Logs\objective-green.log"
```

Expected evidence: ordered first-minute progression and all v3/v4 migration cases pass without settings or run-history loss.
