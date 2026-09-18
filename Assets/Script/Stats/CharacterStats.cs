using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CharacterStats : MonoBehaviour
{
    [SerializeField] private CharacterStatProfile profile;

    private readonly Dictionary<string, StatModifier> runtimeModifiers = new Dictionary<string, StatModifier>();

    public event Action StatsChanged;

    public int MaxHealth => Mathf.Max(1, (profile != null ? profile.MaxHealth : 1) + SumModifiers(x => x.maxHealth));
    public int PhysicalPower => Mathf.Max(0, (profile != null ? profile.PhysicalPower : 0) + SumModifiers(x => x.physicalPower));
    public int Armor => Mathf.Max(0, (profile != null ? profile.Armor : 0) + SumModifiers(x => x.armor));
    public float CriticalChance => profile != null ? profile.CriticalChance : 0f;
    public float CriticalPower => profile != null ? profile.CriticalPower : 1.5f;
    public float HealthRegeneration => profile != null ? profile.HealthRegeneration : 0f;

    public void SetModifier(string sourceId, StatModifier modifier)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return;

        runtimeModifiers[sourceId] = modifier;
        StatsChanged?.Invoke();
    }

    public void RemoveModifier(string sourceId)
    {
        if (!string.IsNullOrWhiteSpace(sourceId) && runtimeModifiers.Remove(sourceId))
            StatsChanged?.Invoke();
    }

    public int CalculateOutgoingPhysical(int baseDamage, float multiplier, out bool critical)
    {
        float scaledDamage = Mathf.Max(0f, baseDamage + PhysicalPower) * Mathf.Max(0f, multiplier);
        critical = scaledDamage > 0f && UnityEngine.Random.value < Mathf.Clamp01(CriticalChance);
        if (critical)
        {
            scaledDamage *= Mathf.Max(1f, CriticalPower);
        }

        return scaledDamage > 0f ? Mathf.Max(1, RoundPositive(scaledDamage)) : 0;
    }

    public int MitigatePhysical(int rawDamage)
    {
        if (rawDamage <= 0)
        {
            return 0;
        }

        float mitigated = rawDamage * 100f / (100f + Mathf.Max(0, Armor));
        return Mathf.Max(1, RoundPositive(mitigated));
    }

    public int MitigateElemental(int rawDamage, DamageElement element)
    {
        if (rawDamage <= 0)
        {
            return 0;
        }

        float resistance = profile != null ? profile.GetResistance(element) : 0f;
        return Mathf.Max(1, RoundPositive(rawDamage * (1f - Mathf.Clamp(resistance, 0f, 0.8f))));
    }

    private static int RoundPositive(float value)
    {
        return Mathf.FloorToInt(Mathf.Max(0f, value) + 0.5f);
    }

    private int SumModifiers(Func<StatModifier, int> selector)
    {
        int total = 0;
        foreach (StatModifier modifier in runtimeModifiers.Values)
            total += selector(modifier);
        return total;
    }
}
