using System.Text;
using TMPro;
using UnityEngine;

public sealed class ProgressionUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI progressionText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private GameObject skillTreePanel;
    [SerializeField] private TextMeshProUGUI skillTreeText;

    private PlayerProgression progression;
    private PlayerSkillController skills;
    private float nextCooldownRefresh;

    private void Start()
    {
        progression = FindFirstObjectByType<PlayerProgression>();
        skills = FindFirstObjectByType<PlayerSkillController>();

        if (progression == null || skills == null)
        {
            Debug.LogError("Progression UI requires PlayerProgression and PlayerSkillController.", this);
            enabled = false;
            return;
        }

        progression.ProgressChanged += RefreshStaticText;
        skillTreePanel.SetActive(false);
        RefreshStaticText();
    }

    private void OnDestroy()
    {
        if (progression != null)
            progression.ProgressChanged -= RefreshStaticText;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            skillTreePanel.SetActive(!skillTreePanel.activeSelf);
            RefreshSkillTree();
        }

        if (Time.unscaledTime >= nextCooldownRefresh)
        {
            nextCooldownRefresh = Time.unscaledTime + 0.1f;
            RefreshCooldownText();
        }
    }

    private void RefreshCooldownText()
    {
        if (!skills.IsPowerStrikeUnlocked)
            cooldownText.text = "Power Strike: Locked";
        else if (skills.IsPowerStrikeReady)
            cooldownText.text = "Power Strike: Ready [Q]";
        else
            cooldownText.text = $"Power Strike: {skills.RemainingCooldown:F1}s";
    }

    private void RefreshStaticText()
    {
        progressionText.text =
            $"Lv {progression.Level}  XP {progression.CurrentExperience}/{progression.ExperienceToNextLevel}  " +
            $"Gold {progression.Currency}  SP {progression.SkillPoints}";
        RefreshSkillTree();
    }

    private void RefreshSkillTree()
    {
        if (skillTreeText == null || skills == null || progression == null)
            return;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("SKILL TREE  [K close]");
        builder.AppendLine($"Skill Points: {progression.SkillPoints}");
        AppendSkill(builder, "[1]", skills.PowerStrike);
        AppendSkill(builder, "[2]", skills.CrushingForce);
        AppendSkill(builder, "[3]", skills.QuickRecovery);
        builder.AppendLine();
        builder.Append("Use Power Strike: [Q]");
        skillTreeText.text = builder.ToString();
    }

    private void AppendSkill(StringBuilder builder, string control, SkillDefinition skill)
    {
        if (skill == null)
            return;

        string state = progression.IsSkillUnlocked(skill.StableId) ? "UNLOCKED" : "LOCKED";
        builder.AppendLine();
        builder.AppendLine($"{control} {skill.DisplayName} — {state} — Cost {skill.SkillPointCost}");
        builder.AppendLine(skill.Description);

        if (!string.IsNullOrWhiteSpace(skill.PrerequisiteId))
            builder.AppendLine($"Requires: {skill.PrerequisiteId}");
        if (!string.IsNullOrWhiteSpace(skill.ConflictingSkillId))
            builder.AppendLine($"Conflicts: {skill.ConflictingSkillId}");
    }
}
