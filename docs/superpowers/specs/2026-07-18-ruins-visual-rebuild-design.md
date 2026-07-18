# Echoes of the Ruins: Visual Rebuild Design

## Goal

Turn the current readable but prototype-like moonlit ruins into a polished, normal-proportion fantasy stealth level suitable for coursework capture. The game remains a single-player third-person exploration and stealth vertical slice.

## Visual Direction

- **Player:** a normal-proportion hooded explorer with a textured cloak, leather equipment and a readable rear silhouette.
- **Guardian:** a heavier armoured or stone-sentinel silhouette, visually distinct from the explorer and tied to the existing patrol/search/chase colours.
- **Architecture:** authored modular ruin segments rather than visible navigation colliders. The visible route uses broken stone walls, archways, door openings, stairs, column bases, rubble and an altar.
- **Lighting:** blue-grey moonlight establishes the scene. Warm braziers indicate safe route landmarks. Cyan is reserved for cores, runes and the unlocked exit.
- **Composition:** the entry presents the player, a safe light source, the first core, a patrol threat and a route through cover in one view.

## Technical Structure

- Retain invisible box colliders for navigation and NavMesh generation.
- Build visible architecture from independently placed modular meshes. A wall facade is composed from authored wall, corner, gate, stair and floor assets, never from a visible collision cube.
- Keep the generated moonlit stone texture as the floor material; use separate dark-stone and mid-stone materials for architectural depth.
- Keep the existing gameplay systems, HUD, collectables, checkpoints, guard AI and local save system unchanged while replacing their visual presentation.

## Asset Rules

- Use only self-made textures or assets with a recorded permissive licence (CC0 or an explicitly compatible free licence).
- Do not use the downloaded KayKit chibi set: its proportions conflict with the selected visual direction.
- Prefer a normal-proportion free humanoid asset. If no compatible direct download can be validated, retain the existing textured rogue until a verified replacement is available rather than introducing an inferior placeholder.
- Record each imported asset, source URL, author, licence and game use in `Documentation/ASSET_LICENSES.md`.

## Acceptance Criteria

1. The entry screenshot contains no visible plain collision wall or white placeholder mesh.
2. The player is clearly a textured, normal-proportion character and is framed from a usable shoulder camera distance.
3. The scene has readable depth: foreground cover, mid-ground core or patrol path, and a background landmark.
4. Gameplay remains playable: player movement, core collection, exit unlocking and guard navigation work with the invisible collision layer.
5. The Console has no missing shader, material, camera or asset-loading error when the scene starts.

## Delivery Sequence

1. Replace visible collision walls with modular facade placement and apply coherent stone materials.
2. Replace or refine player and guardian presentation using a verified normal-proportion asset.
3. Tune entry composition, lighting and HUD scale; capture a new visual checkpoint screenshot.
4. Run gameplay regression checks and record the visual/asset evidence for the coursework report.
