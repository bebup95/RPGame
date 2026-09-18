using UnityEngine;

[RequireComponent(typeof(InventoryModel), typeof(EquipmentController), typeof(EntityHealth))]
public sealed class PlayerInventoryController : MonoBehaviour
{
    private InventoryModel inventory;
    private EquipmentController equipment;
    private EntityHealth health;

    public InventoryModel Inventory => inventory;
    public EquipmentController Equipment => equipment;

    private void Awake()
    {
        inventory = GetComponent<InventoryModel>();
        equipment = GetComponent<EquipmentController>();
        health = GetComponent<EntityHealth>();
    }

    public bool TryUseOrEquip(int slotIndex)
    {
        ItemDefinition item = inventory.GetDefinitionAt(slotIndex);
        if (item == null)
            return false;

        if (item.Category == ItemCategory.Consumable)
        {
            if (item.HealAmount <= 0 || health.IsDead || health.CurrentHealth >= health.MaxHealth)
                return false;
            if (!inventory.TryRemoveAt(slotIndex, 1))
                return false;
            health.Heal(item.HealAmount);
            return true;
        }

        return item.Category == ItemCategory.Weapon || item.Category == ItemCategory.Armor
            ? equipment.TryEquipFromSlot(slotIndex)
            : false;
    }

    public bool TryUnequipWeapon() => equipment.TryUnequip(EquipmentSlotType.Weapon);

    public bool TryUnequipArmor() => equipment.TryUnequip(EquipmentSlotType.Armor);
}
