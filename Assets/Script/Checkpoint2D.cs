using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class Checkpoint2D : MonoBehaviour
{
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 1f);
    [SerializeField] private Color activatedColor = new Color(0.35f, 1f, 0.65f, 1f);

    private SpriteRenderer checkpointRenderer;
    private bool isActivated;

    public bool IsActivated => isActivated;

    private void Awake()
    {
        checkpointRenderer = GetComponent<SpriteRenderer>();
    }

    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRespawnController respawnController = other.GetComponent<PlayerRespawnController>();
        if (respawnController == null)
            return;

        respawnController.SetCheckpoint((Vector2)transform.position + respawnOffset);
        isActivated = true;

        if (checkpointRenderer != null)
            checkpointRenderer.color = activatedColor;
    }
}
