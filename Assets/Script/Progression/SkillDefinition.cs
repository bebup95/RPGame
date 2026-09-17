using UnityEngine;

[CreateAssetMenu(menuName = "RPGame/Skill Definition", fileName = "SkillDefinition")]
public sealed class SkillDefinition : ScriptableObject
{
    [SerializeField] private string stableId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea] private string description;
    [SerializeField, Min(0)] private int skillPointCost = 1;
    [SerializeField] private string prerequisiteId;
    [SerializeField] private string conflictingSkillId;
    [SerializeField, Min(0f)] private float cooldown;
    [SerializeField, Min(1f)] private float damageMultiplier = 1f;

    public string StableId => stableId;
    public string DisplayName => displayName;
    public string Description => description;
    public int SkillPointCost => skillPointCost;
    public string PrerequisiteId => prerequisiteId;
    public string ConflictingSkillId => conflictingSkillId;
    public float Cooldown => cooldown;
    public float DamageMultiplier => damageMultiplier;
}
