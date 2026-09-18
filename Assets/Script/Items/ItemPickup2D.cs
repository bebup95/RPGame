using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D))]
public sealed class ItemPickup2D : MonoBehaviour
{
    [SerializeField] private ItemDefinition item;
    [SerializeField, Min(1)] private int quantity = 1;
    private static Sprite runtimeSprite;
    private bool collected;

    public ItemDefinition Item => item;
    public int Quantity => quantity;

    public static ItemPickup2D Create(ItemDefinition definition, int amount, Vector3 position)
    {
        GameObject pickupObject = new GameObject($"Pickup_{definition.StableId}");
        pickupObject.transform.position = position;
        pickupObject.transform.localScale = Vector3.one * 0.32f;
        pickupObject.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
        SpriteRenderer renderer = pickupObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetRuntimeSprite();
        renderer.color = GetCategoryColor(definition.Category);
        renderer.sortingOrder = 20;
        CircleCollider2D collider = pickupObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.75f;
        Rigidbody2D body = pickupObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;
        ItemPickup2D pickup = pickupObject.AddComponent<ItemPickup2D>();
        pickup.item = definition;
        pickup.quantity = Mathf.Max(1, amount);
        return pickup;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider2D other)
    {
        if (collected)
            return;

        InventoryModel inventory = other.GetComponentInParent<InventoryModel>();
        if (inventory != null && item != null && inventory.TryAdd(item.StableId, quantity))
        {
            collected = true;
            Destroy(gameObject);
        }
    }

    private static Sprite GetRuntimeSprite()
    {
        if (runtimeSprite != null)
            return runtimeSprite;
        Texture2D texture = new Texture2D(1, 1);
        texture.name = "RuntimePickupTexture";
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        runtimeSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        runtimeSprite.name = "RuntimePickupSprite";
        return runtimeSprite;
    }

    private static Color GetCategoryColor(ItemCategory category)
    {
        return category switch
        {
            ItemCategory.Consumable => new Color(0.85f, 0.2f, 0.25f),
            ItemCategory.Weapon => new Color(0.75f, 0.8f, 0.9f),
            ItemCategory.Armor => new Color(0.25f, 0.55f, 0.9f),
            ItemCategory.Material => new Color(0.2f, 0.8f, 0.35f),
            _ => new Color(1f, 0.75f, 0.1f)
        };
    }
}
