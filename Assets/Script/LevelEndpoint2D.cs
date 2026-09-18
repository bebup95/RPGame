using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class LevelEndpoint2D : MonoBehaviour
{
    [SerializeField] private Color reachedColor = new Color(0.4f, 1f, 0.4f, 1f);

    private SpriteRenderer endpointRenderer;

    public bool HasBeenReached { get; private set; }

    private void Awake()
    {
        endpointRenderer = GetComponent<SpriteRenderer>();
    }

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (HasBeenReached || other.GetComponent<Player>() == null)
            return;

        HasBeenReached = true;

        if (endpointRenderer != null)
            endpointRenderer.color = reachedColor;
    }
}
