using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    //protected UI ui;
    protected Animator anim;
    protected Rigidbody2D rb;
    protected Collider2D col;
    protected SpriteRenderer sr;

    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private int currentHealth;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damagefeedbackDuration = 0.1f;
    private Coroutine damageFeedbackCoroutine;

    [Header("Attack details")]
    [SerializeField] protected float attackRadius;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected LayerMask whatIsTarget;




    [Header("Collision details")]
    [SerializeField] private float growCheckDistance;
    [SerializeField] private LayerMask whatisGround;

    // Facing direction: 1 for right, -1 for left
    protected int facingDir = 1;
    protected bool facingRight = true;
    protected bool canMove = true;

    protected bool isGrounded;

    protected virtual void Awake()
    {
        //ui = FindFirstObjectByType<UI>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();
        currentHealth = maxHealth;

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
        Collider2D[] enemyColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);

        foreach (Collider2D enemy in enemyColliders)
        {
            Entity entityTarget = enemy.GetComponent<Entity>();
            if (entityTarget != null)
                entityTarget.takeDamage();
        }
    }

    private void takeDamage()
    {
        currentHealth = currentHealth - 1;
        PlayDamageFeedback();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
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
        StartCoroutine(DamageFeedbackCo());
    }

    private IEnumerator DamageFeedbackCo()
    {
        Material originalMat = sr.material;

        sr.material = damageMaterial;
        yield return new WaitForSeconds(damagefeedbackDuration);
        sr.material = originalMat;
    }



    public virtual void EnableMovement(bool enable)
    {
        canMove = enable;
        
    }

    protected void HandleAnimations()
    {
        anim.SetFloat("Xvelocity", rb.linearVelocity.x);
        anim.SetFloat("Yvelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
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

        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}