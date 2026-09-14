using UnityEngine;

public class ObjectToProtect : Entity
{
    private Transform player;

    protected override void Awake()
    {
        base.Awake();
        Player playerObject = FindFirstObjectByType<Player>();
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    protected override void Update()
    {
        HandleFlip();

    }

    protected override void HandleFlip()
    {

        if (player == null)
            return;

        if (player.transform.position.x > transform.position.x && facingRight == false)
        {
            Flip();
        }
        else if (player.transform.position.x < transform.position.x && facingRight == true)
        {
            Flip();
        }
    }

    protected override void Die()
    {
        base.Die();
        UI ui = UI.Instance;
        if (ui != null)
        {
            ui.EnableGameOverUI();
        }

    }
}
