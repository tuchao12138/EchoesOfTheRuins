# Echoes of the Ruins — Coursework Report Draft

> Add your own screenshots at each `[FIGURE]` marker. This draft only claims verified automated work; complete the three manual Windows playtests in `PLAYTEST.md` before submission.

## 1. Game overview, audience and distinctive loop

*Echoes of the Ruins* is a third-person stealth-exploration vertical slice, built in Unity 6.3 LTS for Windows x64. A relic explorer enters a moonlit citadel, restores three energy cores while avoiding stone guardians, then escapes through the north gate. It targets players who prefer readable, low-pressure stealth to combat-heavy action.

The distinctive loop is intentionally compact: observe a patrol, crouch in shadow to cross safely, or use a limited echo stone to redirect danger. Progress does not end automatically after the final collectible. The player must follow the north-gate signal and press `E`, making the ending an intentional and understandable final action.

`[FIGURE 1 — Main menu and opening mission statement.]`

## 2. Narrative, visuals, controls and engine rationale

The citadel wakes when the explorer disturbs its stolen cores. Blue-grey moonlit masonry is neutral; cyan identifies objectives, beacons and restoration; amber communicates guardian danger. This colour rule makes the player’s next decision readable at a glance. The tracked Explorer and Guardian assets provide separate, readable silhouettes against the environment.

Unity 6.3 LTS with URP was chosen for its Windows build pipeline, third-person camera support, runtime navigation, animation control and accessible UI workflow. Controls are `WASD` movement, mouse camera, `Shift` sprint, `C` crouch, `Q` echo stone and `E` interaction. Camera-relative movement and idle, walk, run, sprint, crouch, jump, throw and hit animation states replace the earlier flat, sideways movement.

`[FIGURE 2 — Walk, sprint and crouch frames.]`

## 3. Level flow and onboarding

One map is organised into six zones: Safe Entry, Courtyard, Shadow Gallery, Echo Passage, Altar and North Gate. The first minute states the complete rule — collect three cores, reach the north exit, press `E` — while short prompts teach observation, shadow and distraction. This supports a polished vertical slice rather than adding a second incomplete map.

Each core has a different route lesson. Core I asks the player to observe a guardian in the Courtyard; Core II directs them through the Shadow Gallery; Core III uses the Altar landmark and a cyan beacon. Objectives are sequential: `CORE I — COURTYARD`, `CORE II — SHADOW GALLERY`, then `CORE III — ALTAR`, each with a world marker and distance. Core III was moved from an elevated side position to the grounded main route, so it is reachable and visible.

`[FIGURE 3 — Core I objective and distance.]`
`[FIGURE 4 — Core II through shadow cover.]`
`[FIGURE 5 — Grounded Core III with cyan beacon and altar marker.]`

## 4. Prototype A: interaction, progress, exit and persistence

Prototype A combines `PlayerInteractor`, `Collectible`, `GameManager`, `ExitGate`, checkpoints and JSON saving. The interactor scans nearby colliders rather than relying on one missed trigger event. It publishes Available, Holding, Interrupted and Completed states. A core needs a 1.5-second hold; releasing `E`, leaving range or entering Chase interrupts it. Unique core IDs prevent duplicate scoring.

At 3/3, the game switches to `ALL CORES RESTORED — ESCAPE THROUGH THE NORTH GATE`; a cyan gate beacon, marker and distance direct the player north. It does not call the results screen at this point. Only the in-range `ESCAPE NORTH — PRESS E` gate interaction completes the run. Checkpoints provide a forgiving response to capture, while versioned JSON save data preserves progress and falls back safely after damaged data.

`[FIGURE 6 — Third-core milestone and north-gate beacon.]`
`[FIGURE 7 — Gate `PRESS E` prompt.]`
`[FIGURE 8 — Results screen.]`

## 5. Prototype B: character movement, camera and guardian AI

Prototype B combines camera-relative locomotion, visual animation states and a guardian state machine. The Explorer accelerates, brakes and faces its movement direction instead of translating along a fixed plane. Guardians transition through Patrol, Investigate, Search, Chase and Capture. Sight obstruction, player noise, crouching and shadows affect the threat response; echo stones supply a limited tactical alternative to waiting.

Three patrol routes create an observation challenge, a shadow route and a distraction route. Amber threat feedback warns before capture, while checkpoint recovery reduces the cost of learning. Untracked Rogue and Warrior assets are not used or committed.

`[FIGURE 9 — Guardian patrol and chase.]`
`[FIGURE 10 — Echo-stone investigation.]`
`[FIGURE 11 — Capture and checkpoint recovery.]`

## 6. Iteration from observed failures to tested fixes

The project was changed in response to observed failure rather than adding features speculatively. A menu-to-game failure was traced to a scene-bootstrap compilation error, which meant an old executable was being launched. The corrected bootstrap listens for `ProductionRuins`, then safely generates the playable world and runtime navigation after loading; this avoids the earlier corrupted static scene data.

Later testing showed that the third core was easy to miss and that `NEXT CORE` did not explain the route. The final implementation moves it to the altar route, adds a cyan beacon and names each next destination. A new regression test also revealed that briefing objectives could overwrite the visible count as `0 / 0`; the HUD now only changes core progress when an objective contains valid core progress.

## 7. Verification, limitations and manual evidence

Fresh Unity validation on 20 July 2026 recorded **162/162 EditMode** and **15/15 PlayMode** tests passing. PlayMode coverage includes menu input, menu-to-game transition, playable-scene generation, HUD core count, marker movement from every core to the gate, guardian attack/capture, non-automatic three-core state and gate-only completion. Windows x64 building also succeeded.

Automated tests are not a substitute for authentic player evidence. Before submission I will record: (1) a fresh complete run showing all three cores and the gate, (2) capture then checkpoint recovery, and (3) close/reopen/Continue completion. The design target is 10–12 minutes, but the duration is deliberately not reported as achieved until it is timed in a real run.

`[FIGURE 12 — EditMode and PlayMode result summaries.]`
`[FIGURE 13 — Windows build result.]`

## 8. GitHub, licences, reflection and AI disclosure

The project is version-controlled at [tuchao12138/EchoesOfTheRuins](https://github.com/tuchao12138/EchoesOfTheRuins) on `feature/production-rebuild`. Source, tests, scenes, documentation and licence records are committed; `Library`, builds, logs, recordings and unused character files are excluded. This is the required GitHub evidence, not a substitute for the report’s prototype evidence.

Asset sources and licences are recorded in `Documentation/ASSET_LICENSES.md`. I used AI assistance under module guidance for code review, debugging, regression-test design, technical documentation and language polishing. I reviewed and tested the final implementation and retain responsibility for design and submission decisions. The principal limitation is scope: a single refined map was prioritised over combat or a second level so the two assessed prototypes could be tested and evidenced to a higher standard.

`[FIGURE 14 — GitHub commit history and licence record.]`
