using UnityEngine;

public class Enemy : Entity
{
    private bool playerDetected;

    [Header("Movement details")]
    [SerializeField] protected float moveSpeed = 8f;

    protected override void Update()
    {
        base.Update();

        HandleAttack();
    }

    protected override void HandleAttack()
    {
        if (playerDetected)
        {
            anim.SetTrigger("Attack");
        }
    }

    protected override void HandleMovement()
    {
        if (canMove)
            rb.linearVelocity = new Vector2(facingDir * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();
        playerDetected = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsTarget);
    }

    protected override void Die()
    {
        base.Die();
        //Destroy(gameObject, 1f);
        UI ui = UI.Instance;
        if (ui != null)
        {
            ui.AddKillCount();
        }
    }

}
