# Item, inventory, and economy design

Milestone 5 deliberately implements one small end-to-end RPG loop without adding a package or changing the project's input architecture.

## Stable data

All definitions are immutable ScriptableObjects resolved through `ItemDatabase` by stable string ID. Runtime quantities live only in `InventoryModel` slots.

| Stable ID | Category | Stack | Function |
| --- | --- | ---: | --- |
| `health_potion` | Consumable | 10 | Restores 5 health |
| `iron_sword` | Weapon | 1 | Adds 2 Physical Power; merchant price 4 Gold |
| `leather_armor` | Armor | 1 | Adds 2 Max Health and 15 Armor |
| `forest_herb` | Material | 20 | Ingredient dropped by Forest Enemy |
| `gold_token` | Currency | 99 | Item-category contract; current-run Gold remains in `PlayerProgression` |

The first recipe is `brew_health_potion`: two `forest_herb` produce one `health_potion`. Swift Forest Enemy drops a potion, while the base Forest Enemy drops one herb. Loot is guaranteed in this initial slice so the loop is testable and balancing is deterministic.

## Runtime rules

- Player inventory has 12 slots; storage has 8.
- Add operations fill existing matching stacks from the lowest slot index, then empty slots from the lowest index.
- Add, remove, spend, craft, equipment replacement, and storage transfer reject invalid or insufficient operations without knowingly losing data or currency.
- Equipment is stored by stable ID in one Weapon and one Armor slot. Runtime stat modifiers are keyed by equipment slot and never mutate a shared stat profile.
- Consumables are removed only when their effect can be applied. A health potion is not consumed at full health.
- Replacing or unequipping gear returns the old item to inventory only when capacity permits.

## Interaction contract

- `I`: open or close inventory.
- Up/Down: deterministic inventory selection.
- Enter: use or equip selected item.
- `B`: buy the configured merchant item.
- `C`: craft the configured recipe.
- `T`: transfer one selected item to storage.
- `R`: retrieve the first occupied storage slot.
- `U` / `O`: unequip weapon / armor.
- The inventory panel exposes click buttons for selection, use/equip, buy, craft, store, and retrieve.

This milestone remains current-run only. Serialization of inventory, equipment, currency, recipes, and checkpoint state belongs to Milestone 6.
