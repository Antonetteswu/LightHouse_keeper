using System;
using System.Collections;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected Animator anim;
    protected Collider2D col;
    protected SpriteRenderer sr;

    public HealthBar healthBar;

    [Header("Health")]
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFeedbackDuration = .2f;
    private Coroutine damageFeedbackCoroutine;
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

    [Header("Climb details")]
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private LayerMask whatIsLadder;
    [SerializeField] private float ladderCheckDistance = 0.5f;
    private bool isClimbing;
    private bool isNearLadder;
    private float yInput;

    [Header("Movement details")]
    [SerializeField] protected float moveSpeed = 3.5f;
    [SerializeField] private float jumpForce = 15f;
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
        if (healthBar != null) healthBar.SetMaxHealth(maxHealth);
    }

    protected virtual void Update()
    {
        if (isDead) return;
        slideCooldownTimer -= Time.deltaTime;

        HandleCollision();
        HandleInput();
        HandleMovement();
        HandleAnimations(xInput, yInput);
        HandleFlip();
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (isNearLadder && Mathf.Abs(yInput) > 0.1f)
            isClimbing = true;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isClimbing) isClimbing = false;
            TryToJump();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0)) HandleAttack();

        if (Input.GetKeyDown(KeyCode.LeftShift) && slideCooldownTimer < 0 && isGrounded && !isSliding)
            StartCoroutine(SlideRoutine());
    }

    protected virtual void HandleMovement()
    {
        if (isSliding) return;

        if (isClimbing)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = new Vector2(xInput * moveSpeed * 0.5f, yInput * climbSpeed);

            // FIX: Ignore collision with the "Ground" layer so you can climb past the ledge
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ground"), true);
        }
        else
        {
            rb.gravityScale = 2;

            // FIX: Turn collisions back on when we stop climbing
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Ground"), false);

            if (canMove)
                rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);
            else
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    protected virtual void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + 0.1f, whatisGround);
        isNearLadder = Physics2D.OverlapCircle(transform.position, ladderCheckDistance, whatIsLadder);

        if (!isNearLadder) isClimbing = false;
    }

    protected virtual void HandleAnimations(float xInput, float yInput)
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isClimbing", isClimbing);

        if (isClimbing)
        {
            // Check if the player is actually pressing keys to move
            // We use Mathf.Abs to check if input is greater than 0.1 or less than -0.1
            bool isInputting = Mathf.Abs(yInput) > 0.1f || Mathf.Abs(xInput) > 0.1f;

            anim.speed = isInputting ? 1 : 0;
        }
        else
        {
            anim.speed = 1;
        }

        float moveAlpha = Mathf.Abs(rb.linearVelocity.x);
        anim.SetBool("isWalking", moveAlpha > 0.1f);
        anim.SetBool("isRunning", moveAlpha > (moveSpeed * 0.8f));

        if (!isGrounded && !isClimbing)
        {
            anim.SetBool("isJumping", rb.linearVelocity.y > 0.1f);
            anim.SetBool("isFalling", rb.linearVelocity.y < -0.1f);
        }
        else
        {
            anim.SetBool("isJumping", false);
            anim.SetBool("isFalling", false);
            anim.SetBool("isSliding", isSliding);
        }
    }

    // --- MISSING METHODS ADDED BELOW TO FIX ERRORS ---

    private void TryToJump()
    {
        if (isGrounded && canJump)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    protected virtual void HandleAttack()
    {
        if (canMove) StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("isAttack");
        yield return new WaitForSeconds(0.2f);
        DamageTargets();
        yield return new WaitForSeconds(0.2f);
        canMove = true;
    }

    public void DamageTargets()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);
        foreach (var target in targets)
        {
            if (target.GetComponent<Entity>())
                target.GetComponent<Entity>().TakeDamage();
        }
    }

    public void TakeDamage()
    {
        if (isDead) return;
        currentHealth--;
        if (healthBar != null) healthBar.SetHealth(currentHealth);
        PlayDamageFeedback();
        if (currentHealth <= 0) Die();
    }

    private void PlayDamageFeedback()
    {
        if (damageFeedbackCoroutine != null) StopCoroutine(damageFeedbackCoroutine);
        damageFeedbackCoroutine = StartCoroutine(DamageFeedbackCo());
    }

    private IEnumerator DamageFeedbackCo()
    {
        Material original = sr.material;
        sr.material = damageMaterial;
        yield return new WaitForSeconds(damageFeedbackDuration);
        sr.material = original;
    }

    protected virtual void Die()
    {
        isDead = true;
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("isDead");
    }

    private IEnumerator SlideRoutine()
    {
        isSliding = true;
        canMove = false;
        slideCooldownTimer = slideCooldown;
        anim.SetBool("isSliding", true);
        rb.linearVelocity = new Vector2(facingDir * slideSpeed, rb.linearVelocity.y);
        yield return new WaitForSeconds(slideDuration);
        anim.SetBool("isSliding", false);
        isSliding = false;
        if (!isDead) canMove = true;
    }

    protected void HandleFlip()
    {
        if (rb.linearVelocity.x > 0.1f && !facingRight) Flip();
        else if (rb.linearVelocity.x < -0.1f && facingRight) Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        facingDir *= -1;
        transform.Rotate(0, 180, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(transform.position, ladderCheckDistance);
        if (attackPoint != null) Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}