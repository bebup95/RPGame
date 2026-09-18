using UnityEngine;

[CreateAssetMenu(menuName = "RPGame/Character Stat Profile", fileName = "CharacterStatProfile")]
public sealed class CharacterStatProfile : ScriptableObject
{
    [SerializeField, Min(1)] private int maxHealth = 10;
    [SerializeField, Min(0)] private int physicalPower;
    [SerializeField, Min(0)] private int armor;
    [SerializeField, Range(0f, 1f)] private float criticalChance;
    [SerializeField, Min(1f)] private float criticalPower = 1.5f;
    [SerializeField, Min(0f)] private float healthRegeneration;
    [SerializeField, Range(0f, 0.8f)] private float fireResistance;
    [SerializeField, Range(0f, 0.8f)] private float iceResistance;
    [SerializeField, Range(0f, 0.8f)] private float lightningResistance;

    public int MaxHealth => maxHealth;
    public int PhysicalPower => physicalPower;
    public int Armor => armor;
    public float CriticalChance => criticalChance;
    public float CriticalPower => criticalPower;
    public float HealthRegeneration => healthRegeneration;

    public float GetResistance(DamageElement element)
    {
        switch (element)
        {
            case DamageElement.Fire:
                return fireResistance;
            case DamageElement.Ice:
                return iceResistance;
            case DamageElement.Lightning:
                return lightningResistance;
            default:
                return 0f;
        }
    }
}
