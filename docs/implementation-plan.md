# Echoes of the Ruins — Vertical Slice

## Goal

Create a playable Unity 3D stealth-exploration coursework prototype. The player collects three energy cores, avoids a patrol guardian, opens the sealed exit, and escapes.

## Tasks

1. Create the Unity project structure and write failing edit-mode tests for core-count and exit-unlock rules.
2. Implement the tested game-state model plus runtime gameplay components: collection, checkpoint/reset, exit, guardian AI, player and camera control, and status UI.
3. Generate a single playable graybox ruin scene; run Unity tests and validate it opens without compiler errors.

## Constraints

- Unity 6.3 LTS (`6000.3.20f1`), 3D only, no paid/external assets required.
- Exactly three unique cores; exit unlocks only when all three have been collected.
- Guardian patrols waypoints, detects/chases the player, and returns them to the most recent checkpoint on capture.
- Controls: WASD, mouse look, Space jump.
- Runtime UI shows core count, objective, detection feedback, and end-state feedback.
