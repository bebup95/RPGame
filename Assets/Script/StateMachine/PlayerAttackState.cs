public sealed class PlayerAttackState : PlayerState
{
    public PlayerAttackState(Player player, EntityStateMachine stateMachine)
        : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        player.TriggerAttackAnimation();
    }

    public override void Update()
    {
        player.ApplyHorizontalMovement();
    }

    public override void Exit()
    {
    }
}
