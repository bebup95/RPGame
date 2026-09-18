using UnityEngine;

public enum ItemCategory
{
    Consumable,
    Weapon,
    Armor,
    Material,
    Currency
}

public enum EquipmentSlotType
{
    None,
    Weapon,
    Armor
}

[CreateAssetMenu(menuName = "RPGame/Items/Item Definition", fileName = "ItemDefinition")]
public sealed class ItemDefinition : ScriptableObject
{
    [SerializeField] private string stableId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private ItemCategory category;
    [SerializeField, Min(1)] private int maxStack = 1;
    [SerializeField, Min(0)] private int buyPrice;
    [SerializeField, Min(0)] private int healAmount;
    [SerializeField] private EquipmentSlotType equipmentSlot;
    [SerializeField] private StatModifier statModifier;

    public string StableId => stableId;
    public string DisplayName => displayName;
    public string Description => description;
    public ItemCategory Category => category;
    public int MaxStack => Mathf.Max(1, maxStack);
    public int BuyPrice => buyPrice;
    public int HealAmount => healAmount;
    public EquipmentSlotType EquipmentSlot => equipmentSlot;
    public StatModifier StatModifier => statModifier;
}
