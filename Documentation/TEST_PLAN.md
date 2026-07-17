# Test evidence: Echoes of the Ruins

| ID | Scenario | Expected result | Evidence |
|---|---|---|---|
| E01 | Collect the same core twice | Counter only changes once | `RuinGameStateTests` |
| E02 | Collect three distinct cores | Exit unlocks at `3/3` | `RuinGameStateTests` |
| E03 | Load invalid JSON | Safe entrance save and defaults returned | `SaveAndScoreServiceTests` |
| E04 | Save checkpoint, core and setting | Values persist after reload | `SaveAndScoreServiceTests` |
| E05 | Quiet, quick run versus noisy run | Quiet score ranks higher | `SaveAndScoreServiceTests` |
| E06 | Hear echo / see player / lose sight / touch player | Investigate, chase, search then patrol, capture | `GuardianBrainTests` |
| P01 | Crouch in a shadow zone | HUD reports SHADOW / CROUCH and guardian detection range is reduced | Record in Play Mode |
| P02 | Throw echo stone (`Q`) | Guardian investigates the noise point | Record in Play Mode |
| P03 | Three full runs | New game, post-capture run, save-and-restart run all finish | Record Windows build |

Latest automated result: `TestResults/StealthIntegration.xml`, 12 passed, 0 failed (2026-07-17).
