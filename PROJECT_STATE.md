# RPGame project state

Last updated: 2026-09-18 (Asia/Ho_Chi_Minh)

This is the durable project handoff. It records verified facts, decisions, completed work, and remaining work. It is not a substitute for inspection: every task must validate relevant entries against current source and Unity MCP.

## Current status

RPGame is a small single-scene 2D action game. The player moves and jumps in a fixed side-view arena, protects a stationary character, and defeats enemies that spawn from both sides with an accelerating cadence. The run shows elapsed time and kill count and reaches Game Over when the player or protected character dies.

The core loop exists and the project opens in Unity 6. Milestones 0–5 are complete. The baseline is smoke-tested, Player locomotion/attack is state-driven, the arena is a bounded traversal slice, combat uses reusable components plus an explicit Enemy FSM, and the RPG vertical slice now includes stats, XP/levels, currency, skill points, a three-node branch, Power Strike, enemy loot, deterministic inventory, consumables, equipment, crafting, a merchant offer, and storage. Two data-configured enemy profiles spawn through the existing respawner. Game Over pauses completely and enemy spawn cadence is capped at 0.7 seconds. Persistence, menus/audio, authored automated tests, and standalone build verification remain unfinished.

## Verified environment

- Repository: `/Users/lenhat/Developer/RPGame-main`
- Git: active milestone branch `codex/milestone-5-items-economy`, based on Milestone 4 commit `ee6d7a3`.
- Unity: `6000.0.72f1` (`b731fd3ae857`).
- Entry scene: `Assets/Scenes/SampleScene.unity`; it is the only enabled build scene.
- Packages include URP `17.0.4`, Input System `1.19.0`, uGUI `2.0.0`, Test Framework `1.6.0`, and Unity MCP `v10.0.0` from CoplayDev.
- Codex Unity MCP endpoint: `http://127.0.0.1:8080/mcp`.
- Unity MCP is connected and responding through HTTP. On 2026-09-17 it reported one active instance, `RPGame-main@5489ec430c09e367`, with the correct project root and Unity version. Five consecutive read cycles completed successfully without stale state or routing failures (about 1.5–1.8 seconds per cycle).
- Authored asset inventory at this audit: 57 C# runtime scripts, 2 gameplay prefabs, 16 ScriptableObject data assets (2 enemy, 5 item, 1 item database, 1 recipe, 4 stat, 3 skill), 3 Animator Controllers, 3 materials, 1 generated terrain texture, and 2 scenes (one gameplay scene plus the URP scene template).
- No authored EditMode or PlayMode test files were found.

## Current gameplay and controls

- Horizontal movement: legacy `Horizontal` axis (`A`/`D` or arrow keys).
- Jump: Space while grounded and movement is enabled.
- Player attack: left mouse button while grounded and movement is enabled; `PlayerAttackState` invokes the existing base attack path and Animator trigger `attack`.
- Skill controls: `K` toggles the skill tree, `1/2/3` unlock its three nodes, and `Q` uses Power Strike while grounded. These remain on the legacy input path.
- Inventory controls: `I` toggles the inventory; Up/Down select; Enter uses/equips; `B` buys the merchant sword; `C` crafts the potion recipe; `T` stores one selected item; `R` retrieves the first stored item; `U`/`O` unequip weapon/armor. Seven uGUI buttons provide click equivalents for the primary inventory/economy actions.
- Enemy behavior: Idle, Patrol, Chase, Attack, Hurt, Stunned, Retreat, and Death states select the nearest living Player-layer target.
- Enemy attack: an explicit Attack-state decision checks the target collider, fires Animator trigger `Attack` once, and observes a profile cooldown; it no longer requests the trigger every frame.
- Attacks apply a `DamageContext` on the existing Animation Event impact frame. A target can be hit only once per swing, and short hit invulnerability protects against overlapping attackers/events.
- Character stats calculate physical power, critical hits, armor, elemental resistance, and health regeneration from immutable profiles plus keyed runtime equipment modifiers. Chilled, Burned, and Electrified runtime effects provide slow, damage-over-time, and damage-vulnerability behavior.
- Enemy deaths grant profile-defined XP/currency. XP requirements are `10 + 5 × (level - 1)` and every level grants one skill point.
- Base Forest Enemy drops `forest_herb`; Swift Forest Enemy drops `health_potion`. Pickups enter the Player's 12-slot inventory only when capacity permits.
- Enemy deaths increment the UI kill counter.
- Elapsed time uses `Time.timeSinceLevelLoad`, so it resets when the scene reloads.
- Game Over activates a UI panel and sets `Time.timeScale` to `0`; Restart reloads the active scene and `UI.Awake()` restores time scale to `1`.
- Enemy spawn cooldown starts at 3 seconds and decreases by `0.05` per spawn. Both the code default and `SampleScene` serialize the agreed `0.7`-second cap.

## Architecture and serialization contracts

- `Entity.cs`: common component caching, movement hooks, grounding, facing, animation parameters, hit material feedback, combat delegation, and death behavior.
- `Combat/EntityHealth.cs`: reusable current/max health, hit invulnerability, damage/death events, and `IDamageable` implementation.
- `Combat/EntityCombat.cs`: serialized AttackPoint/radius/mask/data, swing lifecycle, overlap targeting, and one-hit-per-target enforcement.
- `Combat/DamageContext.cs` and `AttackData.cs`: dealer, physical/elemental damage, element, knockback, hit position, stun duration, and swing identity.
- `Combat/KnockbackReceiver.cs`: reusable Rigidbody2D knockback window; `WorldHealthBar.cs` supplies runtime world-space health feedback.
- `Combat/EnemyProfile.cs`: immutable enemy health/movement/detection/cooldown/tint configuration shared by both enemy prefabs.
- `Stats/CharacterStatProfile.cs` and `CharacterStats.cs`: immutable defaults plus per-actor runtime formula access for health, power, armor, criticals, regeneration, and elemental resistance.
- `Combat/StatusEffectReceiver.cs`: timed Chilled/Burned/Electrified effects without mutating shared data assets.
- `Progression/PlayerProgression.cs`: current-run level, XP, skill points, currency, unlock set, prerequisites, and conflicts.
- `Progression/SkillDefinition.cs` and `PlayerSkillController.cs`: stable skill data and Power Strike activation/cooldown/branch upgrades.
- `Progression/ProgressionUI.cs`: HUD progression/cooldown labels and the keyboard-operated skill-tree panel.
- `Items/ItemDefinition.cs`, `ItemDatabase.cs`, and `RecipeDefinition.cs`: immutable definitions and stable-ID lookup for five item categories and the first recipe.
- `Items/InventoryModel.cs`: mutable runtime slot quantities, deterministic stacking, capacity, add/remove, and lookup behavior shared by Player inventory and storage.
- `Items/EquipmentController.cs` and `PlayerInventoryController.cs`: Weapon/Armor equip, replacement, unequip, consumable use, and keyed stat modifier application.
- `Items/EnemyLootDrop.cs` and `ItemPickup2D.cs`: data-configured enemy drops and visible trigger-based collection.
- `Items/EconomyServices.cs` and `InventoryUI.cs`: one merchant offer, recipe, storage round trip, tooltips, keyboard navigation, and persistent click bindings.
- `Player.cs`: legacy input, FSM ownership, movement helpers, attack request, and player-death Game Over.
- `Enemy.cs`: owns the Enemy FSM, nearest-target selection, collider-accurate attack decisions, profile configuration, and kill-count update.
- `ObjectToProtect.cs`: faces the player and triggers Game Over on death.
- `Enemy_Respawner.cs`: validates both enemy prefabs/spawn points, selects a configured enemy variant, flips right-side spawns, and accelerates spawn cadence.
- `CameraFollow2D.cs`: smooth dependency-free Player follow with serialized X/Y bounds; current scene bounds are X `0..35.8` and fixed Y `0`.
- `ParallaxBackground2D.cs`: moves each repeated background group by a small fraction of camera movement while preserving their spacing.
- `PlayerRespawnController.cs`: owns the current checkpoint position and restores Player position/velocity after a traversal hazard.
- `Checkpoint2D.cs`, `RespawnTrigger2D.cs`, and `LevelEndpoint2D.cs`: checkpoint activation, hazard/fall respawn, and endpoint completion marker behavior.
- `Entity_AnimationEvents.cs`: forwards attack clip events to movement locking and `DamageTargets()`.
- `UI.cs`: scene singleton, timer, kill count, Game Over UI, and restart.
- `StateMachine/EntityState.cs`: abstract plain-C# state contract with `Enter`, `Update`, and `Exit`, bound to an Entity and its state machine.
- `StateMachine/EntityStateMachine.cs`: initializes one starting state, updates the active state, and performs guarded Exit/Enter transitions.
- `StateMachine/PlayerState.cs`: typed base for Player states.
- `StateMachine/PlayerIdleState.cs` and `PlayerMoveState.cs`: own zero/input-driven horizontal velocity and deterministic Idle/Move transitions while preserving movement locks.
- `StateMachine/PlayerJumpState.cs` and `PlayerFallState.cs`: own jump impulse, airborne horizontal control, apex transition, and landing selection back to Idle/Move.
- `StateMachine/PlayerAttackState.cs`: enters through the existing grounded attack trigger, preserves horizontal movement-lock behavior, and exits through the existing final Animation Event to Idle, Move, or Fall.
- `StateMachine/Enemy*State.cs`: deterministic Idle/Patrol/Chase/Attack/Hurt/Stunned/Retreat/Death transitions with an Animation Event completion path and attack failsafe.

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

Milestone 3 Unity MCP audit completed on 2026-09-18:

- Player has `EntityHealth` (10 HP, 0.35-second invulnerability), `EntityCombat` (AttackPoint, radius 1, Enemy mask 128), `KnockbackReceiver`, and `WorldHealthBar`.
- ObjectToProtect has `EntityHealth` (10 HP, 0.35-second invulnerability), `KnockbackReceiver`, and `WorldHealthBar`; it intentionally has no attack component.
- `Enemy.prefab` and `EnemySwift.prefab` both contain EntityHealth/EntityCombat/KnockbackReceiver/WorldHealthBar. Their EnemyProfile assets provide 2 HP/2.2 speed and 1 HP/3.2 speed respectively.
- Enemy_Respawner keeps the original Enemy prefab and now also references `EnemySwift.prefab`; the `3.0/0.05/0.7` cadence settings remain unchanged.
- Player attack events remain at `0.000/0.333/0.667`; Enemy attack events remain at `0.000/0.583/1.417`. Receiver names still resolve to `Entity_AnimationEvents`.

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

## Milestone 5 smoke test — 2026-09-18

Test environment: Unity `6000.0.72f1`, `SampleScene`, Play Mode and Inspector validation through Unity MCP.

| Check | Result | Evidence |
| --- | --- | --- |
| Stable item data | Pass | ItemDatabase reported unique IDs for all five categories; invalid IDs and zero-quantity adds were rejected. |
| Stacking/capacity | Pass | Adding 21 herbs deterministically produced slot quantities `20 + 1`; 12 one-stack armor items filled all Player slots and the 13th add was rejected. |
| Consumable | Pass | After runtime damage, Health Potion healed Player `7 → 10` and was removed. |
| Weapon equip/unequip | Pass | Iron Sword changed Physical Power `0 → 2 → 0`. |
| Armor equip/unequip | Pass | Leather Armor changed Max Health `10 → 12 → 10` and Armor `1 → 16 → 1`. |
| Merchant | Pass | A sword purchase changed Gold `10 → 6` and added the item. |
| Crafting | Pass | Two Forest Herbs were consumed and potion quantity changed `0 → 1`. |
| Storage | Pass | One potion deposited into the 8-slot storage and was retrieved without quantity loss. |
| Loot and pickup | Pass | Fatal damage to a base Enemy created its configured Forest Herb pickup; invoking the same trigger receiver used by 2D physics changed herb quantity `0 → 1`. |
| UI/Inspector | Pass | All six InventoryUI references resolve; all seven buttons have exactly one RuntimeOnly persistent call to the expected InventoryUI method. Visual Game-view inspection showed readable inventory, tooltip, equipment/stat, storage, and action-button regions without obscuring the existing HUD. |
| Compilation/scene/Console | Pass | All 15 changed/new scripts reported zero validation errors; scene validation found zero missing scripts or broken prefabs; final Console contained zero errors and zero warnings. |

## Decisions and fixed constraints

- Keep the current pixel-art visual direction unless the user explicitly requests a change.
- Keep the current legacy-input gameplay path; the installed new Input System is not authorization to migrate controls.
- Keep animation-driven hit timing. Do not replace Animation Events or rename receiver methods as incidental cleanup.
- Continue through `ROADMAP.md` milestones without routine confirmation. Stop only for destructive actions, dependencies, or unresolved choices that materially change art direction, input strategy, architecture, or game rules.
- Keep milestone history easy to review. Milestone 1 already has pushed incremental commits, so do not rewrite it; finish it with a clearly named completion commit. From Milestone 2 onward, target one clearly named completion commit per milestone.
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
- Milestone 1 completed on branch `codex/milestone-1-fsm-foundation`: added compiled FSM primitives and migrated Player Idle/Move/Jump/Fall/Attack while preserving the existing animation-driven hit path.
- Milestone 2 completed on branch `codex/milestone-2-level-traversal`: added the bounded camera, parallax background extension, generated modular terrain, raised platforms, checkpoint, hazard/fall respawn, and endpoint.
- Milestone 3 completed on branch `codex/milestone-3-production-combat`: added reusable combat components, Enemy FSM/cooldown, one-hit-per-swing contexts, invulnerability, knockback, world health bars, and two data-configured enemy variants.
- Milestone 4 completed on branch `codex/milestone-4-progression-skill`: added fixed stat/progression formulas, runtime stat/status systems, XP/levels/currency, skill points, a mutually exclusive three-node skill branch, Power Strike, cooldown/HUD UI, and verified Enemy rewards.
- Milestone 5 completed on branch `codex/milestone-5-items-economy`: added stable item/recipe definitions, deterministic inventory and storage, enemy loot/pickups, consumables, Weapon/Armor equipment modifiers, one merchant offer, one recipe, and the inventory/economy UI.

## Open questions and known risks

- `Entity.Awake()` assumes Rigidbody2D, Collider2D, child Animator, and child SpriteRenderer exist. These required components were verified for all current Entity types; future prefabs still need the same validation.
- Health bars create a minimal runtime SpriteRenderer visual from a generated 1×1 texture. This avoids a new art dependency but should be replaced by authored UI art during Milestone 7 polish if suitable assets become available.
- Counter, Dash, grounded combo, aerial attack, and unique hurt/death clips remain deferred because the repository still has no matching animation content or finalized bindings/rules.
- Milestone 4–5 progression, inventory, equipment, currency, and storage are intentionally current-run only and reset on scene reload; persistence belongs to Milestone 6.
- No automated tests currently protect combat, spawning, UI, or restart behavior.
- A current standalone player build result has not been recorded.

## Backlog

The full proposed completion sequence, effort estimates and milestone exit criteria are maintained in `ROADMAP.md`. The current recommended target is a combat vertical slice first, then an RPG vertical slice, then release hardening.

1. Start Milestone 6 by freezing a versioned stable-ID save schema for checkpoint, stats/progression, inventory/equipment, skills, storage, and world flags.
2. Implement safe JSON save/load under `Application.persistentDataPath`, including missing/corrupt-file recovery and schema migration hooks.
3. Add New Game/Continue plus pause/settings/audio controls without replacing the current input architecture.
4. Add focused EditMode/PlayMode tests for formula, inventory, ID, save migration, and interaction logic during Milestone 7 hardening.
5. Produce and smoke-test a standalone build during release hardening.

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
- 2026-09-17 — After the user confirmed the focused locomotion checks, migrated Player Attack to `PlayerAttackState` without changing the `attack` Animator trigger or the `DisableMovementAndJump` → `DamageTargets` → `EnableMovementAndJump` event sequence. Runtime state checks passed Attack entry, movement lock, and exits to Idle/Move/Fall; a hit-path check reduced a target to zero health, incremented kills to `1`, and returned to Idle. Script validation reported zero diagnostics, the Console was clean, Play Mode exited, and no scene/prefab/animation/material asset changed.
- 2026-09-17 — Recorded standing authorization to continue roadmap milestones without routine confirmation and the milestone commit policy. Existing pushed Milestone 1 commits will not be rewritten; the one-completion-commit convention applies cleanly from Milestone 2 onward.
- 2026-09-17 — Closed Milestone 1 as the verified base Player FSM. Asset inspection found only Idle/Move/Jump/Fall/Attack player clips and no wall, dash, combo, aerial, hurt, stun, or knockback content. Wall traversal was moved behind Milestone 2 level geometry; combat component separation and advanced attacks were moved into Milestone 3, where their data, clips, cooldown rules, and bindings can be designed together. The legacy input path remains the fixed strategy for the current vertical slice.
- 2026-09-17 — Started Milestone 2 on `codex/milestone-2-level-traversal`. The asset audit found 12 reusable layered forest backgrounds but no Tilemap or terrain tileset. Added `CameraFollow2D`, bound it to Player through Unity MCP, and set horizontal bounds `0..35.8`. Generated and imported `ForestGroundTile.png` as a Point-filtered, uncompressed Sprite at 128 PPU, duplicated the existing background layers twice, added three contiguous Ground-layer terrain chunks, and moved the right level limit to world X `45.8`.
- 2026-09-18 — Completed Milestone 2. Added two raised terrain platforms, group parallax, Player checkpoint state, a visible ground hazard, an invisible fall boundary, and a visible endpoint marker. The project had no source tileset, so this small slice deliberately uses modular SpriteRenderer/BoxCollider2D chunks instead of manufacturing a fragile Tilemap palette; all scene/Inspector edits were made through Unity MCP. Play Mode verified checkpoint activation at `(19,-1.5)`, hazard and fall respawn with zero velocity, endpoint activation/color, camera clamps at X `0/35.8`, parallax displacement, and continuous Ground hits from X `16.7..45.7` including both platforms. All six Milestone 2 scripts validated with zero diagnostics, the Console was clean, and scene validation found no missing scripts or broken references.
- 2026-09-18 — Completed Milestone 3 on `codex/milestone-3-production-combat`. Migrated Player, ObjectToProtect, and both Enemy prefabs to `EntityHealth`/`EntityCombat`/`KnockbackReceiver` through Unity MCP; preserved layer masks, AttackPoints, trigger casing, and all three Animation Events. Added Enemy Idle/Patrol/Chase/Attack/Hurt/Stunned/Retreat/Death states, explicit attack cooldown, physical/elemental `DamageContext`, per-swing target de-duplication, hit invulnerability, knockback, health bars, two EnemyProfile assets, and a profile-driven Swift enemy prefab. Runtime tests proved duplicate impact events yield `10→9→9` while a new swing yields `8`, Enemy attacks reduced Player HP only at cooldown-paced impact frames, both profiles spawned naturally, and a player kill still reached kill count `1`. A collider-edge defect found during endurance testing was fixed with `Collider2D.ClosestPoint`. Base and Swift profiles then completed controlled 410-second and 373-second combat simulations with 262 and 237 valid hits respectively and zero Console errors/warnings. Editor returned idle with a clean Console.
- 2026-09-18 — Completed Milestone 4 on `codex/milestone-4-progression-skill`. Added `Docs/STAT_PROGRESSION_DESIGN.md`, four immutable CharacterStatProfile assets, runtime critical/armor/resistance/regeneration formulas, Chilled/Burned/Electrified effects, profile rewards, current-run XP/level/skill-point/currency state, three SkillDefinition assets, Power Strike and two mutually exclusive upgrades, plus progression/cooldown/skill-tree UI. Unity MCP migrated all actor components and Canvas references. Verified half-up formula results (2.5× base 1 = 3, armor 100→99, 10% fire resistance 100→90), Chilled multiplier 0.65, Electrified raw 4→5, Burned 1-HP tick, regeneration back to full, reward flow `12 XP/4 Gold → level 2, 2/15 XP, 1 SP`, Power Strike `2 HP→0`, 4-second cooldown gating, Quick Recovery 2.5-second cooldown, and symmetric branch conflict rejection. Natural spawning retained stat profiles and the final Console was clean.
- 2026-09-18 — Completed Milestone 5 on `codex/milestone-5-items-economy`. Added `Docs/ITEM_ECONOMY_DESIGN.md`, five immutable item definitions, ItemDatabase, one recipe, deterministic Player/storage inventories, enemy loot and pickup components, consumable use, Weapon/Armor equip/unequip modifiers, safe currency spending, one merchant offer, crafting and storage services, and the inventory/economy panel with keyboard plus persistent click controls. Unity MCP configured Player, Canvas, economy scene object, both Enemy prefabs, and all asset references. Runtime verification passed stable IDs, stacking/capacity, consume/equip/unequip, purchase, recipe, storage, enemy-death loot, pickup collection, UI bindings, clean scene validation, and a zero-error/warning Console.
