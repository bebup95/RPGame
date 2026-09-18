public sealed class PlayerJumpState : PlayerState
{
    public PlayerJumpState(Player player, EntityStateMachine stateMachine)
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        player.SetVerticalVelocity(player.JumpForce);
    }

    public override void Update()
    {
        player.ApplyHorizontalMovement();

        if (player.VerticalVelocity <= 0f)
        {
            stateMachine.ChangeState(player.FallState);
        }
    }

    public override void Exit()
    {
    }
}
