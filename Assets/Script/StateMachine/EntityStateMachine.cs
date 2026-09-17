using System;

public sealed class EntityStateMachine
{
    public EntityState CurrentState { get; private set; }

    public bool IsInitialized => CurrentState != null;

    public void Initialize(EntityState startingState)
    {
        if (startingState == null)
        {
            throw new ArgumentNullException(nameof(startingState));
        }

        if (IsInitialized)
        {
            throw new InvalidOperationException("The state machine is already initialized.");
        }

        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(EntityState newState)
    {
        if (newState == null)
        {
            throw new ArgumentNullException(nameof(newState));
        }

        if (ReferenceEquals(CurrentState, newState))
        {
            return;
        }

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void UpdateActiveState()
    {
        CurrentState?.Update();
    }
}
