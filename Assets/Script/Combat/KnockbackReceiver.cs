using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class KnockbackReceiver : MonoBehaviour
{
    [SerializeField, Min(0f)] private float minimumDuration = 0.12f;

    private Rigidbody2D body;
    private float activeUntil;

    public bool IsActive => Time.time < activeUntil;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Apply(Vector2 velocity, float duration)
    {
        if (velocity.sqrMagnitude <= 0f)
        {
            return;
        }

        activeUntil = Time.time + Mathf.Max(minimumDuration, duration);
        body.linearVelocity = velocity;
    }
}
