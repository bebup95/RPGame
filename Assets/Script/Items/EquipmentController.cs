using System;
using UnityEngine;

[RequireComponent(typeof(InventoryModel), typeof(CharacterStats), typeof(EntityHealth))]
public sealed class EquipmentController : MonoBehaviour
{
    [SerializeField] private string equippedWeaponId;
    [SerializeField] private string equippedArmorId;

    private InventoryModel inventory;
    private CharacterStats stats;

    public event Action EquipmentChanged;
    public string EquippedWeaponId => equippedWeaponId;
    public string EquippedArmorId => equippedArmorId;

    private void Awake()
    {
        inventory = GetComponent<InventoryModel>();
        stats = GetComponent<CharacterStats>();
        ApplySlot(EquipmentSlotType.Weapon, equippedWeaponId);
        ApplySlot(EquipmentSlotType.Armor, equippedArmorId);
    }

    public bool TryEquipFromSlot(int inventoryIndex)
    {
        ItemDefinition item = inventory.GetDefinitionAt(inventoryIndex);
        if (item == null || item.EquipmentSlot == EquipmentSlotType.None)
            return false;

        string previousId = GetEquippedId(item.EquipmentSlot);
        if (!inventory.TryRemoveAt(inventoryIndex, 1))
            return false;

        if (!string.IsNullOrWhiteSpace(previousId) &&
            (!inventory.CanAdd(previousId, 1) || !inventory.TryAdd(previousId, 1)))
        {
            inventory.TryAdd(item.StableId, 1);
            return false;
        }

        SetEquippedId(item.EquipmentSlot, item.StableId);
        ApplySlot(item.EquipmentSlot, item.StableId);
        EquipmentChanged?.Invoke();
        return true;
    }

    public bool TryUnequip(EquipmentSlotType slot)
    {
        if (slot == EquipmentSlotType.None)
            return false;

        string itemId = GetEquippedId(slot);
        if (string.IsNullOrWhiteSpace(itemId) || !inventory.CanAdd(itemId, 1) || !inventory.TryAdd(itemId, 1))
            return false;

        SetEquippedId(slot, string.Empty);
        ApplySlot(slot, string.Empty);
        EquipmentChanged?.Invoke();
        return true;
    }

    private string GetEquippedId(EquipmentSlotType slot) =>
        slot == EquipmentSlotType.Weapon ? equippedWeaponId : equippedArmorId;

    private void SetEquippedId(EquipmentSlotType slot, string itemId)
    {
        if (slot == EquipmentSlotType.Weapon)
            equippedWeaponId = itemId;
        else if (slot == EquipmentSlotType.Armor)
            equippedArmorId = itemId;
    }

    private void ApplySlot(EquipmentSlotType slot, string itemId)
    {
        string source = "equipment:" + slot;
        ItemDefinition item = inventory.Database?.Find(itemId);
        if (item == null)
            stats.RemoveModifier(source);
        else
            stats.SetModifier(source, item.StatModifier);
    }
}
