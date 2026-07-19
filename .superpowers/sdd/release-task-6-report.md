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

## Repair follow-up

- Repaired `SaveService.Load` so invalid and exception paths return safe defaults, preserve the corrupt file, and keep objective-stage sanitization inside the class. EditMode coverage now exercises missing-save defaults, corrupt data recovery, and a real v3 JSON file migrated through `SaveService.Load` while preserving checkpoint, cores, relics, best score/rank, history, and settings.
- `TutorialDirector` is the clean committed runtime adapter: its existing bootstrap lifecycle creates `ObjectiveDirector` and `WorldObjectiveMarker`, resolves the already-shipped player/core/exit/guardian/camera objects after scene startup, and configures the director. `ObjectiveRuntimeIntegrationTests` asserts this wiring without depending on the unrelated production-scene builder.
- `ObjectiveTracker` now has only the brief's `Changed` event. The regression test asserts the duplicate `ObjectiveChanged` event is absent.
- Safe static validation: `git diff --check` reported no whitespace errors for the scoped Task 6 repair files. This is not a Unity compilation result.

### Unity verification status

The specified batch EditMode command was attempted against `C:\Users\HP\Desktop\3\.codex-unity-test-harness`, but it did not produce the requested XML result or log artifact. No Unity GREEN claim is made from this session. Re-run manually:

```powershell
$Unity = 'E:\Unity\Editor\Unity.exe'
$Harness = 'C:\Users\HP\Desktop\3\.codex-unity-test-harness'
$Source = 'C:\Users\HP\Desktop\3\EchoesOfTheRuins\.worktrees\production-rebuild'
Copy-Item -Path "$Source\Assets\*" -Destination "$Harness\Assets" -Recurse -Force
& $Unity -batchmode -nographics -projectPath $Harness -runTests -testPlatform EditMode -testFilter EchoesOfTheRuins.Tests -testResults "$Harness\Evidence\task6-fix-tests.xml" -logFile "$Harness\Logs\task6-fix-tests.log"
```

## Final corrective follow-up

- Removed the `SaveAndScoreServiceTests` call to `RunHistoryService`; that service is not part of the Task 6 commit, so the test suite now depends only on tracked Task 6 save/score APIs.
- `ObjectiveDirector` now waits for its `Start` lifecycle callback and valid player, core, exit, and guardian targets before creating its tracker or starting the briefing reveal. This makes an existing director safe when Unity runs it before `TutorialDirector.Start` configures the adapter.
- Added an EditMode regression that invokes `ObjectiveDirector.Start` first, verifies no objective flow has begun, then calls the production `TutorialDirector.Configure(player)` overload. It verifies that null optional targets are resolved before the briefing tracker begins.
- Static validation only: `git diff --check` was run for this corrective scope. Unity was not run for this final follow-up at the parent’s direction, so no Unity GREEN claim is made.
