using UnityEngine;

public readonly struct DamageContext
{
    public Entity Dealer { get; }
    public int PhysicalDamage { get; }
    public int ElementalDamage { get; }
    public DamageElement Element { get; }
    public int TotalDamage => PhysicalDamage + ElementalDamage;
    public Vector2 Knockback { get; }
    public Vector2 HitPosition { get; }
    public float StunDuration { get; }
    public int SwingId { get; }

    public DamageContext(
        Entity dealer,
        int physicalDamage,
        int elementalDamage,
        DamageElement element,
        Vector2 knockback,
        Vector2 hitPosition,
        float stunDuration,
        int swingId)
    {
        Dealer = dealer;
        PhysicalDamage = Mathf.Max(0, physicalDamage);
        ElementalDamage = Mathf.Max(0, elementalDamage);
        Element = element;
        Knockback = knockback;
        HitPosition = hitPosition;
        StunDuration = Mathf.Max(0f, stunDuration);
        SwingId = swingId;
    }
}
