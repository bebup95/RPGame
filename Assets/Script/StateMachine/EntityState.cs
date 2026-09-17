using System;

public abstract class EntityState
{
    protected readonly Entity entity;
    protected readonly EntityStateMachine stateMachine;

    protected EntityState(Entity entity, EntityStateMachine stateMachine)
    {
        this.entity = entity != null
            ? entity
            : throw new ArgumentNullException(nameof(entity));
        this.stateMachine = stateMachine
            ?? throw new ArgumentNullException(nameof(stateMachine));
    }

    public abstract void Enter();

    public abstract void Update();

    public abstract void Exit();
}
