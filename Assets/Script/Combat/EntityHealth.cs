using System;
using UnityEngine;

public sealed class EntityHealth : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1)] private int maxHealth = 1;
    [SerializeField, Min(0f)] private float invulnerabilityDuration = 0.15f;

    private float invulnerableUntil;
    private float regenerationAccumulator;
    private CharacterStats stats;
    private StatusEffectReceiver statusEffects;

    public event Action<int, int> HealthChanged;
    public event Action<DamageContext> Damaged;
    public event Action<DamageContext> Died;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    private void Awake()
    {
        stats = GetComponent<CharacterStats>();
        statusEffects = GetComponent<StatusEffectReceiver>();
        CurrentHealth = maxHealth;
        if (stats != null)
            stats.StatsChanged += HandleStatsChanged;
    }

    private void OnDestroy()
    {
        if (stats != null)
            stats.StatsChanged -= HandleStatsChanged;
    }

    private void Start()
    {
        if (stats != null)
        {
            ConfigureMaxHealth(stats.MaxHealth, true);
        }
    }

    private void Update()
    {
        if (IsDead || stats == null || stats.HealthRegeneration <= 0f || CurrentHealth >= maxHealth)
        {
            return;
        }

        regenerationAccumulator += stats.HealthRegeneration * Time.deltaTime;
        int wholeHealth = Mathf.FloorToInt(regenerationAccumulator);
        if (wholeHealth > 0)
        {
            regenerationAccumulator -= wholeHealth;
            Heal(wholeHealth);
        }
    }

    public bool ReceiveDamage(DamageContext context)
    {
        if (IsDead || context.TotalDamage <= 0 || Time.time < invulnerableUntil)
        {
            return false;
        }

        int physicalDamage = stats != null
            ? stats.MitigatePhysical(context.PhysicalDamage)
            : context.PhysicalDamage;
        int elementalDamage = stats != null
            ? stats.MitigateElemental(context.ElementalDamage, context.Element)
            : context.ElementalDamage;
        float incomingMultiplier = statusEffects != null ? statusEffects.IncomingDamageMultiplier : 1f;
        int finalDamage = Mathf.Max(
            1,
            Mathf.FloorToInt((physicalDamage + elementalDamage) * incomingMultiplier + 0.5f));

        CurrentHealth = Mathf.Max(0, CurrentHealth - finalDamage);
        invulnerableUntil = Time.time + invulnerabilityDuration;
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Damaged?.Invoke(context);
        statusEffects?.Apply(
            context.StatusEffect,
            context.StatusDuration,
            context.StatusPotency,
            context.Dealer);

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

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0 || CurrentHealth >= maxHealth)
        {
            return;
        }

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void ReceiveStatusDamage(int amount, Entity dealer)
    {
        if (IsDead || amount <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        DamageContext context = new DamageContext(
            dealer,
            amount,
            0,
            DamageElement.None,
            false,
            Vector2.zero,
            transform.position,
            0f,
            StatusEffectType.None,
            0f,
            0,
            -1);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);
        Damaged?.Invoke(context);
        if (IsDead)
            Died?.Invoke(context);
    }

    private void HandleStatsChanged()
    {
        ConfigureMaxHealth(stats.MaxHealth, false);
    }
}
