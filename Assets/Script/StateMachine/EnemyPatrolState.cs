public sealed class EnemyPatrolState : EnemyState
{
    public EnemyPatrolState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter() { }

    public override void Update()
    {
        if (enemy.HasTarget)
        {
            stateMachine.ChangeState(enemy.ChaseState);
            return;
        }

        enemy.MoveInFacingDirection();
    }

    public override void Exit() { }
}
