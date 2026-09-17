using UnityEngine;

public sealed class EnemyIdleState : EnemyState
{
    private float exitTime;

    public EnemyIdleState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.StopMoving();
        exitTime = Time.time + enemy.IdleDuration;
    }

    public override void Update()
    {
        if (enemy.HasTarget)
            stateMachine.ChangeState(enemy.ChaseState);
        else if (Time.time >= exitTime)
            stateMachine.ChangeState(enemy.PatrolState);
    }

    public override void Exit() { }
}
