using UnityEngine;

public sealed class EnemyHurtState : EnemyState
{
    private float recoveryTime;

    public EnemyHurtState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        recoveryTime = Time.time + enemy.HurtRecoveryDuration;
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
