using UnityEngine;

[CreateAssetMenu(menuName = "RPGame/Enemy Profile", fileName = "EnemyProfile")]
public sealed class EnemyProfile : ScriptableObject
{
    [SerializeField] private string displayName = "Forest Enemy";
    [SerializeField, Min(1)] private int maxHealth = 2;
    [SerializeField, Min(0f)] private float moveSpeed = 2f;
    [SerializeField, Min(0f)] private float detectionRadius = 7f;
    [SerializeField, Min(0f)] private float disengageRadius = 10f;
    [SerializeField, Min(0f)] private float attackCooldown = 1.25f;
    [SerializeField] private Color tint = Color.white;

    public string DisplayName => displayName;
    public int MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float DetectionRadius => detectionRadius;
    public float DisengageRadius => Mathf.Max(detectionRadius, disengageRadius);
    public float AttackCooldown => attackCooldown;
    public Color Tint => tint;
}
