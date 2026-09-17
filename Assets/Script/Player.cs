using UnityEngine;

public class Player : Entity
{
    [Header("Movement details")]
    [SerializeField] protected float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 12f;
    private float xInput;
    private bool canJump = true;

    private EntityStateMachine stateMachine;

    internal float HorizontalInput => xInput;
    internal float JumpForce => jumpForce;
    internal float VerticalVelocity => rb.linearVelocity.y;
    internal bool IsGrounded => isGrounded;
    internal PlayerIdleState IdleState { get; private set; }
    internal PlayerMoveState MoveState { get; private set; }
    internal PlayerJumpState JumpState { get; private set; }
    internal PlayerFallState FallState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        stateMachine = new EntityStateMachine();
        IdleState = new PlayerIdleState(this, stateMachine);
        MoveState = new PlayerMoveState(this, stateMachine);
        JumpState = new PlayerJumpState(this, stateMachine);
        FallState = new PlayerFallState(this, stateMachine);
        stateMachine.Initialize(IdleState);
    }

    protected override void Update()
    {
        HandleCollision();
        HandleInputs();
        stateMachine.UpdateActiveState();
        HandleAnimations();
        HandleFlip();
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
        float horizontalVelocity = canMove ? xInput * moveSpeed : 0f;
        SetHorizontalVelocity(horizontalVelocity);
    }

    internal void ApplyHorizontalMovement() => HandleMovement();

    internal void SetHorizontalVelocity(float horizontalVelocity)
    {
        rb.linearVelocity = new Vector2(horizontalVelocity, rb.linearVelocity.y);
    }

    internal void SetVerticalVelocity(float verticalVelocity)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalVelocity);
    }

    private void TryToJump()
    {
        if (isGrounded && canJump)
            stateMachine.ChangeState(JumpState);
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
