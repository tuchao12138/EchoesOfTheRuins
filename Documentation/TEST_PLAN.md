# Test evidence: Echoes of the Ruins

| ID | Scenario | Expected result | Evidence |
|---|---|---|---|
| E01 | Collect the same core twice | Counter only changes once | `RuinGameStateTests` |
| E02 | Collect three distinct cores | Exit unlocks at `3/3` | `RuinGameStateTests` |
| E03 | Load invalid JSON | Safe entrance save and defaults returned | `SaveAndScoreServiceTests` |
| E04 | Save checkpoint, core and setting | Values persist after reload | `SaveAndScoreServiceTests` |
| E05 | Quiet, quick run versus noisy run | Quiet score ranks higher | `SaveAndScoreServiceTests` |
| E06 | Hear echo / see player / lose sight / touch player | Investigate, chase, search then patrol, capture | `GuardianBrainTests` |
| E07 | Apply production project setup | URP is assigned and Windows uses D3D11 only | `ProductionProjectSetupTests` |
| E08 | Generate citadel skyline and courtyard dressing | Map boundary is hidden; decorative colliders remain disabled | `CitadelBackdropBuilderTests`, `CourtyardSetDressingBuilderTests` |
| A01 | Load `MainMenu` | Active camera, Canvas and EventSystem exist | `ProductionFlowPlayModeTests` |
| A02 | Pointer-click `CONTROLS` | Controls modal opens | `ProductionFlowPlayModeTests` |
| A03 | Load `ProductionRuins` | Player, camera, 3 guardians, 3 cores, HUD and exit exist | `ProductionFlowPlayModeTests` |
| A04 | Approach a core without a trigger callback | Range scan still discovers the nearest interactable | `PlayerInteractorTests` |
| A05 | Tap E on a core | Core does not complete; 1.5 second hold is required | `PlayerInteractorTests` |
| A06 | Load saved production scene | No missing scripts; persistent baked NavMesh exists | `StaticProductionSceneTests` |
| A07 | Configure explorer animation | Valid Avatar drives semantic Idle/Walk/Run/Sprint/Crouch actions | `CharacterMotionAnimatorTests` |
| P01 | Crouch in a shadow zone | HUD reports SHADOW / CROUCH and guardian detection range is reduced | Record in Play Mode |
| P02 | Throw echo stone (`Q`) | Guardian investigates the noise point | Record in Play Mode |
| P03 | Three full runs | New game, post-capture run, save-and-restart run all finish | Record Windows build |

Latest automated results (2026-07-20):

- `Evidence/TestResults-EditMode.xml`: **156 passed, 0 failed**.
- `Evidence/TestResults-PlayMode.xml`: **14 passed, 0 failed**.
- `Evidence/final-windows-build.log`: Windows 64-bit build completed successfully.

The three Windows full-run scenarios remain manual acceptance tasks and must not be described as complete until their screenshots/video and result screens have been recorded.
