using System;
using UnityEngine;

[Serializable]
public struct RecipeIngredient
{
    public string itemId;
    [Min(1)] public int quantity;
}

[CreateAssetMenu(menuName = "RPGame/Items/Recipe", fileName = "RecipeDefinition")]
public sealed class RecipeDefinition : ScriptableObject
{
    [SerializeField] private string stableId;
    [SerializeField] private string displayName;
    [SerializeField] private RecipeIngredient[] ingredients;
    [SerializeField] private string outputItemId;
    [SerializeField, Min(1)] private int outputQuantity = 1;

    public string StableId => stableId;
    public string DisplayName => displayName;
    public RecipeIngredient[] Ingredients => ingredients;
    public string OutputItemId => outputItemId;
    public int OutputQuantity => outputQuantity;
}
