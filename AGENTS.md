# RPGame working agreement

This file contains durable instructions for every Codex task in this repository. Global guidance from `~/.codex/AGENTS.md` is inherited automatically; that file was empty when this document was refreshed on 2026-09-17.

## Required startup sequence

1. Read `PROJECT_STATE.md` before doing any work.
2. Inspect the relevant source files and current Git status.
3. Verify claims about the live Unity project with Unity MCP when the Editor state, scene hierarchy, prefab, animation, material, serialized reference, Inspector value, Console, or Play Mode behavior matters.
4. Treat old notes as hypotheses, not truth. If documentation conflicts with current code or Unity Editor state, the verified code/Editor state wins; correct `PROJECT_STATE.md` as part of the work.

## Scope and approval boundaries

- The user has approved continuing through `ROADMAP.md` milestones without asking for routine confirmation at every milestone. Continue autonomously while preserving all fixed constraints; still stop for destructive actions, new dependencies, or an unresolved choice that would materially change art direction, input strategy, architecture, or game rules.
- Keep changes within the user's stated request. Do not independently change the art style, input system, overall architecture, game rules, or difficulty model.
- Do not delete assets, install or update dependencies, migrate systems, or make broad structural changes without asking first.
- Do not edit Unity scene or prefab YAML directly when the operation can be performed through the Unity Editor.
- Use Unity MCP for modifications to scenes, prefabs, Animator Controllers/clips, Animation Events, materials, render settings, components, or Inspector-serialized values.
- Preserve `.meta` files and GUID-backed references. Move or rename Unity assets through the Editor when practical.
- Do not reserialize or reimport unrelated assets. Keep diffs focused.
- Prefer continuing the existing Codex task so its conversational context remains available. Important constraints and decisions must still be recorded in this file or `PROJECT_STATE.md`, never only in chat.

## Project invariants

- Unity version: `6000.0.72f1`.
- Gameplay scene and only enabled build scene: `Assets/Scenes/SampleScene.unity`.
- Rendering/physics/UI: URP 2D Renderer, 2D physics, uGUI, and TextMesh Pro.
- Runtime scripts live in `Assets/Script/` and currently compile into `Assembly-CSharp`; there are no authored assembly definitions.
- Gameplay currently reads the legacy `UnityEngine.Input` API. `activeInputHandler: 2` enables both input backends. Do not migrate input incidentally.
- Combat is driven by Animator triggers and Animation Events. Event receiver names and Animator parameter casing are serialization contracts.
- Important layers are `Ground` (6), `Enemy` (7), `Player` (8), and `Level_Limits` (9). Verify masks in Unity before changing them.
- `Assets/Prefab/Enemy.prefab` is spawned at runtime by the scene's `Enemy_Respawner`.

## Unity MCP workflow

- Before an Editor-dependent change, read Editor state, project information, the relevant scene/object/component data, and current Console errors.
- Use the smallest appropriate MCP operation. Prefer targeted component/asset edits over broad scene rewrites.
- After a mutation, save through Unity, wait for compilation/import to finish, reread the affected object or asset, and inspect the Console.
- For gameplay-affecting work, enter Play Mode and exercise the affected path. A successful C# compile alone is insufficient.
- If Unity MCP is unavailable, do not guess serialized state or hand-edit YAML as a workaround. Report the limitation or ask the user to start/reconnect the server.

## Code conventions

- Match the current C# style: one `MonoBehaviour` per file, matching class/file names, four-space indentation, and braces on separate lines.
- Prefer `[SerializeField] private`/`protected` fields over public mutable state.
- Keep shared actor behavior in `Entity` and actor-specific behavior in subclasses.
- Cache component references in `Awake()` and use Unity 2D APIs, including `Rigidbody2D.linearVelocity`.
- Guard Inspector references, scene lookups, singleton access, component access, arrays, and destroyed `UnityEngine.Object` references.
- Preserve serialized field names unless migration is deliberate; use `FormerlySerializedAs` where appropriate.
- Do not add `UnityEditor` dependencies to runtime scripts or edit generated solution/project files.

## Verification and documentation

- Check Unity compilation and Console output after relevant changes.
- Verify affected scene/prefab Inspector references and layer masks after serialized changes.
- Run `SampleScene` through the affected behavior for gameplay changes.
- There are currently no authored EditMode or PlayMode tests. Add tests only when they are in scope and place them in a dedicated test assembly.
- Update `PROJECT_STATE.md` after every completed task or milestone. Record what changed, what was verified, remaining risks, and backlog changes.
- Before handoff, report checks performed and any verification that could not be completed.
- Keep milestone history easy to review: target one clearly named completion commit per milestone. Do not rewrite commits that are already pushed; Milestone 1 predates this rule, so finish it with a clearly named milestone-completion commit, then apply the one-commit rule from Milestone 2 onward.
