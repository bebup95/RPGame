public abstract class EnemyState : EntityState
{
    protected readonly Enemy enemy;

    protected EnemyState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine)
    {
        this.enemy = enemy;
    }
}
