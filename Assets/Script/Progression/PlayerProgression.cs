using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public sealed class PlayerProgression : MonoBehaviour
{
    private readonly HashSet<string> unlockedSkillIds = new HashSet<string>();

    public event Action ProgressChanged;

    public int Level { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int SkillPoints { get; private set; }
    public int Currency { get; private set; }
    public int ExperienceToNextLevel => GetExperienceRequirement(Level);

    public static int GetExperienceRequirement(int level)
    {
        return 10 + 5 * Mathf.Max(0, level - 1);
    }

    public void AddRewards(int experience, int currency)
    {
        CurrentExperience += Mathf.Max(0, experience);
        Currency += Mathf.Max(0, currency);

        while (CurrentExperience >= ExperienceToNextLevel)
        {
            CurrentExperience -= ExperienceToNextLevel;
            Level++;
            SkillPoints++;
        }

        ProgressChanged?.Invoke();
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0)
            return;

        Currency += amount;
        ProgressChanged?.Invoke();
    }

    public bool TrySpendCurrency(int amount)
    {
        if (amount < 0 || Currency < amount)
            return false;

        Currency -= amount;
        ProgressChanged?.Invoke();
        return true;
    }

    public bool IsSkillUnlocked(string stableId)
    {
        return !string.IsNullOrWhiteSpace(stableId) && unlockedSkillIds.Contains(stableId);
    }

    public bool TryUnlockSkill(SkillDefinition skill)
    {
        if (skill == null || string.IsNullOrWhiteSpace(skill.StableId) || IsSkillUnlocked(skill.StableId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(skill.PrerequisiteId) && !IsSkillUnlocked(skill.PrerequisiteId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(skill.ConflictingSkillId) && IsSkillUnlocked(skill.ConflictingSkillId))
        {
            return false;
        }

        if (SkillPoints < skill.SkillPointCost)
        {
            return false;
        }

        SkillPoints -= skill.SkillPointCost;
        unlockedSkillIds.Add(skill.StableId);
        ProgressChanged?.Invoke();
        return true;
    }
}
