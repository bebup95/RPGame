using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "RPGame/Items/Item Database", fileName = "ItemDatabase")]
public sealed class ItemDatabase : ScriptableObject
{
    [SerializeField] private ItemDefinition[] items;
    private Dictionary<string, ItemDefinition> lookup;

    public ItemDefinition Find(string stableId)
    {
        EnsureLookup();
        return !string.IsNullOrWhiteSpace(stableId) && lookup.TryGetValue(stableId, out ItemDefinition item)
            ? item
            : null;
    }

    public bool HasUniqueStableIds(out string duplicateId)
    {
        duplicateId = null;
        HashSet<string> ids = new HashSet<string>();
        foreach (ItemDefinition item in items ?? System.Array.Empty<ItemDefinition>())
        {
            if (item == null || string.IsNullOrWhiteSpace(item.StableId) || !ids.Add(item.StableId))
            {
                duplicateId = item != null ? item.StableId : "<null>";
                return false;
            }
        }
        return true;
    }

    private void OnEnable() => lookup = null;

    private void EnsureLookup()
    {
        if (lookup != null)
            return;

        lookup = new Dictionary<string, ItemDefinition>();
        foreach (ItemDefinition item in items ?? System.Array.Empty<ItemDefinition>())
        {
            if (item != null && !string.IsNullOrWhiteSpace(item.StableId))
                lookup[item.StableId] = item;
        }
    }
}
