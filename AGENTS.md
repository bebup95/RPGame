# AGENTS.md

## Project overview

This repository is a small single-scene 2D action RPG made with Unity 6 and C#. The player moves through a fixed side-view arena, protects a stationary character, and kills skeleton enemies that spawn from both sides at an increasing rate. A run ends when either the player or the protected character dies.

Open the project with **Unity 6000.0.72f1**. The project uses URP's 2D Renderer, 2D physics, Animator Controllers, uGUI, and TextMesh Pro. `Assets/Scenes/SampleScene.unity` is the only enabled build scene and is the gameplay entry point.

## Repository layout

- `Assets/Script/` contains all authored gameplay code. There are no namespaces or assembly definition files; these scripts compile into `Assembly-CSharp`.
- `Assets/Scenes/SampleScene.unity` contains the whole playable level, player, protected character, UI, spawn points, camera, and background.
- `Assets/Prefab/Enemy.prefab` is the skeleton spawned at runtime.
- `Assets/Animations/Controllers/` contains the Player, Enemy, and protected-character Animator Controllers.
- `Assets/Animations/Animations/` contains clips. Attack clips carry gameplay-critical Animation Events.
- `Assets/Graphics/` contains source textures and sliced character/background sprites.
- `Assets/Materials/` contains the damage-flash material and the player's 2D physics material.
- `Assets/Settings/` contains the URP 2D renderer/pipeline assets and an Input System action asset.
- `Packages/manifest.json` and `Packages/packages-lock.json` define package dependencies.
- `ProjectSettings/` is committed Unity configuration. Do not treat generated IDE files as authoritative.
- `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `*.csproj`, and `*.slnx` are generated/local artifacts and should not be committed.

## Gameplay architecture

### Entity and combat

`Entity` is the common base class for `Player`, `Enemy`, and `ObjectToProtect`. It caches `Rigidbody2D`, `Collider2D`, child `Animator`, and child `SpriteRenderer` references in `Awake()`, initializes health, handles ground detection and facing, drives compatible Animator parameters, applies damage feedback, and implements death.

Combat is animation-driven:

1. `Player.HandleAttack()` or `Enemy.HandleAttack()` sets an Animator trigger.
2. The attack clip invokes `Entity_AnimationEvents.DisableMovementAndJump` at its start.
3. The clip invokes `Entity_AnimationEvents.DamageTargets` on the impact frame.
4. `Entity.DamageTargets()` performs `Physics2D.OverlapCircleAll` using the serialized attack point, radius, and target mask.
5. The clip invokes `Entity_AnimationEvents.EnableMovementAndJump` near its end.

Do not rename these event receiver methods or remove `Entity_AnimationEvents` from the animated child without updating every attack clip. The methods currently used by Animation Events are private except for `DamageTargets`; Unity invokes them by name.

Animator parameter names are case-sensitive and intentionally differ by controller:

- Player: `Xvelocity`, `Yvelocity`, `isGrounded`, and trigger `attack`.
- Enemy: `Xvelocity` and trigger `Attack`.
- `Entity` checks whether velocity/ground parameters exist before writing them, allowing the protected character's parameterless controller to share the base class.

### Player

`Player` reads the legacy Input Manager directly:

- `A`/`D` or Left/Right Arrow: horizontal movement through the `Horizontal` axis.
- Space: jump, only while grounded and movement is enabled.
- Left Mouse Button: attack.

Although the new Input System package and `InputSystem_Actions.inputactions` are present, gameplay does not use that asset. Project setting `activeInputHandler: 2` enables both input backends. Do not migrate input as an incidental change.

The scene currently serializes player health `10`, speed `8`, jump force `12`, attack radius `1`, Ground mask, and Enemy target mask.

### Enemies and spawning

`Enemy` continuously moves in `facingDir`, detects targets with an overlap circle, and repeatedly requests the `Attack` trigger while a target is in range. On death it increments `UI.Instance`'s kill counter before the base class destroys the object.

`Enemy_Respawner` finds the player in `Awake()`, waits for its cooldown, then randomly spawns `Enemy.prefab` at one of two scene transforms. It flips enemies spawned to the player's right. The scene starts at a 3-second cooldown, subtracts `0.05` after each spawn, and clamps at `0.7` seconds. Preserve the cooldown cap when changing difficulty logic.

### Protected character and game over

`ObjectToProtect` is an `Entity` with 10 health. It has no attack configuration. Its death and the player's death both call `UI.Instance.EnableGameOverUI()`.

`UI` is a scene singleton (a simple public static field, not a persistent service). It updates elapsed time, displays kill count, activates the Game Over panel, changes `Time.timeScale` to `0.5`, and reloads the active scene from the Restart button. The button's persistent UnityEvent must continue to target `UI.RestartLevel()`.

The displayed timer uses `Time.time`, which is global time since startup and therefore does not reset to zero merely because the scene reloads.

## Scene and serialization contracts

The scene and prefab rely heavily on Inspector references. When changing scripts, preserve serialized field names unless a migration is deliberate; use `FormerlySerializedAs` for renames. Verify all references in the Inspector after changing component structure.

Important project layers are:

- Layer 6: `Ground`
- Layer 7: `Enemy`
- Layer 8: `Player` (both the player and protected character use this layer)
- Layer 9: `Level_Limits`

The player attacks layer 7. Enemies attack layer 8. Player and enemy ground checks target layer 6. Changing layer numbers, object layers, or masks without updating both scene and prefab will silently break combat or grounding.

`SampleScene` contains these gameplay roots: `Main Camera`, `Global Light 2D`, `Level`, `Player`, `ObjectToProtect`, `Canvas`, `EventSystem`, and `Enemy_Respawner`. The spawner references `Enemy.prefab` and two childless transforms named `Point` and `Point (1)`.

Keep every Unity asset's `.meta` file paired with it. Move and rename assets from Unity when practical so GUID-backed references survive. Avoid hand-editing `.unity`, `.prefab`, `.controller`, `.anim`, and `.asset` YAML unless the change is small, intentional, and verified by reopening Unity.

## Coding conventions

- Use C# 9-compatible syntax and Unity lifecycle methods.
- Follow the existing style: one MonoBehaviour per file, class and file names match, four-space indentation, braces on new lines, and Inspector fields as `[SerializeField] private/protected` rather than public state.
- Keep reusable entity behavior in `Entity`; put actor-specific movement, attack, and death behavior in overrides.
- Use `Rigidbody2D.linearVelocity`, matching the Unity 6 API already used here.
- Cache component references in `Awake()` rather than repeatedly calling `GetComponent` in `Update()`.
- Keep physics work in 2D APIs and preserve the Rigidbody2D Z-rotation constraint.
- Guard scene lookups, singleton access, component access, array references, and Unity's destroyed-object null semantics. A reference to a destroyed `UnityEngine.Object` compares equal to null even though the managed wrapper still exists.
- Animator hashes are preferred for frequently written parameters. Trigger spelling must match its controller exactly.
- Do not add `UnityEditor` dependencies to runtime scripts. Editor-only code belongs under an `Editor/` folder or behind `#if UNITY_EDITOR`.
- Do not edit generated `Assembly-CSharp.csproj` or solution files; Unity regenerates them.
- Keep changes focused. Do not reimport or reserialize unrelated assets, because that produces noisy YAML/meta diffs.

## Running and verification

### In the Editor

1. Open the repository root with Unity `6000.0.72f1`.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Enter Play Mode and focus the Game view before sending keyboard input.
4. Check movement, jumping, attack timing, damage, enemy spawning from both sides, kill count, both game-over paths, and Restart.
5. Check the Console after gameplay, not only after compilation. Let the player die once because the known destroyed-reference failure occurs after death.

### Command-line compile/import check (macOS)

Run this only when the same project is not already open in Unity:

```sh
/Applications/Unity/Hub/Editor/6000.0.72f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit \
  -projectPath "$(pwd)" \
  -logFile -
```

Treat a nonzero exit, compiler error, serialization failure, or runtime exception as a failed verification. There are currently no authored EditMode or PlayMode tests. If behavior grows, add tests under a dedicated `Assets/Tests/` assembly rather than mixing test code with runtime scripts.

For gameplay or serialization changes, a successful C# compile is not enough: run the affected scene manually. Changes to runtime code should also be checked in a non-Editor player build when practical.

## Known issues in the current baseline

Do not conceal these failures or assume they were introduced by a new change. Fix them only when they are in scope, and update this section after verifying a fix.

- `ObjectToProtect.HandleFlip()` has an inverted null guard: it returns while the player exists, then dereferences `player` after the player has been destroyed. Letting the player die produces repeated `MissingReferenceException` messages at `Assets/Script/ObjectToProtect.cs:25`.
- `UI.cs` imports `UnityEditor.ShaderGraph.Internal` through an unused static `using`. Runtime scripts must not depend on `UnityEditor`; remove this before relying on standalone/player builds.
- `Entity.PlayDamageFeedback()` checks `damageFeedbackCoroutine` but never assigns the result of `StartCoroutine`, so overlapping damage feedback cannot actually stop the prior coroutine.
- `UI.RestartLevel()` reloads the scene but the timer displays global `Time.time`, so the visible timer continues across restarts.
- `UI.Instance` has no duplicate protection or teardown. It is safe only under the current single-scene, single-Canvas arrangement.
- `Enemy_Respawner.CreateNewEnemy()` assumes `respawnPoints` itself and `enemyPrefab` are assigned, and assumes the prefab has an `Enemy` component. Inspector configuration is part of the contract.
- The project currently has no Git metadata in this working folder and no automated tests. Use file-level review and Unity verification; do not assume `git diff` or a test suite is available.

## Definition of done

Before handing off a change:

- Confirm Unity compiles without new errors.
- Run `SampleScene` through the affected behavior and inspect the Console.
- Verify scene/prefab Inspector references and layer masks if serialized fields or assets changed.
- Include matching `.meta` files for every added/moved asset.
- Exclude generated folders and IDE files from the change.
- Report which checks were run, plus any known issue that prevented complete verification.
