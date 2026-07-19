# Echoes of the Ruins — coursework report draft

## 1. Game overview and selling point

*Echoes of the Ruins* is a third-person stealth-exploration game for casual players. The player enters a moonlit citadel, attunes three energy cores and escapes through the north gate while avoiding three stone guardians. The central selling point is a readable stealth loop built from three verbs: observe patrols, hide in shadow and throw limited echo stones to redirect danger. The submission is deliberately scoped as one polished 8–10 minute vertical slice rather than several unfinished maps.

## 2. Story, audience and player character

The ruins are waking after centuries of silence. A hooded relic hunter must restore the stolen cores before the guardian network seals the citadel. The normal-proportion explorer silhouette, cyan amulet and shoulder camera make the player readable against blue-grey stone. Casual players receive staged teaching, forgiving suspicion buildup and checkpoint recovery; optional relics and score ranks give experienced players reasons to replay without adding combat.

## 3. Visual and audio direction

The art direction uses cold moonlight for architecture, cyan for objectives and warm amber for fire and guardian danger. URP supplies D3D11 rendering, restrained bloom, colour adjustment, fog and high-quality antialiasing. Modular CC0 ruins, masonry meshes, broken arches, distant silhouettes, braziers and layered stone textures replace the prototype cubes. Film grain, motion blur and temporal antialiasing are disabled to prevent moving-floor smear. Audio communicates wind, footsteps, core progress, investigation, attack, capture and exit unlock; warnings never rely on colour alone.

## 4. Level, controls and first-minute experience

The route is approximately 129 metres across six zones: Safe Entry, Courtyard, Shadow Gallery, Echo Passage, Altar and North Gate. `WASD` moves, mouse controls the camera, `Space` jumps, `Shift` sprints, `C` crouches, `Q` throws an echo stone and `E` interacts. Each core requires a 1.5-second hold that resets if the player releases the key, leaves range or enters Chase. The HUD first states the complete rule—three cores, north gate, press E—then shows one current instruction, target direction and distance. The final core changes the run to Escape rather than ending automatically.

## 5. Working prototype A: interaction, progress and persistence

Prototype A combines `PlayerInteractor`, `Collectible`, `GameManager`, `ExitGate`, checkpoints and JSON saving. The interactor actively scans nearby colliders so interaction does not depend on a missed trigger callback. `InteractionViewData` publishes Available, Holding, Interrupted and Completed states to the HUD. Unique core IDs prevent duplicate scoring. At 3/3 the exit changes visual state and the objective points north; only a separate E press at the gate calls `CompleteEscape()`. Save version 3 retains checkpoint, core/relic progress, settings, run metrics and recent scores, with safe fallback for damaged JSON.

## 6. Working prototype B: character animation and stealth AI

Prototype B combines a camera-relative locomotion model, semantic animation roles, baked NavMesh navigation and the guardian state machine. The player accelerates, brakes and rotates toward movement instead of sliding sideways. The PlayableGraph maps Idle, Walk, Run, Sprint, Crouch, Jump, Throw and Hit to the imported model; the FBX Avatar is explicitly bound to the skeleton Animator. Guardians use Patrol, Investigate, Search, Chase and Capture, with line-of-sight obstruction, suspicion, noise, shadow and crouch modifiers. Three distinct patrol routes create observation, shadow and distraction challenges.

## 7. Testing and iteration

The final automated run passed 156 EditMode and 14 PlayMode tests. Tests cover interaction timing and interruption, scene integrity, Avatar binding, baked navigation data, menu input, guardian attack/capture and the complete three-core–exit–results flow. The Windows x64 build completed with no missing-script, invalid-NavMesh, shader, C# or camera errors. Earlier videos revealed unreadable darkness, static character sliding, unclear E input and an invisible exit; these findings directly produced the clarity settings, Avatar fix, hold UI and north-gate objective. Three recorded Windows playthroughs remain a manual submission check and are not falsely reported as complete.

## 8. GitHub, assets and reflection

Git records the project in small functional milestones covering prototype systems, release bindings, objectives, guardian feedback, static scene generation, interaction, animation and final verification. GitHub is used as the required code repository; generated `Library`, `Temp`, `Logs`, `Builds` and recordings are excluded. Only used lightweight CC0 assets, source code, settings, licence records and evidence are committed. External GitHub stealth projects informed modular state and feedback patterns only; no code, art or level content was copied. The main limitation is production breadth: one refined map was prioritised over combat, networking or a second level because those features would weaken reliability and coursework evidence.

## Appendix checklist

- Greybox and final entrance comparison.
- Courtyard core, shadow gallery, echo-stone investigation, altar core, north-gate unlock and result screen.
- `TestResults-EditMode.xml` and `TestResults-PlayMode.xml`.
- Three signed manual playthrough rows from `PLAYTEST.md`.
- `ASSET_LICENSES.md` and Git commit history.
