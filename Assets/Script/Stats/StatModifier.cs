using System;

[Serializable]
public struct StatModifier
{
    public int maxHealth;
    public int physicalPower;
    public int armor;

    public StatModifier(int maxHealth, int physicalPower, int armor)
    {
        this.maxHealth = maxHealth;
        this.physicalPower = physicalPower;
        this.armor = armor;
    }
}
