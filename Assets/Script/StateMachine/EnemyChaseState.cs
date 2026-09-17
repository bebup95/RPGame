public sealed class EnemyChaseState : EnemyState
{
    public EnemyChaseState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter() { }

    public override void Update()
    {
        if (!enemy.HasTarget)
        {
            stateMachine.ChangeState(enemy.RetreatState);
            return;
        }

        if (enemy.IsTargetInAttackRange())
        {
            enemy.StopMoving();
            if (enemy.CanAttack)
                stateMachine.ChangeState(enemy.AttackState);
            return;
        }

        enemy.MoveTowardTarget();
    }

    public override void Exit() { }
}
