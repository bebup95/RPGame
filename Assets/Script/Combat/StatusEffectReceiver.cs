using UnityEngine;

[RequireComponent(typeof(EntityHealth))]
public sealed class StatusEffectReceiver : MonoBehaviour
{
    private const float ChilledMovementMultiplier = 0.65f;
    private const float ElectrifiedDamageMultiplier = 1.25f;

    private EntityHealth health;
    private float chilledUntil;
    private float burnedUntil;
    private float electrifiedUntil;
    private float nextBurnTick;
    private int burnPotency;
    private Entity burnDealer;

    public float MovementMultiplier => Time.time < chilledUntil ? ChilledMovementMultiplier : 1f;
    public float IncomingDamageMultiplier => Time.time < electrifiedUntil ? ElectrifiedDamageMultiplier : 1f;

    private void Awake()
    {
        health = GetComponent<EntityHealth>();
    }

    private void Update()
    {
        if (Time.time >= burnedUntil || Time.time < nextBurnTick || burnPotency <= 0)
        {
            return;
        }

        nextBurnTick = Time.time + 1f;
        health.ReceiveStatusDamage(burnPotency, burnDealer);
    }

    public void Apply(StatusEffectType type, float duration, int potency, Entity dealer)
    {
        if (type == StatusEffectType.None || duration <= 0f)
        {
            return;
        }

        float expiresAt = Time.time + duration;
        switch (type)
        {
            case StatusEffectType.Chilled:
                chilledUntil = Mathf.Max(chilledUntil, expiresAt);
                break;
            case StatusEffectType.Burned:
                burnedUntil = Mathf.Max(burnedUntil, expiresAt);
                burnPotency = Mathf.Max(burnPotency, potency);
                burnDealer = dealer;
                nextBurnTick = Mathf.Min(nextBurnTick, Time.time);
                break;
            case StatusEffectType.Electrified:
                electrifiedUntil = Mathf.Max(electrifiedUntil, expiresAt);
                break;
        }
    }
}
