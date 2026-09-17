using UnityEngine;

[RequireComponent(typeof(PlayerProgression))]
public sealed class PlayerSkillController : MonoBehaviour
{
    [SerializeField] private SkillDefinition powerStrike;
    [SerializeField] private SkillDefinition crushingForce;
    [SerializeField] private SkillDefinition quickRecovery;

    private Player player;
    private PlayerProgression progression;
    private float powerStrikeReadyTime;

    public SkillDefinition PowerStrike => powerStrike;
    public SkillDefinition CrushingForce => crushingForce;
    public SkillDefinition QuickRecovery => quickRecovery;
    public float RemainingCooldown => Mathf.Max(0f, powerStrikeReadyTime - Time.time);
    public bool IsPowerStrikeUnlocked => powerStrike != null && progression.IsSkillUnlocked(powerStrike.StableId);
    public bool IsPowerStrikeReady => IsPowerStrikeUnlocked && RemainingCooldown <= 0f;

    private void Awake()
    {
        player = GetComponent<Player>();
        progression = GetComponent<PlayerProgression>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            progression.TryUnlockSkill(powerStrike);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            progression.TryUnlockSkill(crushingForce);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            progression.TryUnlockSkill(quickRecovery);
        if (Input.GetKeyDown(KeyCode.Q))
            TryUsePowerStrike();
    }

    public bool TryUsePowerStrike()
    {
        if (!IsPowerStrikeReady)
        {
            return false;
        }

        bool crushingForceUnlocked = crushingForce != null && progression.IsSkillUnlocked(crushingForce.StableId);
        float multiplier = crushingForceUnlocked
            ? crushingForce.DamageMultiplier
            : powerStrike.DamageMultiplier;

        if (!player.TryStartSkillAttack(multiplier))
        {
            return false;
        }

        bool quickRecoveryUnlocked = quickRecovery != null && progression.IsSkillUnlocked(quickRecovery.StableId);
        float cooldown = quickRecoveryUnlocked
            ? quickRecovery.Cooldown
            : powerStrike.Cooldown;
        powerStrikeReadyTime = Time.time + cooldown;
        return true;
    }
}
