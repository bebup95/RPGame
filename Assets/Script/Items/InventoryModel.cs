using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class InventorySlot
{
    [SerializeField] private string itemId;
    [SerializeField] private int quantity;

    public string ItemId => itemId;
    public int Quantity => quantity;
    public bool IsEmpty => string.IsNullOrWhiteSpace(itemId) || quantity <= 0;

    public void Set(string id, int amount)
    {
        itemId = amount > 0 ? id : string.Empty;
        quantity = Mathf.Max(0, amount);
    }
}

public sealed class InventoryModel : MonoBehaviour
{
    [SerializeField] private ItemDatabase database;
    [SerializeField, Min(1)] private int capacity = 12;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();

    public event Action Changed;
    public ItemDatabase Database => database;
    public int Capacity => capacity;
    public IReadOnlyList<InventorySlot> Slots => slots;

    private void Awake() => EnsureCapacity();

    public ItemDefinition GetDefinitionAt(int index)
    {
        EnsureCapacity();
        return index >= 0 && index < slots.Count && !slots[index].IsEmpty
            ? database?.Find(slots[index].ItemId)
            : null;
    }

    public int Count(string itemId)
    {
        EnsureCapacity();
        int total = 0;
        foreach (InventorySlot slot in slots)
            if (!slot.IsEmpty && slot.ItemId == itemId)
                total += slot.Quantity;
        return total;
    }

    public bool CanAdd(string itemId, int quantity)
    {
        ItemDefinition definition = database?.Find(itemId);
        if (definition == null || quantity <= 0)
            return false;

        int remaining = quantity;
        foreach (InventorySlot slot in slots)
        {
            if (!slot.IsEmpty && slot.ItemId == itemId)
                remaining -= Mathf.Max(0, definition.MaxStack - slot.Quantity);
            else if (slot.IsEmpty)
                remaining -= definition.MaxStack;
            if (remaining <= 0)
                return true;
        }
        return false;
    }

    public bool TryAdd(string itemId, int quantity)
    {
        EnsureCapacity();
        ItemDefinition definition = database?.Find(itemId);
        if (definition == null || quantity <= 0 || !CanAdd(itemId, quantity))
            return false;

        int remaining = quantity;
        foreach (InventorySlot slot in slots)
        {
            if (slot.IsEmpty || slot.ItemId != itemId || slot.Quantity >= definition.MaxStack)
                continue;
            int amount = Mathf.Min(remaining, definition.MaxStack - slot.Quantity);
            slot.Set(itemId, slot.Quantity + amount);
            remaining -= amount;
            if (remaining == 0)
                break;
        }

        foreach (InventorySlot slot in slots)
        {
            if (remaining == 0)
                break;
            if (!slot.IsEmpty)
                continue;
            int amount = Mathf.Min(remaining, definition.MaxStack);
            slot.Set(itemId, amount);
            remaining -= amount;
        }

        Changed?.Invoke();
        return true;
    }

    public bool TryRemove(string itemId, int quantity)
    {
        EnsureCapacity();
        if (quantity <= 0 || Count(itemId) < quantity)
            return false;

        int remaining = quantity;
        for (int i = slots.Count - 1; i >= 0 && remaining > 0; i--)
        {
            InventorySlot slot = slots[i];
            if (slot.IsEmpty || slot.ItemId != itemId)
                continue;
            int amount = Mathf.Min(remaining, slot.Quantity);
            slot.Set(itemId, slot.Quantity - amount);
            remaining -= amount;
        }
        Changed?.Invoke();
        return true;
    }

    public bool TryRemoveAt(int index, int quantity)
    {
        EnsureCapacity();
        if (index < 0 || index >= slots.Count || slots[index].IsEmpty || quantity <= 0 || slots[index].Quantity < quantity)
            return false;

        InventorySlot slot = slots[index];
        slot.Set(slot.ItemId, slot.Quantity - quantity);
        Changed?.Invoke();
        return true;
    }

    public int FirstOccupiedIndex()
    {
        EnsureCapacity();
        for (int i = 0; i < slots.Count; i++)
            if (!slots[i].IsEmpty)
                return i;
        return -1;
    }

    private void EnsureCapacity()
    {
        capacity = Mathf.Max(1, capacity);
        slots ??= new List<InventorySlot>();
        while (slots.Count < capacity)
            slots.Add(new InventorySlot());
        if (slots.Count > capacity)
            slots.RemoveRange(capacity, slots.Count - capacity);
    }
}
