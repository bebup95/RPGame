# RPGame completion roadmap

Status: Milestone 0 frozen; Milestone 1 ready for reviewed implementation  
Prepared: 2026-09-17  
Effort unit: one focused developer-day is approximately 4–6 productive hours. Learning while following the course can multiply estimates by 1.5–2.5.

## Product target

Build a polished, playable 2D action-RPG vertical slice that follows the system progression of AlexDev's Unity 6 RPG course without blindly copying every lecture. The first complete slice should contain one finished level, a responsive player state machine, one full enemy, meaningful stats and combat, one skill branch, a small item/equipment loop, persistence, menus, audio, and a distributable build.

The current project is best treated as a completed crash-course prototype. It already implements the protect-the-girl survival loop shown in AlexDev's free beginner project, but it does not yet contain the scalable architecture or RPG systems from the main course.

## Current capability map

| Area | Current state | Gap to RPG target |
|---|---|---|
| Player | Horizontal movement, jump, flip, ground check, one attack | No FSM, wall states, dash, combo, aerial attack, counter, knockback, input abstraction |
| Enemy | Walks in facing direction, overlap detection, one attack, death | No idle/patrol/chase/battle/retreat/stun FSM or multiple enemy types |
| Combat | Animation-event hit timing, one damage per hit, flash, death | No damage data, variable damage, invulnerability, knockback, health bars, critical/armor/element/status systems |
| Level | One fixed arena and background | No authored Tilemap level, camera follow/confiner, parallax system, hazards, checkpoints, scene flow |
| UI | Timer, kill count, Game Over, Restart | No HUD health/resources, pause/settings, inventory, equipment, skill tree, tooltips, save slots |
| Progression | None | No stats, XP/level, currency, skills, unlocks, equipment progression |
| Content | One player, one enemy prefab, protected character | No loot, interactables, chests, items, recipes, merchants, storage or bosses |
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
2. Separate reusable components: health, combat target detection, status/knockback receiver, and animation-event relay.
3. Implement player states in this order: Idle → Move → Jump/Fall → Attack.
4. Preserve current animation parameters and hit timing during migration; verify one state at a time.
5. Add WallSlide and WallJump only after base locomotion is stable.
6. Add Dash, grounded combo queue, and aerial attack behind explicit cooldown/state gates.
7. Decide whether to migrate to the installed Input System. Course parity favors it, but this is a deliberate migration: create an action map, preserve current keyboard/mouse bindings, then remove direct legacy input only after equivalence tests.

Recommended implementation:

- Plain C# state classes owned by the Player/Entity; avoid adding a third-party FSM.
- Input System package is already installed. Do not install another input library.
- C# events or a small input facade prevent states from polling unrelated controls.

Exit criteria: all locomotion and attack states transition deterministically; controls feel at least as responsive as the baseline; no animation-event or Inspector reference regressions.  
Estimate: 9–15 days. Difficulty: high. Risk: highest architectural milestone.

## Milestone 2 — Level, camera and traversal slice

Goal: provide a real level in which the state machine and combat can be evaluated.

1. Build one small Tilemap-based level with Ground, One-way/limits, decoration, and collision layers.
2. Use TilemapCollider2D plus CompositeCollider2D for stable collision geometry.
3. Add camera follow, dead zone/look-ahead, and bounds.
4. Add parallax layers without changing the established pixel-art direction.
5. Add one hazard, one fall/death boundary, one checkpoint, and a clear level endpoint.
6. Validate pixel-perfect import settings, sprite pivots, sorting layers, and camera resolution.

Recommended implementation:

- Unity Tilemap and 2D physics are already available.
- Cinemachine is suitable for follow/confiner behavior, but it is a new dependency and must be approved before installation. A small custom follow camera is a valid dependency-free alternative.

Exit criteria: player can traverse the complete level without camera leaks, collider snags, or sorting errors.  
Estimate: 4–7 days. Difficulty: medium.

## Milestone 3 — Enemy FSM and production combat

Goal: turn the current moving damage source into an understandable, testable opponent.

1. Implement enemy Idle, Patrol/Walk, Battle/Chase, Attack, Hurt/Knockback, Stunned, Retreat, and Death states incrementally.
2. Replace per-frame attack trigger requests with an explicit attack decision and cooldown/state transition.
3. Introduce an `AttackData`/damage context carrying dealer, physical/elemental damage, knockback, and hit position.
4. Add hit invulnerability or one-hit-per-swing protection to prevent multi-collider/multi-event damage.
5. Add player and enemy health bars, hit VFX, knockback and clear death feedback.
6. Add counter/stun only after ordinary combat is reliable.
7. Create a second enemy by data/configuration reuse rather than copying all code.

Recommended implementation:

- Prefer interfaces such as `IDamageable` for enemies, player, chests, and interactables.
- Keep Animation Events for impact frames but route payloads through the combat component.
- Do not introduce object pooling until profiling shows spawn/destruction spikes.

Exit criteria: enemy behavior is readable; one attack produces one expected hit; player and enemy death flows are reliable; combat survives repeated five-minute sessions without Console errors.  
Estimate: 8–13 days. Difficulty: high.

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

Begin Milestone 1 as a sequence of small reviewed steps: introduce only the state-machine primitives first, compile and test, then migrate Idle/Move before Jump/Fall and Attack. Preserve the legacy input bindings, Animator parameters, Animation Events, and current combat timing until equivalence is demonstrated.
