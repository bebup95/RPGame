public sealed class PlayerFallState : PlayerState
{
    public PlayerFallState(Player player, EntityStateMachine stateMachine)
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
    }

    public override void Update()
    {
        player.ApplyHorizontalMovement();

        if (!player.IsGrounded || player.VerticalVelocity > 0f)
        {
            return;
        }

        EntityState groundedState = player.HorizontalInput == 0f
            ? player.IdleState
            : player.MoveState;
        stateMachine.ChangeState(groundedState);
    }

    public override void Exit()
    {
    }
}
