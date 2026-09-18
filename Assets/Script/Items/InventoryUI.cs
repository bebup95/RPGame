using System.Text;
using TMPro;
using UnityEngine;

public sealed class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private TextMeshProUGUI equipmentText;
    [SerializeField] private TextMeshProUGUI storageText;
    [SerializeField] private TextMeshProUGUI statusText;

    private PlayerInventoryController playerInventory;
    private EconomyServices economy;
    private int selectedIndex;

    private void Start()
    {
        playerInventory = FindFirstObjectByType<PlayerInventoryController>();
        economy = FindFirstObjectByType<EconomyServices>();
        if (playerInventory == null || economy == null)
        {
            Debug.LogError("Inventory UI requires PlayerInventoryController and EconomyServices.", this);
            enabled = false;
            return;
        }

        playerInventory.Inventory.Changed += Refresh;
        playerInventory.Equipment.EquipmentChanged += Refresh;
        economy.StorageInventory.Changed += Refresh;
        economy.StatusChanged += SetStatus;
        inventoryPanel.SetActive(false);
        Refresh();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.Inventory.Changed -= Refresh;
            playerInventory.Equipment.EquipmentChanged -= Refresh;
        }
        if (economy != null)
        {
            economy.StorageInventory.Changed -= Refresh;
            economy.StatusChanged -= SetStatus;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
            Refresh();
        }
        if (!inventoryPanel.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.UpArrow)) SelectPrevious();
        if (Input.GetKeyDown(KeyCode.DownArrow)) SelectNext();
        if (Input.GetKeyDown(KeyCode.Return)) UseSelected();
        if (Input.GetKeyDown(KeyCode.B)) BuyItem();
        if (Input.GetKeyDown(KeyCode.C)) CraftItem();
        if (Input.GetKeyDown(KeyCode.T)) StoreSelected();
        if (Input.GetKeyDown(KeyCode.R)) RetrieveItem();
        if (Input.GetKeyDown(KeyCode.U)) UnequipWeapon();
        if (Input.GetKeyDown(KeyCode.O)) UnequipArmor();
    }

    public void SelectPrevious()
    {
        selectedIndex = (selectedIndex - 1 + playerInventory.Inventory.Capacity) % playerInventory.Inventory.Capacity;
        Refresh();
    }

    public void SelectNext()
    {
        selectedIndex = (selectedIndex + 1) % playerInventory.Inventory.Capacity;
        Refresh();
    }

    public void UseSelected()
    {
        SetStatus(playerInventory.TryUseOrEquip(selectedIndex) ? "Item used/equipped." : "This item cannot be used now.");
        Refresh();
    }

    public void BuyItem() { economy.TryBuyMerchantItem(); Refresh(); }
    public void CraftItem() { economy.TryCraftRecipe(); Refresh(); }
    public void StoreSelected() { economy.TryStore(selectedIndex); Refresh(); }
    public void RetrieveItem() { economy.TryRetrieveFirst(); Refresh(); }
    public void UnequipWeapon() { SetStatus(playerInventory.TryUnequipWeapon() ? "Weapon unequipped." : "No room or no weapon equipped."); Refresh(); }
    public void UnequipArmor() { SetStatus(playerInventory.TryUnequipArmor() ? "Armor unequipped." : "No room or no armor equipped."); Refresh(); }

    private void Refresh()
    {
        if (playerInventory == null)
            return;

        StringBuilder list = new StringBuilder("INVENTORY  [I close]\n");
        for (int i = 0; i < playerInventory.Inventory.Slots.Count; i++)
        {
            InventorySlot slot = playerInventory.Inventory.Slots[i];
            ItemDefinition definition = playerInventory.Inventory.GetDefinitionAt(i);
            list.Append(i == selectedIndex ? "> " : "  ");
            list.Append(i + 1).Append(". ");
            list.AppendLine(definition == null ? "Empty" : $"{definition.DisplayName} x{slot.Quantity}");
        }
        inventoryText.text = list.ToString();

        ItemDefinition selected = playerInventory.Inventory.GetDefinitionAt(selectedIndex);
        tooltipText.text = selected == null
            ? "Select an item."
            : $"{selected.DisplayName} [{selected.Category}]\n{selected.Description}\nStack {selected.MaxStack} | Price {selected.BuyPrice}";

        CharacterStats stats = playerInventory.GetComponent<CharacterStats>();
        equipmentText.text =
            $"EQUIPMENT\nWeapon: {NameOf(playerInventory.Equipment.EquippedWeaponId)}\n" +
            $"Armor: {NameOf(playerInventory.Equipment.EquippedArmorId)}\n" +
            $"HP {stats.MaxHealth} | Power {stats.PhysicalPower} | Armor {stats.Armor}\n" +
            "Unequip: [U] weapon / [O] armor";

        StringBuilder storage = new StringBuilder("STORAGE\n");
        foreach (InventorySlot slot in economy.StorageInventory.Slots)
            if (!slot.IsEmpty)
                storage.AppendLine($"{NameOf(slot.ItemId)} x{slot.Quantity}");
        storageText.text = storage.ToString();
    }

    private string NameOf(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return "None";
        ItemDefinition item = playerInventory.Inventory.Database.Find(itemId);
        return item != null ? item.DisplayName : itemId;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }
}
