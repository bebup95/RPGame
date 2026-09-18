using UnityEngine;

[RequireComponent(typeof(EntityHealth))]
public sealed class WorldHealthBar : MonoBehaviour
{
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private Vector2 size = new Vector2(1.1f, 0.12f);
    [SerializeField] private bool hideWhenFull;

    private static Sprite barSprite;
    private EntityHealth health;
    private GameObject root;
    private Transform fill;

    private void Awake()
    {
        health = GetComponent<EntityHealth>();
        CreateVisuals();
        health.HealthChanged += Refresh;
        Refresh(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.HealthChanged -= Refresh;
        }
    }

    private void Start()
    {
        Refresh(health.CurrentHealth, health.MaxHealth);
    }

    private void LateUpdate()
    {
        if (root != null)
        {
            root.transform.rotation = Quaternion.identity;
        }
    }

    private void CreateVisuals()
    {
        if (barSprite == null)
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.name = "Runtime Health Bar Pixel";
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            barSprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            barSprite.name = "Runtime Health Bar Sprite";
        }

        root = new GameObject("HealthBar_Runtime");
        root.transform.SetParent(transform, false);
        root.transform.localPosition = localOffset;

        CreateLayer("Background", new Color(0.08f, 0.08f, 0.1f, 0.9f), size, 100);
        fill = CreateLayer("Fill", new Color(0.25f, 0.85f, 0.35f, 1f), size, 101).transform;
    }

    private SpriteRenderer CreateLayer(string layerName, Color color, Vector2 layerSize, int order)
    {
        GameObject layer = new GameObject(layerName);
        layer.transform.SetParent(root.transform, false);
        layer.transform.localScale = new Vector3(layerSize.x, layerSize.y, 1f);

        SpriteRenderer renderer = layer.AddComponent<SpriteRenderer>();
        renderer.sprite = barSprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        return renderer;
    }

    private void Refresh(int current, int maximum)
    {
        if (root == null || fill == null)
        {
            return;
        }

        float ratio = maximum > 0 ? Mathf.Clamp01((float)current / maximum) : 0f;
        root.SetActive(!hideWhenFull || ratio < 1f);
        fill.localScale = new Vector3(size.x * ratio, size.y, 1f);
        fill.localPosition = new Vector3(-size.x * (1f - ratio) * 0.5f, 0f, -0.01f);
    }
}
