using UnityEngine;

public sealed class EnemyStunnedState : EnemyState
{
    private float recoveryTime;

    public EnemyStunnedState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        recoveryTime = Time.time + enemy.PendingStunDuration;
    }

    public override void Update()
    {
        if (enemy.IsKnockbackActive)
            return;

        enemy.StopMoving();
        if (Time.time >= recoveryTime)
            stateMachine.ChangeState(enemy.HasTarget ? enemy.ChaseState : enemy.RetreatState);
    }

    public override void Exit() { }
}
