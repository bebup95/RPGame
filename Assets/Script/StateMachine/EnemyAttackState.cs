using UnityEngine;

public sealed class EnemyAttackState : EnemyState
{
    private const float AttackFailsafeDuration = 1.5f;
    private float failsafeTime;

    public EnemyAttackState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.BeginAttack();
        failsafeTime = Time.time + AttackFailsafeDuration;
    }

    public override void Update()
    {
        enemy.StopMoving();
        if (Time.time >= failsafeTime)
            enemy.FinishAttack();
    }

    public override void Exit() { }
}
