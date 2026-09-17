using UnityEngine;

public readonly struct DamageContext
{
    public Entity Dealer { get; }
    public int PhysicalDamage { get; }
    public int ElementalDamage { get; }
    public DamageElement Element { get; }
    public int TotalDamage => PhysicalDamage + ElementalDamage;
    public bool IsCritical { get; }
    public StatusEffectType StatusEffect { get; }
    public float StatusDuration { get; }
    public int StatusPotency { get; }
    public Vector2 Knockback { get; }
    public Vector2 HitPosition { get; }
    public float StunDuration { get; }
    public int SwingId { get; }

    public DamageContext(
        Entity dealer,
        int physicalDamage,
        int elementalDamage,
        DamageElement element,
        bool isCritical,
        Vector2 knockback,
        Vector2 hitPosition,
        float stunDuration,
        StatusEffectType statusEffect,
        float statusDuration,
        int statusPotency,
        int swingId)
    {
        Dealer = dealer;
        PhysicalDamage = Mathf.Max(0, physicalDamage);
        ElementalDamage = Mathf.Max(0, elementalDamage);
        Element = element;
        IsCritical = isCritical;
        Knockback = knockback;
        HitPosition = hitPosition;
        StunDuration = Mathf.Max(0f, stunDuration);
        StatusEffect = statusEffect;
        StatusDuration = Mathf.Max(0f, statusDuration);
        StatusPotency = Mathf.Max(0, statusPotency);
        SwingId = swingId;
    }
}
