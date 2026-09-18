public sealed class EnemyDeathState : EnemyState
{
    public EnemyDeathState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.StopMoving();
    }

    public override void Update() { }

    public override void Exit() { }
}
