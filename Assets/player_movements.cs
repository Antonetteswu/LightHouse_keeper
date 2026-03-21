using System;
using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Collider2D col;
    protected SpriteRenderer sr;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int currentHealth;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFeedbackDuration = .2f;
    private Coroutine damageFeedbackCoroutine;

    // FIX 1: Added the missing isDead variable declaration
    protected bool isDead = false;

    [Header("Slide details")]
    [SerializeField] private float slideSpeed = 15f;
    [SerializeField] private float slideDuration = 0.5f;
    [SerializeField] private float slideCooldown = 1f;
    private float slideCooldownTimer;
    private bool isSliding;

    [Header("Attack details")]
    [SerializeField] protected float attackRadius;
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected LayerMask whatIsTarget;

    [Header("Movement details")]
    // NOTE: If you get "serialized multiple times" errors, ensure 
    // child classes (like Player or Enemy) don't also have a "moveSpeed" variable.
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] private float jumpForce = 8f;
    protected int facingDir = 1;
    private float xInput;
    private bool facingRight = true;
    protected bool canMove = true;
    private bool canJump = true;

    [Header("Collision details")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatisGround;
    private bool isGrounded;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        // FIX 2: This now works because isDead is declared above
        if (isDead) return;

        slideCooldownTimer -= Time.deltaTime;

        HandleCollision();
        HandleInput();
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
                entityTarget.TakeDamage();
        }
    }

    private IEnumerator HurtStunCoroutine()
    {
        canMove = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.2f);
        if (!isDead) canMove = true;
    }

    private void TakeDamage()
    {
        if (isDead) return;

        currentHealth -= 1;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            anim.SetTrigger("isHurting");
            PlayDamageFeedback();
            StartCoroutine(HurtStunCoroutine());
        }
    }

    private void PlayDamageFeedback()
    {
        if (damageFeedbackCoroutine != null)
            StopCoroutine(damageFeedbackCoroutine);

        StartCoroutine(DamageFeedbackCo());
    }

    private IEnumerator DamageFeedbackCo()
    {
        Material originalMat = sr.material;
        sr.material = damageMaterial;
        yield return new WaitForSeconds(damageFeedbackDuration);
        sr.material = originalMat;
    }

    protected virtual void Die()
    {
        isDead = true;
        canMove = false;

        // FIX 3: Sets to Static to prevent "linearVelocity" errors on static bodies
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        anim.SetBool("isDead", true);
        col.enabled = true;
    }

    protected virtual void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);

        float moveAlpha = Mathf.Abs(rb.linearVelocity.x);
        anim.SetBool("isWalking", moveAlpha > 0.1f);
        anim.SetBool("isRunning", moveAlpha > (moveSpeed * 0.8f));

        if (!isGrounded)
        {
            anim.SetBool("isJumping", rb.linearVelocity.y > 0.1f);
            anim.SetBool("isFalling", rb.linearVelocity.y < -0.1f);
        }
        else
        {
            anim.SetBool("isJumping", false);
            anim.SetBool("isFalling", false);

            // FIX 4: Moved this inside the Grounded check logic properly
            anim.SetBool("isSliding", isSliding);
        }
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;
        canMove = false;
        slideCooldownTimer = slideCooldown;

        // 1. Force the Animator into the sliding state
        anim.SetBool("isSliding", true);

        // 2. Apply the slide velocity
        rb.linearVelocity = new Vector2(facingDir * slideSpeed, rb.linearVelocity.y);

        // 3. Wait for the slide to finish
        yield return new WaitForSeconds(slideDuration);

        // 4. Reset states
        anim.SetBool("isSliding", false);
        isSliding = false;

        if (!isDead) canMove = true;
    }



    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space))
            TryToJump();

        if (Input.GetKeyDown(KeyCode.Mouse0))
            HandleAttack();

        if (Input.GetKeyDown(KeyCode.LeftShift) && slideCooldownTimer < 0 && isGrounded && !isSliding)
        {
            StartCoroutine(SlideRoutine());
        }
    }

    protected virtual void HandleAttack()
    {
        // 1. Check if we can move and are grounded before attacking
        if (isGrounded && canMove)
        {
            StartCoroutine(AttackRoutine());
        }
    }
    private IEnumerator AttackRoutine()
    {
        canMove = false; // Stops HandleMovement from moving the player
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Freeze X movement

        anim.SetTrigger("isAttack"); // Trigger the animation

        // 2. Wait for the duration of your attack animation (e.g., 0.4 seconds)
        // Adjust this time to match the length of your 'playerattack' clip
        yield return new WaitForSeconds(0.4f);

        canMove = true; // Let the player move again
    }
    private void TryToJump()
    {
        if (isGrounded && canJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    protected virtual void HandleMovement()
    {
        if (isSliding) return;

        if (canMove)
            rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + 0.1f, whatisGround);
    }

    protected void HandleFlip()
    {
        if (rb.linearVelocity.x > 0.1f && !facingRight)
            Flip();
        else if (rb.linearVelocity.x < -0.1f && facingRight)
            Flip();
    }

    private void Flip()
    {
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
        facingDir *= -1;
    }

    
    public void EnableMovementAndJump(bool enable)
    {
        canJump = enable;
        canMove = enable;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}