public sealed class PlayerIdleState : PlayerState
{
    public PlayerIdleState(Player player, EntityStateMachine stateMachine)
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        player.SetHorizontalVelocity(0f);
    }

    public override void Update()
    {
        if (!player.IsGrounded)
        {
            stateMachine.ChangeState(player.FallState);
            return;
        }

        if (player.HorizontalInput != 0f)
        {
            stateMachine.ChangeState(player.MoveState);
        }
    }

    public override void Exit()
    {
    }
}
