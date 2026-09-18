using System;
using UnityEngine;

public sealed class EconomyServices : MonoBehaviour
{
    [SerializeField] private InventoryModel playerInventory;
    [SerializeField] private PlayerProgression progression;
    [SerializeField] private InventoryModel storageInventory;
    [SerializeField] private ItemDefinition merchantItem;
    [SerializeField] private RecipeDefinition recipe;

    public event Action<string> StatusChanged;
    public InventoryModel StorageInventory => storageInventory;
    public ItemDefinition MerchantItem => merchantItem;
    public RecipeDefinition Recipe => recipe;

    public bool TryBuyMerchantItem()
    {
        if (merchantItem == null || !playerInventory.CanAdd(merchantItem.StableId, 1))
            return Report(false, "Inventory full.");
        if (!progression.TrySpendCurrency(merchantItem.BuyPrice))
            return Report(false, $"Need {merchantItem.BuyPrice} Gold.");
        if (!playerInventory.TryAdd(merchantItem.StableId, 1))
        {
            progression.AddCurrency(merchantItem.BuyPrice);
            return Report(false, "Purchase could not be completed.");
        }
        return Report(true, $"Bought {merchantItem.DisplayName}.");
    }

    public bool TryCraftRecipe()
    {
        if (recipe == null)
            return Report(false, "No recipe configured.");
        foreach (RecipeIngredient ingredient in recipe.Ingredients)
            if (playerInventory.Count(ingredient.itemId) < ingredient.quantity)
                return Report(false, "Missing crafting materials.");

        foreach (RecipeIngredient ingredient in recipe.Ingredients)
            playerInventory.TryRemove(ingredient.itemId, ingredient.quantity);

        if (!playerInventory.TryAdd(recipe.OutputItemId, recipe.OutputQuantity))
        {
            foreach (RecipeIngredient ingredient in recipe.Ingredients)
                playerInventory.TryAdd(ingredient.itemId, ingredient.quantity);
            return Report(false, "Inventory full.");
        }
        return Report(true, $"Crafted {recipe.DisplayName}.");
    }

    public bool TryStore(int playerSlotIndex)
    {
        InventorySlot slot = playerSlotIndex >= 0 && playerSlotIndex < playerInventory.Slots.Count
            ? playerInventory.Slots[playerSlotIndex]
            : null;
        if (slot == null || slot.IsEmpty || !storageInventory.CanAdd(slot.ItemId, 1))
            return Report(false, "Select an item or free storage space.");

        string itemId = slot.ItemId;
        if (!playerInventory.TryRemoveAt(playerSlotIndex, 1))
            return Report(false, "Storage transfer failed.");
        if (!storageInventory.TryAdd(itemId, 1))
        {
            playerInventory.TryAdd(itemId, 1);
            return Report(false, "Storage transfer failed.");
        }
        return Report(true, $"Stored {playerInventory.Database.Find(itemId).DisplayName}.");
    }

    public bool TryRetrieveFirst()
    {
        int index = storageInventory.FirstOccupiedIndex();
        if (index < 0)
            return Report(false, "Storage is empty.");
        InventorySlot slot = storageInventory.Slots[index];
        string itemId = slot.ItemId;
        if (!playerInventory.CanAdd(itemId, 1))
            return Report(false, "Inventory full.");
        if (!storageInventory.TryRemoveAt(index, 1))
            return Report(false, "Storage transfer failed.");
        if (!playerInventory.TryAdd(itemId, 1))
        {
            storageInventory.TryAdd(itemId, 1);
            return Report(false, "Storage transfer failed.");
        }
        return Report(true, $"Retrieved {playerInventory.Database.Find(itemId).DisplayName}.");
    }

    private bool Report(bool result, string message)
    {
        StatusChanged?.Invoke(message);
        return result;
    }
}
