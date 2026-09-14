using UnityEngine;

public class Player : Entity
{
    [Header("Movement details")]
    [SerializeField] protected float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    private float xInput;
    private bool canJump = true;

    protected override void Update()
    {
        base.Update();
        HandleInputs();
    }

    private void HandleInputs()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            TryToJump();

        if (Input.GetKeyDown(KeyCode.Mouse0))
            HandleAttack();
    }

    protected override void HandleMovement()
    {
        if (canMove)
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void TryToJump()
    {
        // Lúc này isGrounded đã được nhận diện nhờ đổi thành protected bên Entity
        if (isGrounded && canJump)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public override void EnableMovement(bool value)
    {
        base.EnableMovement(value);
        canJump = value;
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
