using UnityEngine;

public sealed class EnemyRetreatState : EnemyState
{
    private float exitTime;

    public EnemyRetreatState(Enemy enemy, EntityStateMachine stateMachine)
        : base(enemy, stateMachine) { }

    public override void Enter()
    {
        exitTime = Time.time + enemy.RetreatDuration;
    }

    public override void Update()
    {
        if (Time.time >= exitTime)
        {
            enemy.ClearTarget();
            stateMachine.ChangeState(enemy.PatrolState);
            return;
        }

        enemy.MoveAwayFromTarget();
    }

    public override void Exit() { }
}
