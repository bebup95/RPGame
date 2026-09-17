using UnityEngine;

[RequireComponent(typeof(Player), typeof(Rigidbody2D))]
public sealed class PlayerRespawnController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 respawnPosition;

    public Vector2 RespawnPosition => respawnPosition;

    private void Awake()
    {
        if (!TryGetComponent(out rb))
        {
            Debug.LogError($"{nameof(PlayerRespawnController)} requires a Rigidbody2D.", this);
            enabled = false;
            return;
        }

        respawnPosition = rb.position;
    }

    public void SetCheckpoint(Vector2 checkpointPosition)
    {
        respawnPosition = checkpointPosition;
    }

    public void Respawn()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.position = respawnPosition;
        Physics2D.SyncTransforms();
    }
}
