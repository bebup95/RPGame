# RPGame project state

Last updated: 2026-09-17 (Asia/Ho_Chi_Minh)

This is the durable project handoff. It records verified facts, decisions, completed work, and remaining work. It is not a substitute for inspection: every task must validate relevant entries against current source and Unity MCP.

## Current status

RPGame is a small single-scene 2D action game. The player moves and jumps in a fixed side-view arena, protects a stationary character, and defeats enemies that spawn from both sides with an accelerating cadence. The run shows elapsed time and kill count and reaches Game Over when the player or protected character dies.

The core loop exists and the project opens in Unity 6. Milestone 0 was frozen and smoke-tested on 2026-09-17. All runtime branches passed; sustained physical keyboard movement/jump remains a manual feel check because desktop automation cannot hold those inputs reliably. The two baseline design decisions are now fixed: Game Over pauses completely and enemy spawn cadence is capped at 0.7 seconds. Milestone 1 now has compiled state-machine primitives plus Player Idle, Move, Jump, and Fall states; Attack intentionally remains on the verified legacy path until a focused Game-view locomotion feel check is confirmed. This project is still in development: automated tests are absent and a standalone build verification has not yet been recorded.

## Verified environment

- Repository: `/Users/lenhat/Developer/RPGame-main`
- Git: Milestone 1 work started from merged `origin/main` commit `4a88cd2` on branch `codex/milestone-1-fsm-foundation`.
- Unity: `6000.0.72f1` (`b731fd3ae857`).
- Entry scene: `Assets/Scenes/SampleScene.unity`; it is the only enabled build scene.
- Packages include URP `17.0.4`, Input System `1.19.0`, uGUI `2.0.0`, Test Framework `1.6.0`, and Unity MCP `v10.0.0` from CoplayDev.
- Codex Unity MCP endpoint: `http://127.0.0.1:8080/mcp`.
- Unity MCP is connected and responding through HTTP. On 2026-09-17 it reported one active instance, `RPGame-main@5489ec430c09e367`, with the correct project root and Unity version. Five consecutive read cycles completed successfully without stale state or routing failures (about 1.5–1.8 seconds per cycle).
- Authored asset inventory at this audit: 14 C# runtime scripts, 1 gameplay prefab, 3 Animator Controllers, 3 materials, and 2 scenes (one gameplay scene plus the URP scene template).
- No authored EditMode or PlayMode test files were found.

## Current gameplay and controls

- Horizontal movement: legacy `Horizontal` axis (`A`/`D` or arrow keys).
- Jump: Space while grounded and movement is enabled.
- Player attack: left mouse button; the base `Entity` sets Animator trigger `attack`.
- Enemy attack: an overlap check sets Animator trigger `Attack` while a target is detected.
- Attacks apply damage on an Animation Event by calling `Entity.DamageTargets()`.
- Enemy deaths increment the UI kill counter.
- Elapsed time uses `Time.timeSinceLevelLoad`, so it resets when the scene reloads.
- Game Over activates a UI panel and sets `Time.timeScale` to `0`; Restart reloads the active scene and `UI.Awake()` restores time scale to `1`.
- Enemy spawn cooldown starts at 3 seconds and decreases by `0.05` per spawn. Both the code default and `SampleScene` serialize the agreed `0.7`-second cap.

## Architecture and serialization contracts

- `Entity.cs`: common health, component caching, movement hooks, grounding, facing, animation parameters, overlap-circle damage, hit material feedback, and death behavior.
- `Player.cs`: legacy input, horizontal movement, jump, attack request, and player-death Game Over.
- `Enemy.cs`: forward movement, target detection/attack request, and kill-count update.
- `ObjectToProtect.cs`: faces the player and triggers Game Over on death.
- `Enemy_Respawner.cs`: validates its prefab/spawn points, spawns randomly, flips enemies spawned to the player's right, and accelerates spawn cadence.
- `Entity_AnimationEvents.cs`: forwards attack clip events to movement locking and `DamageTargets()`.
- `UI.cs`: scene singleton, timer, kill count, Game Over UI, and restart.
- `StateMachine/EntityState.cs`: abstract plain-C# state contract with `Enter`, `Update`, and `Exit`, bound to an Entity and its state machine.
- `StateMachine/EntityStateMachine.cs`: initializes one starting state, updates the active state, and performs guarded Exit/Enter transitions.
- `StateMachine/PlayerState.cs`: typed base for Player states.
- `StateMachine/PlayerIdleState.cs` and `PlayerMoveState.cs`: own zero/input-driven horizontal velocity and deterministic Idle/Move transitions while preserving movement locks.
- `StateMachine/PlayerJumpState.cs` and `PlayerFallState.cs`: own jump impulse, airborne horizontal control, apex transition, and landing selection back to Idle/Move.

Known serialization-sensitive contracts:

- Player Animator parameters: `Xvelocity`, `Yvelocity`, `isGrounded`, trigger `attack`.
- Enemy Animator parameter: `Xvelocity`, trigger `Attack`.
- Animation Event method names: `DisableMovementAndJump`, `DamageTargets`, and `EnableMovementAndJump`.
- Layers: `Ground` 6, `Enemy` 7, `Player` 8, `Level_Limits` 9.
- The player and protected character are expected on the player/target layer; enemies are expected on the enemy layer.
- Scene/prefab Inspector references and exact serialized values must be verified via Unity MCP before relying on them.

Deep Unity MCP audit completed on 2026-09-17:

- Player has Rigidbody2D, CapsuleCollider2D, child Animator/SpriteRenderer, `Entity_AnimationEvents`, damage material, AttackPoint, Enemy target mask (128/layer 7), and Ground mask (64/layer 6).
- Enemy prefab is on layer 7 and has Rigidbody2D, CapsuleCollider2D, child Animator/SpriteRenderer, `Entity_AnimationEvents`, AttackPoint, Player target mask (256/layer 8), and Ground mask (64/layer 6).
- ObjectToProtect has the required Rigidbody2D, CapsuleCollider2D, child Animator/SpriteRenderer, and damage material. Its attack reference and masks are intentionally empty because it never attacks or performs the base ground check.
- Enemy_Respawner references `Enemy.prefab` and both spawn transforms. Effective serialized settings are `3.0`, `0.05`, and `0.7` seconds for initial cooldown, decrease rate, and cap.
- Player and Enemy Animator parameter names and casing match code. Both attack clips contain `DisableMovementAndJump`, `DamageTargets`, and `EnableMovementAndJump` events, and all receiver methods exist.
- UI references resolve to `GameOver_UI`, `Timer_Value`, and `KillCount_Value`. `TryAgain_Button` is interactable and has one RuntimeOnly persistent call to `Canvas.UI.RestartLevel`.
- EventSystem and InputSystemUIInputModule are enabled; the module uses `DefaultInputActions`.

## Milestone 0 smoke test — 2026-09-17

Test environment: Unity `6000.0.72f1`, `SampleScene`, Play Mode through Unity MCP. Runtime-only setup was used where necessary to isolate branches; no scene, prefab, animation, material, script, or Inspector value was saved.

| Check | Result | Evidence |
| --- | --- | --- |
| Scene startup and HUD | Pass | Play Mode started with `timeScale=1`, Player/ObjectToProtect/respawner present, Game Over hidden, timer advancing, and kill count `0`. |
| Enemy spawning from both sides | Pass | Natural spawning produced five live enemies around player X `4.00`: three to the left and two to the right. |
| Horizontal movement and jump mechanics | Partial | Runtime invocation of the existing handlers produced X velocities `+8.00` and `-8.00`, and grounded jump velocity `12.00`. Short synthetic key presses through desktop automation did not produce observable displacement, so holding `A`/`D` and pressing Space still needs one manual Game-view confirmation. |
| Player mouse attack, Animation Event damage, enemy death, kill count | Pass | With three enemies isolated at the Player AttackPoint, an actual Game-view left click played the attack path and changed kill count from `0` to `3`, demonstrating the clip event reached `DamageTargets()` and enemy death reached `UI.AddKillCount()`. |
| Player-death Game Over | Pass | Player health reached `0`; its collider disabled, Game Over appeared, and the 2026-09-17 decision retest confirmed `timeScale=0`. |
| Protected-character-death Game Over | Pass | Ten runtime damage calls reduced ObjectToProtect health from `10` to `0`; its collider disabled, Game Over appeared, and the decision retest confirmed `timeScale=0`. |
| Restart button and scene reset | Pass | Clicking the visible Restart button after focusing Game view reloaded the scene. A second restart check restored `timeScale=1`, Player/ObjectToProtect, hidden Game Over, kill count `0`, and timer near `0.07s`. |
| Console and Editor cleanup | Pass | Console contained zero errors and zero warnings. Play Mode exited successfully; Editor returned idle with no compile/import work pending. |

Overall Milestone 0 result: **pass with one manual feel check outstanding**. The complete gameplay branch structure is operational. Runtime handler checks produced movement velocities `+8/-8` and jump velocity `12`; sustained physical keyboard input in the focused Game view remains a manual confirmation because desktop automation cannot hold keys reliably. This is a test-automation limitation, not a confirmed gameplay defect.

### Baseline controls and acceptance checklist

- Move: `A`/`D` or Left/Right Arrow.
- Jump: Space while grounded.
- Attack: left mouse button.
- Restart after Game Over: Restart button.
- Acceptance baseline: scene starts with timer and zero kills; player can move, jump and attack; one valid attack produces one damage/death path; enemies spawn from both sides; enemy deaths increment kills; Player or ObjectToProtect death pauses the game; Restart restores a clean run; Console remains free of gameplay errors and warnings.

## Decisions and fixed constraints

- Keep the current pixel-art visual direction unless the user explicitly requests a change.
- Keep the current legacy-input gameplay path; the installed new Input System is not authorization to migrate controls.
- Keep animation-driven hit timing. Do not replace Animation Events or rename receiver methods as incidental cleanup.
- Game Over must pause simulation completely with `Time.timeScale = 0`; Restart restores `Time.timeScale = 1`.
- Enemy spawn cooldown must not fall below `0.7` seconds. Keep code defaults, comments, and Inspector values aligned to this decision.
- Use Unity Editor through Unity MCP for scene, prefab, animation, material, and Inspector work; do not hand-edit their YAML when Editor operations are available.
- Do not delete assets, install dependencies, or make architectural migrations without explicit approval.
- Current code and live Unity state outrank this document when evidence conflicts; update this file immediately after resolving a conflict.

## Completed work

- Core player movement, jump, attack, health, damage feedback, and death flow implemented.
- Enemy movement, target detection, attack, death, and kill counting implemented.
- Protected-character death path and player-following facing implemented.
- Enemy spawning and increasing spawn frequency implemented with validation guards.
- Timer, kill count, Game Over UI, and scene restart implemented.
- Recent null-safety/singleton fixes are present in source: protected character stops tracking a destroyed player, UI handles duplicates and clears its static instance, respawner validates core references, and damage feedback stores its coroutine handle.
- Unity MCP package pinned to `v10.0.0`, local endpoint configured in Codex, and connection previously validated.
- 2026-09-17: stale repository instructions replaced, durable project state created, and Codex Memories enabled globally.
- Milestone 0 baseline checkpoint created on branch `codex/milestone-0-baseline` at commit `176fafe` before any FSM migration.
- Milestone 1 steps 1 and 3a/3b completed on branch `codex/milestone-1-fsm-foundation`: added compiled FSM primitives and migrated Player Idle/Move/Jump/Fall while leaving Attack on its prior path.

## Open questions and known risks

- Enemy code calls `SetTrigger("Attack")` every frame while a target remains detected. Verify actual Animator behavior and decide whether an explicit cooldown/state gate is needed.
- `Entity.Awake()` assumes Rigidbody2D, Collider2D, child Animator, and child SpriteRenderer exist. These required components were verified for all current Entity types; future prefabs still need the same validation.
- No automated tests currently protect combat, spawning, UI, or restart behavior.
- A current standalone player build result has not been recorded.

## Backlog

Priority order is provisional and must be confirmed with the user before gameplay changes:

The full proposed completion sequence, effort estimates and milestone exit criteria are maintained in `ROADMAP.md`. The current recommended target is a combat vertical slice first, then an RPG vertical slice, then release hardening.

1. Manually confirm sustained `A`/`D` movement and Space jump in a focused Game view, closing the only partial item from the 2026-09-17 Milestone 0 smoke test.
2. Manually verify sustained movement, jump arc, air control, landing, and edge-fall behavior in the focused Game view; only then migrate Player Attack into its own state.
3. Decide and implement enemy attack cadence/state behavior if repeated trigger requests cause incorrect combat.
4. Add focused EditMode/PlayMode tests for logic that can be tested reliably.
5. Produce and smoke-test a standalone build.
6. Only after gameplay requirements are agreed: add or refine assets, animations, materials, VFX, audio, levels, UI polish, and balancing while preserving the established art direction.

## Change log

- 2026-09-17 — Created this state file from current code, project settings, package manifest, Git metadata, and prior successful Unity MCP setup. Replaced stale claims in `AGENTS.md`. No gameplay scripts, scenes, prefabs, animations, materials, or Inspector values were changed.
- 2026-09-17 — Rechecked Unity MCP: one correct Unity instance connected; Editor ready; `SampleScene` loaded and clean; hierarchy/layers readable; Console contained no errors or warnings; five consecutive read cycles succeeded. No Unity state was modified.
- 2026-09-17 — Completed read-only deep audit of Entity Inspector contracts, Enemy prefab, Animator Controllers, Animation Events, UI references, Restart button binding, and EventSystem. All required references and event receivers are present. Recorded the effective `0.7`-second scene spawn cap. Unity MCP's generic Animator serializer emitted transient IK/playback inspection errors; those audit-generated logs were cleared and the Console was clean afterward. No project asset was modified.
- 2026-09-17 — Added `ROADMAP.md`, a proposed system-by-system completion plan grounded in the verified prototype and the public AlexDev Unity 6 RPG curriculum. No gameplay implementation was authorized or changed.
- 2026-09-17 — Ran and documented the Milestone 0 Play Mode smoke test through Unity MCP. Startup/HUD, bilateral spawning, mouse attack plus Animation Event damage, enemy death and kill counting, both Game Over branches, Restart, Console cleanliness, and Editor cleanup passed. Movement/jump handlers produced the configured velocities, but sustained physical keyboard input remains a manual confirmation. No persistent Unity asset or gameplay file was changed by the test.
- 2026-09-17 — Finalized Milestone 0 decisions: Game Over now pauses at `Time.timeScale=0`; the spawn cooldown default/comment and the already-serialized Inspector value are aligned at `0.7` seconds. Retest confirmed startup/HUD, handler velocities, 6/6 bilateral runtime spawns, combat damage and kill count, both Game Over branches, Restart at zero time scale, and a clean Console. No scene/prefab/animation/material asset required modification.
- 2026-09-17 — Created the pre-FSM Git checkpoint `176fafe` on branch `codex/milestone-0-baseline` (`Checkpoint Milestone 0 baseline`).
- 2026-09-17 — Started Milestone 1 on branch `codex/milestone-1-fsm-foundation`. Added abstract `EntityState` and sealed `EntityStateMachine` primitives with null guards, one-time initialization, active-state update, and guarded transitions. Unity compiled both into `Assembly-CSharp`; API reflection and validation reported the expected members with zero diagnostics. A Play Mode startup regression found all core scene objects active at `timeScale=1` and the Console remained clean. No Player/Enemy behavior or serialized asset was changed.
- 2026-09-17 — Migrated Player Idle/Move to `PlayerIdleState` and `PlayerMoveState` without changing serialized data, legacy input, speed, jump, attack, Animator parameters, or Animation Events. Play Mode transition checks produced `Idle → Move → Idle`, horizontal velocities `+8/0/-8`, jump velocity `12`, and movement-lock velocities `8 → 0 → 8`; Console remained clean. Jump/Fall and Attack are intentionally not state-driven yet.
- 2026-09-17 — Migrated Player Jump/Fall to `PlayerJumpState` and `PlayerFallState` while keeping Space input, jump force `12`, horizontal air control, ground raycast, Animator parameters, and legacy Attack behavior. Runtime checks produced `Idle → Jump(12) → Fall(-3) → Idle/Move`, air-control velocities `+8/-8`, correct `Yvelocity`/`isGrounded` Animator values, and movement/jump lock behavior `0 → 8` after unlock. Console remained clean and no serialized asset changed. A focused physical keyboard feel check remains required before Attack migration.
