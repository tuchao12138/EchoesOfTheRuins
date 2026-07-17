# Moonlit Citadel visual rebuild

## Goal

Turn the current playable greybox into a readable third-person stealth vertical slice. The visual target is the selected Moonlit Citadel concept: a moonlit stone courtyard with an amber-lit safe path, cyan objectives, and a clearly visible guard threat. The result must look intentional in a Windows build and remain achievable with Unity 6.3, CC0 Kenney models, self-authored materials, and Unity built-in features.

## Scope and constraints

- Keep the existing single-level objective: collect three cores, avoid a guardian, unlock the exit.
- Keep the current player controller, save data, guardian state machine, and scoring model unless a visual integration requires a small adapter.
- Use only self-authored materials/effects, Unity packages, and recorded CC0 assets.
- Do not add combat, online services, a second map, paid assets, or copied game content.
- Do not claim URP migration until a production build is validated in that pipeline. The first pass must remain stable in the current pipeline.

## Experience pillars

1. **Read the route immediately.** The entry arch frames the courtyard, cyan core light marks the next destination, and warm braziers define the safe route.
2. **Read danger immediately.** The guardian silhouette, blue patrol cone, and yellow/red escalation colours are visible before the player is detected.
3. **Read the player immediately.** A small hooded explorer silhouette with teal cloak, shoulder camera, and rim light replaces the bare capsule presentation.
4. **Keep the HUD quiet.** One compact objective block and a small alert indicator support play without covering the scene.

## Scene composition

### Entry tutorial

- Camera begins behind and above the explorer, looking through a stone arch into the courtyard.
- Two amber braziers light the player and first route; cool moon fill prevents black silhouettes.
- The guardian is beyond the safe area and cannot detect the player for the first eight seconds.
- A single prompt explains the objective, then movement, crouch, and echo stone controls one at a time.

### Courtyard

- Central cyan core altar provides the primary focal point.
- Four stone pillars and low rubble create readable, waist-high cover silhouettes.
- A visible blue guard cone crosses a route that can be bypassed behind a pillar.
- Moonlight is cool and directional; braziers provide amber pools; cyan is reserved for interactables and exit unlocks.

### Side chamber and altar

- Side chamber uses lower blue ambience, a cyan plinth, and deep-but-readable shadow cover.
- Altar chamber is warmer, with raised stairs, rune details, and a final core.
- Exit uses a tall sealed gate and cyan beacon rather than a plain solid cube.

## Art system

| Element | Implementation |
| --- | --- |
| Architecture | Selected Kenney Castle Kit CC0 walls, towers, pillars, stairs, gate and rubble, arranged over retained collision greybox. |
| Stone | Three authored material variants: cool stone, dark occlusion stone, and rune/emissive accent. |
| Lighting | Directional moon, low ambient tri-light, local amber braziers, cyan objective lights, restrained fog. |
| Explorer | Low-poly hooded silhouette built from a simple readable body/cloth profile until a compatible free animated model is imported; no naked capsule visible. |
| Guardian | Warm gold/bronze material, mounted visible spot cone; colour is driven by its Patrol, Investigate/Search and Chase state. |
| VFX | Core float/rotation, cyan light shaft, braziers, subtle particles; no unverified shader graph dependencies. |

## UI and build reliability

- Remove the faulty runtime UI material path responsible for magenta panels.
- Use an explicitly referenced Canvas UI material and packaged font/text rendering path; build validation must inspect the actual launched window, not only the process log.
- HUD: upper-left `CORE 0/3` plus current objective; a compact top-center detection indicator; lower prompts only while teaching or near an interaction.
- A build is rejected if any magenta material, black-only view, missing text, missing core/guardian visual, or shader exception is observed.

## Verification

1. EditMode: retain core, save, score, guardian and resource-catalog tests.
2. PlayMode/manual: capture six screenshots: entry, core altar, cover route, guard patrol cone, side-chamber shadow, unlocked exit.
3. Windows: launch the executable, confirm no pink UI, camera sees explorer/route/guardian, and Player.log has no shader/material/resource errors.
4. Coursework: add the selected asset source and screenshots to the asset and testing evidence.

## Completion criteria

- A new player can identify the character, first core, guard threat and likely route within five seconds.
- The core loop completes from a fresh build with no magenta UI or debug overlay.
- Screenshots demonstrate the same visual language as the selected concept: moonlit blue-grey stone, amber route lighting, cyan objectives, and gold guardian threat.
