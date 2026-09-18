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
    [SerializeField] private StatusEffectType statusEffect;
    [SerializeField, Min(0f)] private float statusDuration;
    [SerializeField, Min(0)] private int statusPotency;

    public DamageContext CreateContext(Entity dealer, Vector2 hitPosition, int swingId, float damageMultiplier)
    {
        bool critical = false;
        CharacterStats stats = dealer != null ? dealer.GetComponent<CharacterStats>() : null;
        int outgoingPhysical = stats != null
            ? stats.CalculateOutgoingPhysical(physicalDamage, damageMultiplier, out critical)
            : Mathf.Max(0, Mathf.FloorToInt(physicalDamage * Mathf.Max(0f, damageMultiplier) + 0.5f));
        float direction = dealer != null ? dealer.FacingDirection : 1f;
        Vector2 knockback = new Vector2(horizontalKnockback * direction, verticalKnockback);
        return new DamageContext(
            dealer,
            outgoingPhysical,
            elementalDamage,
            element,
            critical,
            knockback,
            hitPosition,
            stunDuration,
            statusEffect,
            statusDuration,
            statusPotency,
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
