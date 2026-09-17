# RPGame completion roadmap

Status: Milestones 0–3 complete; Milestone 4 next
Prepared: 2026-09-17  
Effort unit: one focused developer-day is approximately 4–6 productive hours. Learning while following the course can multiply estimates by 1.5–2.5.

## Product target

Build a polished, playable 2D action-RPG vertical slice that follows the system progression of AlexDev's Unity 6 RPG course without blindly copying every lecture. The first complete slice should contain one finished level, a responsive player state machine, one full enemy, meaningful stats and combat, one skill branch, a small item/equipment loop, persistence, menus, audio, and a distributable build.

The project has progressed from its crash-course prototype into a combat vertical slice with state-driven actors, a traversable level, and reusable combat components. It still lacks the RPG progression, item, persistence, menu, audio, and release systems from the larger course target.

## Current capability map

| Area | Current state | Gap to RPG target |
|---|---|---|
| Player | FSM-driven Idle, Move, Jump, Fall, grounded Attack, damage/invulnerability and knockback receiving | No wall states, dash, combo, aerial attack, counter, hurt animation, or input abstraction |
| Enemy | Idle/Patrol/Chase/Attack/Hurt/Stunned/Retreat/Death FSM, explicit cooldown, two profile-driven variants | No counter behavior, boss, unique variant animations, or larger enemy roster |
| Combat | Animation-event hit timing, physical/elemental damage context, one-hit-per-swing, invulnerability, knockback, health bars, flash and death | No stat formulas, critical/armor/resistance calculation, named status effects, combo, aerial attack, counter, or dash |
| Level | Bounded forest traversal slice with follow camera, parallax, modular terrain, platforms, hazards, checkpoint and endpoint | No authored terrain tileset/Tilemap workflow, scene transition, or additional level |
| UI | Timer, kill count, world health bars, Game Over, Restart | No pause/settings, resource HUD, inventory, equipment, skill tree, tooltips, or save slots |
| Progression | None | No stats, XP/level, currency, skills, unlocks, equipment progression |
| Content | One player, two data-configured enemy prefabs/profiles, protected character | No loot, interactables, chests, items, recipes, merchants, storage or bosses |
| Persistence | None | No save/load, checkpoint, inventory/skill/stat persistence |
| Quality | MCP connection and Inspector contracts verified | No authored tests, complete smoke-test record, standalone build verification, profiler pass |

## Priority model

| Priority | System | Impact | Difficulty | Estimate |
|---|---|---:|---:|---:|
| P0 | Baseline smoke test and architecture decision | Very high | Low | 1–2 days |
| P0 | Entity/player finite state machine | Very high | High | 4–7 days |
| P0 | Responsive player states and combat input | Very high | High | 5–8 days |
| P0 | Enemy FSM and reliable combat loop | Very high | High | 5–8 days |
| P1 | Tilemap level, camera, hazards, checkpoints | High | Medium | 4–7 days |
| P1 | Health HUD, knockback, hit VFX, death flow | High | Medium | 3–5 days |
| P1 | Stat and damage model | High | High | 5–8 days |
| P1 | One skill branch and cooldown UI | High | High | 5–8 days |
| P2 | Items, loot, inventory and equipment | High | High | 6–10 days |
| P2 | XP, level, currency and rewards | Medium–high | Medium | 3–5 days |
| P2 | Crafting, shop and storage | Medium | High | 5–9 days |
| P2 | Save/load and scene/checkpoint persistence | Very high | High | 5–8 days |
| P3 | Audio, pause/settings, accessibility and polish | Medium | Medium | 4–7 days |
| P3 | Automated tests, profiling, builds and release | Very high | Medium | 4–7 days |

## Milestone 0 — Freeze and verify the prototype

Goal: create a trustworthy baseline before architectural work.

1. Run the complete current loop in Play Mode: movement, jump, player attack, enemy damage/death, both spawn sides, kill count, both Game Over paths, and Restart.
2. Record defects separately from feature requests.
3. Decide whether Game Over should pause at time scale `0` or intentionally slow to `0.5`.
4. Decide the intended enemy spawn cap (`0.5` code default or `0.7` serialized scene value) and document it.
5. Create a Git checkpoint/branch before the FSM migration.
6. Define a vertical-slice acceptance checklist and a short list of controls.

Exit criteria: current game behavior is reproducible, the Console is clean, and design decisions are written down.  
Estimate: 1–2 days. Difficulty: low. Risk: low.

Completion record (2026-09-17):

- Core-loop smoke test completed; sustained keyboard input remains a manual feel check, while movement/jump handlers were verified at their configured velocities.
- Game Over decision: pause completely with `Time.timeScale = 0`.
- Spawn balance decision: clamp enemy cooldown at `0.7` seconds in both code and Inspector.
- Baseline controls: `A`/`D` or arrows to move, Space to jump, left mouse to attack, Restart button after Game Over.
- Vertical-slice baseline: clean startup/HUD, bilateral spawning, working combat/kill count, both fail paths, restartable clean run, and no gameplay Console errors or warnings.
- Pre-FSM checkpoint: branch `codex/milestone-0-baseline`, commit `176fafe`.

## Milestone 1 — Scalable foundation and player FSM

Goal: replace the prototype's Update-driven conditionals with the architecture expected by later RPG systems.

1. Introduce `EntityStateMachine` and an abstract `EntityState` with `Enter`, `Update`, and `Exit`.
2. Identify reusable component boundaries for health, combat target detection, status/knockback receiving, and animation-event relay; perform the serialized-data migration with Milestone 3 combat data so values move only once.
3. Implement player states in this order: Idle → Move → Jump/Fall → Attack.
4. Preserve current animation parameters and hit timing during migration; verify one state at a time.
5. Defer WallSlide and WallJump until Milestone 2 supplies deliberate wall geometry and suitable visual feedback.
6. Defer Dash, grounded combo queue, and aerial attack until Milestone 3 defines their clips, bindings, and explicit cooldown/state gates.
7. Keep the verified legacy input path for the current vertical slice. Reconsider an Input System migration only when a supported device or rebinding requirement justifies it.

Recommended implementation:

- Plain C# state classes owned by the Player/Entity; avoid adding a third-party FSM.
- Input System package is already installed. Do not install another input library.
- C# events or a small input facade prevent states from polling unrelated controls.

Exit criteria: all locomotion and attack states transition deterministically; controls feel at least as responsive as the baseline; no animation-event or Inspector reference regressions.  
Estimate: 9–15 days. Difficulty: high. Risk: highest architectural milestone.

Completion record (2026-09-18):

- Step 1 complete on `codex/milestone-1-fsm-foundation`: added `EntityState` and `EntityStateMachine` as plain C# primitives.
- The primitives compile into `Assembly-CSharp` and expose the planned Enter/Update/Exit and Initialize/ChangeState/UpdateActiveState APIs.
- Player Idle, Move, Jump, and Fall now use dedicated states. Runtime checks preserved `+8/0/-8` horizontal velocities, jump force `12`, air control, apex/landing transitions, Animator velocity/ground values, and Animation Event movement locks.
- The user confirmed the focused locomotion checks. Player Attack now uses `PlayerAttackState` while preserving the existing `attack` trigger and `DisableMovementAndJump` → `DamageTargets` → `EnableMovementAndJump` event sequence.
- Runtime Attack checks passed entry, movement lock, exits to Idle/Move/Fall, target death, and kill-count increment. Script validation and the Unity Console are clean; no serialized asset changed.
- Milestone 1 exit criteria passed for the available base locomotion and grounded attack: transitions are deterministic, the user confirmed control feel, Animation Events remain intact, and no Inspector reference regressed.
- Scope was re-sequenced rather than stubbed: the project contains no wall, dash, combo, aerial, hurt, stun, or knockback clips. Wall traversal now belongs to Milestone 2 after level geometry exists; reusable combat components and advanced attacks belong to Milestone 3 alongside their data and content.

## Milestone 2 — Level, camera and traversal slice

Goal: provide a real level in which the state machine and combat can be evaluated.

1. Build one small Tilemap-based level with Ground, One-way/limits, decoration, and collision layers.
2. Use TilemapCollider2D plus CompositeCollider2D for stable collision geometry.
3. Add camera follow, dead zone/look-ahead, and bounds.
4. Add parallax layers without changing the established pixel-art direction.
5. Add one hazard, one fall/death boundary, one checkpoint, and a clear level endpoint.
6. Validate pixel-perfect import settings, sprite pivots, sorting layers, and camera resolution.
7. Add WallSlide/WallJump only if the authored level and available visual content support clear, testable behavior.

Recommended implementation:

- Unity Tilemap and 2D physics are already available.
- Cinemachine is suitable for follow/confiner behavior, but it is a new dependency and must be approved before installation. A small custom follow camera is a valid dependency-free alternative.

Exit criteria: player can traverse the complete level without camera leaks, collider snags, or sorting errors.  
Estimate: 4–7 days. Difficulty: medium.

Progress record (2026-09-17):

- Existing content audit found 12 reusable forest background layers, one flat Ground collider, and no authored Tilemap or terrain tileset.
- Added a dependency-free `CameraFollow2D`, bound it to Player, and configured horizontal clamps at X `0..35.8` while keeping Y fixed at `0`.
- Added a generated mossy forest terrain texture with Point filtering, no compression, Repeat wrapping, no mipmaps, and 128 PPU. Three Ground-layer chunks extend the walkable collision to X `45.8`; two duplicated background sets cover the extension.
- Added two raised terrain platforms, parallax on all three background groups, a checkpoint, visible ground hazard, fall boundary, and visible endpoint.
- Runtime raycasts found continuous Ground at seven seam/interior samples from X `16.7` through `45.7`, including both platforms. Camera checks reached both bounds, parallax moved at its configured factor, checkpoint/hazard/fall/endpoint flows passed, and the Console remained clean.
- Deliberate implementation adjustment: the project contained no source terrain tileset. The small vertical slice uses modular SpriteRenderer/BoxCollider2D chunks rather than generating an unreliable Tilemap palette. Sorting, Point filtering, 128 PPU, collision continuity, and camera framing were verified through Unity MCP.
- WallSlide/WallJump remain deferred because the project still has no matching clips or finalized wall-interaction rules; they will not be faked with unrelated animation content.

## Milestone 3 — Enemy FSM and production combat

Goal: turn the current moving damage source into an understandable, testable opponent.

1. Implement enemy Idle, Patrol/Walk, Battle/Chase, Attack, Hurt/Knockback, Stunned, Retreat, and Death states incrementally.
2. Replace per-frame attack trigger requests with an explicit attack decision and cooldown/state transition.
3. Introduce an `AttackData`/damage context carrying dealer, physical/elemental damage, knockback, and hit position.
4. Add hit invulnerability or one-hit-per-swing protection to prevent multi-collider/multi-event damage.
5. Add player and enemy health bars, hit VFX, knockback and clear death feedback.
6. Add counter/stun only after ordinary combat is reliable.
7. Create a second enemy by data/configuration reuse rather than copying all code.
8. Separate reusable health, target detection, status/knockback receiving, and Animation Event relay responsibilities during this combat-data migration.
9. Add Dash, grounded combo, and aerial attack only after their animation clips, cooldown rules, and legacy-input bindings are defined.

Recommended implementation:

- Prefer interfaces such as `IDamageable` for enemies, player, chests, and interactables.
- Keep Animation Events for impact frames but route payloads through the combat component.
- Do not introduce object pooling until profiling shows spawn/destruction spikes.

Exit criteria: enemy behavior is readable; one attack produces one expected hit; player and enemy death flows are reliable; combat survives repeated five-minute sessions without Console errors.  
Estimate: 8–13 days. Difficulty: high.

Completion record (2026-09-18):

- Added explicit Enemy Idle, Patrol, Chase, Attack, Hurt, Stunned, Retreat, and Death states. The `Attack` trigger is issued once on state entry and the original Animation Events still own movement lock, impact, and completion timing.
- Split combat into `EntityHealth`, `EntityCombat`, `KnockbackReceiver`, `AttackData`/`DamageContext`, `IDamageable`, and runtime world health bars. Player/ObjectToProtect/Enemy Inspector data was migrated through Unity MCP without hand-editing scene or prefab YAML.
- One target is accepted only once per swing. Isolated tests produced Player HP `10→9→9` for duplicate events and `→8` for the next swing; Enemy death still increments the kill counter.
- Added `ForestEnemy` and `SwiftForestEnemy` ScriptableObject profiles plus `EnemySwift.prefab`; the existing respawner now selects either validated prefab without duplicating behavior code.
- A natural 25-second smoke run spawned both profiles, exercised cooldown-paced attacks and reached the existing Game Over flow with a clean Console. Controlled accelerated sessions then exercised the base profile for 410 simulated seconds/262 hits and the Swift profile for 373 simulated seconds/237 hits, both with zero Console errors or warnings.
- Collider-accurate attack range uses `Collider2D.ClosestPoint`; this fixed an endurance-test edge case where an Enemy could stop a few hundredths of a unit before its overlap actually reached the target.
- Counter, Dash, combo, aerial attack, and bespoke hurt/death animations remain deliberately deferred because the required clips and explicit bindings/rules do not yet exist.

## Milestone 4 — Stats, progression and first skill

Goal: create the minimum RPG progression loop before building lots of content.

1. Define major, offensive and defensive stats and the exact formulas in a design note.
2. Store default enemy/stat configurations in ScriptableObjects; keep runtime mutable values on instances.
3. Implement physical damage, critical chance/power, armor mitigation, and health regeneration first.
4. Add XP, levels, skill points and currency rewards.
5. Add elemental damage/resistance only after physical balance is stable.
6. Add one status effect at a time: chilled → burned → electrified.
7. Build one small skill-tree branch with prerequisites/conflicts, tooltip, unlock state and one usable skill with cooldown UI.
8. Use the first skill as an end-to-end proof before authoring the full tree.

Recommended implementation:

- ScriptableObjects for immutable definitions: stat presets, SkillData and effect data.
- Plain serializable runtime models for current HP, modifiers, unlocks and cooldown state.
- Cache calculated stat totals and invalidate them when modifiers change.

Exit criteria: player earns a level/skill point, unlocks one skill, uses it in combat, and all displayed values match the underlying formulas.  
Estimate: 10–16 days. Difficulty: high.

## Milestone 5 — Items, loot, inventory and economy

Goal: complete a small but coherent obtain–use–equip–upgrade loop.

1. Define item categories and stable IDs: consumable, weapon, armor, material and currency.
2. Create ScriptableObject item definitions; do not store mutable inventory quantities inside shared assets.
3. Implement loot drops/pickups and an inventory model before building the full UI.
4. Add stacking, capacity rules, add/remove/use and deterministic selection.
5. Add inventory UI, tooltips, drag/click interaction and controller/keyboard navigation as required.
6. Add equipment slots and stat modifiers with clear equip/unequip behavior.
7. Add one recipe, one merchant and one storage container as vertical slices.
8. Expand crafting/shop content only after the data flows are covered by tests.

Recommended implementation:

- Stable string/GUID item IDs and an item database.
- ScriptableObjects for definitions; serializable DTOs for slots and save data.
- Built-in uGUI/TMP is sufficient. No UI framework dependency is required.

Exit criteria: enemy drops an item; player picks it up, inspects it, uses/equips it, sees the stat change, can buy/craft one item, and can transfer one item to storage.  
Estimate: 11–19 days. Difficulty: high.

## Milestone 6 — Persistence, menus and complete game loop

Goal: make progress survive scene changes and application restarts.

1. Define a versioned save schema before writing persistence code.
2. Save player transform/checkpoint, health/resources, stats/XP/currency, inventory/equipment, skill unlocks and world flags.
3. Resolve ScriptableObject definitions by stable ID during load; never serialize Unity object references directly.
4. Write JSON under `Application.persistentDataPath` using a temporary file and atomic replace where practical.
5. Implement New Game, Continue, save slot metadata, checkpoint save, failure recovery and schema version handling.
6. Add pause menu, settings and AudioMixer volume controls.
7. Add scene transition/loading UI if the vertical slice uses more than one gameplay scene.

Recommended implementation:

- Built-in JSON or a deliberately approved serializer; PlayerPrefs only for small settings, not the main save.
- A persistent bootstrap/GameManager can coordinate services, but avoid a single god object.

Exit criteria: a save made after combat and inventory changes restores the same checkpoint, stats, equipment, skills and world state after relaunch. Corrupt/missing saves fail safely.  
Estimate: 7–12 days. Difficulty: high.

## Milestone 7 — QA, polish and release

Goal: convert the vertical slice from “works on my machine” into a deliverable game build.

1. Add EditMode tests for stat formulas, modifiers, inventory stacking, item IDs and save migrations.
2. Add PlayMode smoke tests for damage/death, state transitions, UI opening and save/load integration where reliable.
3. Maintain a manual regression checklist for animation timing, input feel and visuals.
4. Profile CPU, allocations, physics and rendering before optimizing.
5. Add audio feedback, particles, screenshake/accessibility toggles, consistent UI states and credits/licenses.
6. Test multiple resolutions and input devices that are officially supported.
7. Produce clean development and release builds, test on another machine, and fix build-only failures.

Recommended implementation:

- Unity Test Framework is already installed.
- Use asmdefs for runtime and test boundaries once the codebase grows.
- Use the built-in Profiler and Frame Debugger; Addressables, dependency injection frameworks and async libraries are unnecessary until a demonstrated need exists.

Exit criteria: no blocking defects in the regression list, no recurring Console errors, acceptable frame time, save compatibility tested, and a distributable build completed end-to-end.  
Estimate: 6–10 days. Difficulty: medium.

## Delivery strategy

Do not attempt all course systems simultaneously. Use these three releases:

1. **Combat vertical slice:** Milestones 0–3. One level, complete movement, one polished enemy, reliable win/fail loop.
2. **RPG vertical slice:** Milestones 4–6. One skill branch, small inventory/equipment set, one recipe/shop/storage interaction, persistent progress.
3. **Release candidate:** Milestone 7 plus more content only after the systems are stable.

Estimated effort:

- Combat vertical slice: 22–37 focused developer-days.
- RPG vertical slice: an additional 28–47 days.
- Release hardening: an additional 6–10 days.
- Tutorial/course parity while learning: roughly 150–300 focused hours, depending on how much content is copied versus redesigned.

## Immediate next action

Start Milestone 4 by defining the exact stat, damage, XP, level, currency, and first-skill formulas in a design note. Then implement immutable stat/skill ScriptableObjects and runtime stat instances before wiring the first complete progression branch.
