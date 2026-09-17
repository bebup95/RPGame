public abstract class PlayerState : EntityState
{
    protected readonly Player player;

    protected PlayerState(Player player, EntityStateMachine stateMachine)
        : base(player, stateMachine)
    {
        this.player = player;
    }
}
