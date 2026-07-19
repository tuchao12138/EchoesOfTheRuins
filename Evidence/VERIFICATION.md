# Final automated verification

Verified on 20 July 2026 with Unity 6.3 LTS (`6000.3.20f1`).

- EditMode: **156 passed, 0 failed** (`TestResults-EditMode.xml`).
- PlayMode: **14 passed, 0 failed** (`TestResults-PlayMode.xml`).
- Windows 64-bit build: **Succeeded** at `Builds/EchoesOfTheRuins.exe` in the local test workspace.
- Build-log audit: **0** missing-script messages, **0** invalid-NavMesh messages, **0** shader errors, **0** C# compiler errors and **0** no-camera errors.

The automated suite covers unique core counting, 1.5-second hold interaction, interaction range fallback, scene script integrity, persistent NavMesh data, animation Avatar binding, guardian attack/capture, menu pointer input, three-core escape gating, objective/HUD transition and score completion.

Manual acceptance still required before submission:

1. Record one no-alert Windows run.
2. Record one captured-then-completed Windows run.
3. Close and reopen the game, continue from save and record completion.

These manual runs are intentionally not marked as passed until their recordings and result screens exist.
