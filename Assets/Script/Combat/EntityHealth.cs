using System;
using UnityEngine;

public sealed class EntityHealth : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealth = 1;
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.15f;

    private float invulnerableUntil;

    public event Action<int, int> HealthChanged;
    public event Action<DamageContext> Damaged;
    public event Action<DamageContext> Died;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public bool ReceiveDamage(DamageContext context)
    {
        if (IsDead || context.TotalDamage <= 0 || Time.time < invulnerableUntil)
        {
            return false;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - context.TotalDamage);
        invulnerableUntil = Time.time + invulnerabilityDuration;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Damaged?.Invoke(context);

        if (IsDead)
        {
            Died?.Invoke(context);
        }

        return true;
    }

    public void ConfigureMaxHealth(int value, bool restoreToFull)
    {
        maxHealth = Mathf.Max(1, value);
        CurrentHealth = restoreToFull ? maxHealth : Mathf.Min(CurrentHealth, maxHealth);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void RestoreFullHealth()
    {
        CurrentHealth = maxHealth;
        invulnerableUntil = 0f;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }
}
