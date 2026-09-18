using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private static readonly int XVelocityHash = Animator.StringToHash("Xvelocity");
    private static readonly int YVelocityHash = Animator.StringToHash("Yvelocity");
    private static readonly int IsGroundedHash = Animator.StringToHash("isGrounded");

    //protected UI ui;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer sr;

    protected EntityHealth health;
    protected EntityCombat combat;
    protected KnockbackReceiver knockbackReceiver;
    protected StatusEffectReceiver statusEffects;

    [Header("Damage feedback")]
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damagefeedbackDuration = 0.1f;
    private Coroutine damageFeedbackCoroutine;
    private Material originalMaterial;

    [Header("Collision details")]
    [SerializeField] private float growCheckDistance;
    [SerializeField] private LayerMask whatisGround;

    // Facing direction: 1 for right, -1 for left
    protected int facingDir = 1;
    protected bool facingRight = true;
    protected bool canMove = true;

    protected bool isGrounded;

    public int FacingDirection => facingDir;
    public bool IsKnockbackActive => knockbackReceiver != null && knockbackReceiver.IsActive;

    private bool hasXVelocityParameter;
    private bool hasYVelocityParameter;
    private bool hasIsGroundedParameter;

    protected virtual void Awake()
    {
        //ui = FindFirstObjectByType<UI>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        health = GetComponent<EntityHealth>();
        combat = GetComponent<EntityCombat>();
        knockbackReceiver = GetComponent<KnockbackReceiver>();
        statusEffects = GetComponent<StatusEffectReceiver>();

        if (health == null)
        {
            Debug.LogError($"{name} requires an EntityHealth component.", this);
            enabled = false;
            return;
        }

        originalMaterial = sr.material;
        health.Damaged += HandleDamageReceived;
        health.Died += HandleDeath;

        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            hasXVelocityParameter |= parameter.nameHash == XVelocityHash;
            hasYVelocityParameter |= parameter.nameHash == YVelocityHash;
            hasIsGroundedParameter |= parameter.nameHash == IsGroundedHash;
        }

    }

    protected virtual void OnDestroy()
    {
        if (health != null)
        {
            health.Damaged -= HandleDamageReceived;
            health.Died -= HandleDeath;
        }
    }

    protected virtual void Update()
    {
        HandleCollision();
        HandleMovement();
        HandleAnimations();
        HandleFlip();
    }

    public void DamageTargets()
    {
        combat?.DamageTargets();
    }

    public void BeginAttackSwing()
    {
        combat?.BeginSwing();
    }

    public void EndAttackSwing()
    {
        combat?.EndSwing();
    }

    private void HandleDamageReceived(DamageContext context)
    {
        PlayDamageFeedback();
        knockbackReceiver?.Apply(context.Knockback, context.StunDuration);
        OnDamaged(context);
    }

    private void HandleDeath(DamageContext context)
    {
        Die();
    }

    protected virtual void OnDamaged(DamageContext context) { }

    protected virtual void Die()
    {
        EndAttackSwing();
        anim.enabled = false;
        col.enabled = false;

        rb.gravityScale = 12;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15);

        Destroy(gameObject, 3);
    }

    private void PlayDamageFeedback()
    {
        if (damageFeedbackCoroutine != null)
        {
            StopCoroutine(damageFeedbackCoroutine);
        }
        damageFeedbackCoroutine = StartCoroutine(DamageFeedbackCo());
    }

    private IEnumerator DamageFeedbackCo()
    {
        sr.material = damageMaterial;
        yield return new WaitForSeconds(damagefeedbackDuration);
        sr.material = originalMaterial;
        damageFeedbackCoroutine = null;
    }



    public virtual void EnableMovement(bool enable)
    {
        canMove = enable;
        
    }

    protected void HandleAnimations()
    {
        if (hasXVelocityParameter)
            anim.SetFloat(XVelocityHash, rb.linearVelocity.x);

        if (hasYVelocityParameter)
            anim.SetFloat(YVelocityHash, rb.linearVelocity.y);

        if (hasIsGroundedParameter)
            anim.SetBool(IsGroundedHash, isGrounded);
    }



    protected virtual void HandleAttack()
    {
        if (isGrounded)
            anim.SetTrigger("attack");
    }

    protected virtual void HandleMovement()
    {

    }

    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, growCheckDistance, whatisGround);
    }

    protected virtual void HandleFlip()
    {
        if (rb.linearVelocity.x > 0 && facingRight == false)
        {
            Flip();
        }
        else if (rb.linearVelocity.x < 0 && facingRight == true)
        {
            Flip();
        }
    }

    public void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir *= -1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, -growCheckDistance));

    }
}
