using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class RespawnTrigger2D : MonoBehaviour
{
    private void Reset()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerRespawnController respawnController = other.GetComponent<PlayerRespawnController>();
        if (respawnController != null)
            respawnController.Respawn();
    }
}
