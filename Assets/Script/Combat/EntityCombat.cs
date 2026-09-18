using System.Collections.Generic;
using UnityEngine;

public sealed class EntityCombat : MonoBehaviour
{
    [SerializeField] private Transform attackPoint;
    [SerializeField, Min(0f)] private float attackRadius = 1f;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private AttackData attackData;

    private readonly HashSet<int> hitTargets = new HashSet<int>();
    private Entity owner;
    private int swingId;
    private bool swingActive;

    public float AttackRadius => attackRadius;
    public LayerMask TargetMask => targetMask;
    public Transform AttackPoint => attackPoint;

    private void Awake()
    {
        owner = GetComponent<Entity>();
    }

    public void BeginSwing()
    {
        swingId++;
        hitTargets.Clear();
        swingActive = true;
    }

    public int DamageTargets()
    {
        if (owner == null || attackPoint == null || attackRadius <= 0f)
        {
            return 0;
        }

        if (!swingActive)
        {
            BeginSwing();
        }

        int hitCount = 0;
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            targetMask);

        foreach (Collider2D targetCollider in colliders)
        {
            IDamageable damageable = targetCollider.GetComponentInParent<IDamageable>();
            Component targetComponent = damageable as Component;
            if (damageable == null || targetComponent == null || targetComponent.gameObject == gameObject)
            {
                continue;
            }

            int targetId = targetComponent.GetInstanceID();
            if (!hitTargets.Add(targetId))
            {
                continue;
            }

            Vector2 hitPosition = targetCollider.ClosestPoint(attackPoint.position);
            DamageContext context = attackData.CreateContext(owner, hitPosition, swingId);
            if (damageable.ReceiveDamage(context))
            {
                hitCount++;
            }
        }

        return hitCount;
    }

    public void EndSwing()
    {
        swingActive = false;
        hitTargets.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
