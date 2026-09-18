using UnityEngine;

public class Enemy : Entity
{
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    [Header("Configuration")]
    [SerializeField] private EnemyProfile profile;

    [Header("Movement fallback")]
    [SerializeField] protected float moveSpeed = 2f;

    [Header("AI fallback")]
    [SerializeField, Min(0f)] private float targetDetectionRadius = 7f;
    [SerializeField, Min(0f)] private float disengageRadius = 10f;
    [SerializeField, Min(0.05f)] private float targetScanInterval = 0.2f;
    [SerializeField, Min(0f)] private float attackCooldown = 1.25f;
    [SerializeField, Min(0f)] private float idleDuration = 0.35f;
    [SerializeField, Min(0f)] private float retreatDuration = 0.5f;
    [SerializeField, Min(0f)] private float hurtRecoveryDuration = 0.18f;

    private readonly EntityStateMachine stateMachine = new EntityStateMachine();
    private Transform currentTarget;
    private float nextAttackTime;
    private float nextTargetScanTime;

    internal EnemyIdleState IdleState { get; private set; }
    internal EnemyPatrolState PatrolState { get; private set; }
    internal EnemyChaseState ChaseState { get; private set; }
    internal EnemyAttackState AttackState { get; private set; }
    internal EnemyHurtState HurtState { get; private set; }
    internal EnemyStunnedState StunnedState { get; private set; }
    internal EnemyRetreatState RetreatState { get; private set; }
    internal EnemyDeathState DeathState { get; private set; }

    internal bool HasTarget => currentTarget != null;
    internal bool CanAttack => Time.time >= nextAttackTime;
    internal float IdleDuration => idleDuration;
    internal float RetreatDuration => retreatDuration;
    internal float HurtRecoveryDuration => hurtRecoveryDuration;
    internal float PendingStunDuration { get; private set; }

    private float EffectiveMoveSpeed
    {
        get
        {
            float statusMultiplier = statusEffects != null ? statusEffects.MovementMultiplier : 1f;
            float configuredSpeed = profile != null ? profile.MoveSpeed : moveSpeed;
            return configuredSpeed * statusMultiplier;
        }
    }
    private float EffectiveDetectionRadius => profile != null ? profile.DetectionRadius : targetDetectionRadius;
    private float EffectiveDisengageRadius => profile != null ? profile.DisengageRadius : disengageRadius;
    private float EffectiveAttackCooldown => profile != null ? profile.AttackCooldown : attackCooldown;

    protected override void Awake()
    {
        base.Awake();

        if (health == null || combat == null)
        {
            Debug.LogError($"{name} requires EntityHealth and EntityCombat components.", this);
            enabled = false;
            return;
        }

        if (profile != null)
        {
            health.ConfigureMaxHealth(profile.MaxHealth, true);
            sr.color *= profile.Tint;
            gameObject.name = profile.DisplayName;
        }

        IdleState = new EnemyIdleState(this, stateMachine);
        PatrolState = new EnemyPatrolState(this, stateMachine);
        ChaseState = new EnemyChaseState(this, stateMachine);
        AttackState = new EnemyAttackState(this, stateMachine);
        HurtState = new EnemyHurtState(this, stateMachine);
        StunnedState = new EnemyStunnedState(this, stateMachine);
        RetreatState = new EnemyRetreatState(this, stateMachine);
        DeathState = new EnemyDeathState(this, stateMachine);
        stateMachine.Initialize(IdleState);
    }

    protected override void Update()
    {
        HandleCollision();

        if (!health.IsDead)
        {
            RefreshTarget();
        }

        stateMachine.UpdateActiveState();
        HandleAnimations();
        HandleFlip();
    }

    private void RefreshTarget()
    {
        if (currentTarget != null)
        {
            EntityHealth targetHealth = currentTarget.GetComponent<EntityHealth>();
            if (targetHealth != null && targetHealth.IsDead)
            {
                currentTarget = null;
            }
        }

        if (currentTarget != null)
        {
            float distance = Vector2.Distance(transform.position, currentTarget.position);
            if (distance <= EffectiveDisengageRadius)
            {
                return;
            }

            currentTarget = null;
        }

        if (Time.time < nextTargetScanTime)
        {
            return;
        }

        nextTargetScanTime = Time.time + targetScanInterval;

        Collider2D[] candidates = Physics2D.OverlapCircleAll(
            transform.position,
            EffectiveDetectionRadius,
            combat.TargetMask);

        float closestDistance = float.MaxValue;
        Transform closestTarget = null;

        foreach (Collider2D candidate in candidates)
        {
            Entity targetEntity = candidate.GetComponentInParent<Entity>();
            if (targetEntity == null || targetEntity == this)
            {
                continue;
            }

            float distance = ((Vector2)candidate.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = targetEntity.transform;
            }
        }

        currentTarget = closestTarget;
    }

    internal bool IsTargetInAttackRange()
    {
        if (currentTarget == null || combat.AttackPoint == null)
        {
            return false;
        }

        Collider2D targetCollider = currentTarget.GetComponent<Collider2D>();
        if (targetCollider == null || !targetCollider.enabled)
        {
            return false;
        }

        Vector2 closestPoint = targetCollider.ClosestPoint(combat.AttackPoint.position);
        float distance = Vector2.Distance(combat.AttackPoint.position, closestPoint);
        return distance <= combat.AttackRadius;
    }

    internal void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    internal void MoveInFacingDirection()
    {
        if (IsKnockbackActive)
        {
            return;
        }

        rb.linearVelocity = new Vector2(facingDir * EffectiveMoveSpeed, rb.linearVelocity.y);
    }

    internal void MoveTowardTarget()
    {
        if (currentTarget == null || IsKnockbackActive)
        {
            StopMoving();
            return;
        }

        FaceTarget();
        rb.linearVelocity = new Vector2(facingDir * EffectiveMoveSpeed, rb.linearVelocity.y);
    }

    internal void MoveAwayFromTarget()
    {
        if (currentTarget == null || IsKnockbackActive)
        {
            MoveInFacingDirection();
            return;
        }

        float directionAway = Mathf.Sign(transform.position.x - currentTarget.position.x);
        if (directionAway == 0f)
        {
            directionAway = facingDir;
        }

        FaceDirection((int)directionAway);
        rb.linearVelocity = new Vector2(facingDir * EffectiveMoveSpeed, rb.linearVelocity.y);
    }

    internal void BeginAttack()
    {
        StopMoving();
        FaceTarget();
        base.EnableMovement(false);
        BeginAttackSwing();
        nextAttackTime = Time.time + EffectiveAttackCooldown;
        anim.ResetTrigger(AttackHash);
        anim.SetTrigger(AttackHash);
    }

    internal void FinishAttack()
    {
        EndAttackSwing();
        base.EnableMovement(true);
        stateMachine.ChangeState(HasTarget ? ChaseState : RetreatState);
    }

    internal void ClearTarget()
    {
        currentTarget = null;
    }

    public override void EnableMovement(bool enable)
    {
        base.EnableMovement(enable);

        if (enable && ReferenceEquals(stateMachine.CurrentState, AttackState))
        {
            FinishAttack();
        }
    }

    protected override void OnDamaged(DamageContext context)
    {
        if (context.Dealer != null)
        {
            currentTarget = context.Dealer.transform;
        }

        PendingStunDuration = context.StunDuration;
        stateMachine.ChangeState(context.StunDuration > 0f ? StunnedState : HurtState);
    }

    protected override void HandleMovement() { }

    protected override void HandleAttack() { }

    protected override void Die()
    {
        GetComponent<EnemyLootDrop>()?.Drop();

        if (stateMachine.IsInitialized)
        {
            stateMachine.ChangeState(DeathState);
        }

        base.Die();
        UI ui = UI.Instance;
        if (ui != null)
        {
            ui.AddKillCount();
        }

        PlayerProgression progression = FindFirstObjectByType<PlayerProgression>();
        if (progression != null && profile != null)
        {
            progression.AddRewards(profile.ExperienceReward, profile.CurrencyReward);
        }
    }

    private void FaceTarget()
    {
        if (currentTarget != null)
        {
            FaceDirection(currentTarget.position.x >= transform.position.x ? 1 : -1);
        }
    }

    private void FaceDirection(int direction)
    {
        if (direction != 0 && direction != facingDir)
        {
            Flip();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, EffectiveDetectionRadius);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, EffectiveDisengageRadius);
    }
}
