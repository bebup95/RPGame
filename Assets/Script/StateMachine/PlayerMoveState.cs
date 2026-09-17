public sealed class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player, EntityStateMachine stateMachine)
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        player.ApplyHorizontalMovement();
    }

    public override void Update()
    {
        if (player.HorizontalInput == 0f)
        {
            stateMachine.ChangeState(player.IdleState);
            return;
        }

        player.ApplyHorizontalMovement();
    }

    public override void Exit()
    {
    }
}
