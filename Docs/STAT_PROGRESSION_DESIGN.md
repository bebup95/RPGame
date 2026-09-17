# RPGame stat, progression, and first-skill rules

Status: fixed for Milestone 4  
Date: 2026-09-18

This note is the source of truth for the first RPG progression slice. Numbers are intentionally small so they remain readable against the current 1–10 HP prototype scale.

## Runtime ownership

- `CharacterStatProfile` assets contain immutable defaults.
- `CharacterStats` owns one runtime copy per actor. Shared ScriptableObjects are never mutated.
- `PlayerProgression` owns level, XP, skill points, currency, and unlocked skill IDs for the current run.
- `SkillDefinition` assets contain stable IDs, prerequisites, conflicts, cost, cooldown, damage multiplier, and tooltip text.
- Save/load is Milestone 6. Milestone 4 progression resets when the scene reloads.

## Combat formulas

All final damage values use positive half-up rounding (`floor(value + 0.5)`) and never fall below 1 when a positive hit is accepted.

1. Outgoing physical damage: `round((attack base physical + PhysicalPower) × skill multiplier)`.
2. Critical roll: `Random.value < clamp(CriticalChance, 0, 1)`.
3. Critical physical damage: `round(outgoing physical × max(1, CriticalPower))`.
4. Armor mitigation: `round(raw physical × 100 / (100 + max(0, Armor)))`, minimum 1.
5. Elemental mitigation: `round(raw elemental × (1 - clamp(resistance, 0, 0.8)))`, minimum 1 when raw elemental is positive.
6. Electrified vulnerability is applied after mitigation: final damage is multiplied by `1.25`.
7. Health regeneration accumulates continuously and heals one whole HP whenever the accumulator reaches 1; it does not revive dead actors.

Initial profiles:

| Actor | Max HP | Physical power | Armor | Crit chance | Crit power | Regen/s |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Player | 10 | 0 | 1 | 10% | 1.5× | 0.10 |
| Protected character | 10 | 0 | 2 | 0% | 1.5× | 0.05 |
| Forest Guardian | 2 | 0 | 0 | 0% | 1.5× | 0 |
| Swift Forest Guardian | 1 | 0 | 0 | 5% | 1.5× | 0 |

## Elemental statuses

- Chilled: movement multiplier `0.65` for the effect duration.
- Burned: deals the configured potency as direct damage once per second; burn ticks bypass hit invulnerability but still use death events.
- Electrified: accepted damage is multiplied by `1.25` for the effect duration.
- Reapplying the same status refreshes its duration. Statuses do not persist across scene reloads.

The first Player skill is physical only. Status support is implemented as the data/runtime foundation for later skills and content, not silently added to the current attack.

## XP, level, skill points, and currency

- XP required for the next level: `10 + 5 × (level - 1)`.
- XP carries over after leveling and can grant multiple levels in one reward.
- Each level grants exactly 1 skill point.
- Forest Guardian reward: 6 XP and 2 currency.
- Swift Forest Guardian reward: 8 XP and 3 currency.
- Kill count remains independent from XP/currency and still increments once per Enemy death.

## First skill branch

Controls remain on the legacy input path:

- `K`: show/hide the skill-tree panel.
- `1`: unlock Power Strike when one skill point is available.
- `2`: unlock Crushing Force after Power Strike.
- `3`: unlock Quick Recovery after Power Strike.
- `Q`: use Power Strike while grounded and able to attack.

Branch rules:

| Skill | Cost | Prerequisite | Conflict | Effect |
| --- | ---: | --- | --- | --- |
| Power Strike | 1 | None | None | Next grounded attack deals 2× physical damage; 4-second cooldown. |
| Crushing Force | 1 | Power Strike | Quick Recovery | Power Strike multiplier becomes 2.5×. |
| Quick Recovery | 1 | Power Strike | Crushing Force | Power Strike cooldown becomes 2.5 seconds. |

The skill-tree panel must display each skill's tooltip, lock/unlock state, prerequisite/conflict, current skill points, and the keyboard controls. The always-visible cooldown label must show `Locked`, `Ready [Q]`, or remaining seconds.

## Milestone 4 acceptance

- Repeated test inputs produce damage values matching these formulas.
- Enemy rewards can raise the Player from level 1 to level 2 and grant one skill point.
- Power Strike can be unlocked, used on an Enemy, and blocked until its cooldown expires.
- Choosing one branch upgrade prevents the conflicting upgrade.
- HUD level/XP/currency/skill-point values and cooldown text match runtime state.
- Console remains clean after the progression and skill flow.
