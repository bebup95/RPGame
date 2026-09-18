using System;
using UnityEngine;

[Serializable]
public struct AttackData
{
    [SerializeField, Min(0)] private int physicalDamage;
    [SerializeField, Min(0)] private int elementalDamage;
    [SerializeField] private DamageElement element;
    [SerializeField, Min(0f)] private float horizontalKnockback;
    [SerializeField, Min(0f)] private float verticalKnockback;
    [SerializeField, Min(0f)] private float stunDuration;

    public DamageContext CreateContext(Entity dealer, Vector2 hitPosition, int swingId)
    {
        float direction = dealer != null ? dealer.FacingDirection : 1f;
        Vector2 knockback = new Vector2(horizontalKnockback * direction, verticalKnockback);
        return new DamageContext(
            dealer,
            physicalDamage,
            elementalDamage,
            element,
            knockback,
            hitPosition,
            stunDuration,
            swingId);
    }
}

public enum DamageElement
{
    None,
    Fire,
    Ice,
    Lightning
}
