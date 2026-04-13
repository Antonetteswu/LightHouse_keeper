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


    [Header("Audio")]
    [SerializeField] private AudioSource attackSound;
    [SerializeField] private AudioSource JumpSound;
    [SerializeField] private AudioSource runSound;
    [SerializeField] private AudioSource slideSound;

    [Header("Health")]
    [SerializeField] public int maxHealth = 20;
    [SerializeField] public int currentHealth;
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFeedbackDuration = 2f;
    private Coroutine damageFeedbackCoroutine;
    public bool isDead = false;

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
    protected bool facingRight = true;
    protected bool canMove = true;
    private bool canJump = true;

    [Header("Collision details")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private LayerMask whatisGround;
    protected bool isGrounded;

    private Material defaultMaterial;

    private Rigidbody2D platformRb;
    private bool isOnPlatform;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        sr = GetComponentInChildren<SpriteRenderer>();

        // SAVE THE NORMAL LOOK HERE
        if (sr != null) defaultMaterial = sr.material;

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
        HandleRunSound(); // NEW: Check if we should play footsteps
    }
    private void HandleRunSound()
    {
        if (runSound == null) return;

        // Condition: Moving horizontally, grounded, not sliding, and not dead
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (isMoving && isGrounded && !isSliding && !isDead && !isClimbing)
        {
            if (!runSound.isPlaying)
            {
                runSound.Play();
            }
        }
        else
        {
            if (runSound.isPlaying)
            {
                runSound.Stop();
            }
        }
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");

        if (isNearLadder && Mathf.Abs(yInput) > 0.1f)
        {
            // FIX: If we are at the top and press DOWN, nudge the player 
            // slightly so they pass through the platform trigger faster.
            if (!isClimbing && yInput < -0.1f)
            {
                transform.position = new Vector2(transform.position.x, transform.position.y - 0.1f);
            }

            isClimbing = true;
        }

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

            // ADD THIS LINE: This tells the player's physics to ignore the ground
            // so you can move DOWN through the platform.
            rb.excludeLayers = whatisGround;
        }
        else
        {
            rb.gravityScale = 2;

            // RESET THIS LINE: Turn collisions back on when you stop climbing.
            rb.excludeLayers = 0;

            // ... rest of your existing platform/movement logic
            float targetXVelocity = xInput * moveSpeed;
            if (isOnPlatform && platformRb != null)
                targetXVelocity += platformRb.linearVelocity.x;

            if (canMove)
                rb.linearVelocity = new Vector2(targetXVelocity, rb.linearVelocity.y);
            else
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    protected virtual void HandleCollision()
    {
        // FIX: If we are climbing, we are NOT grounded. 
        // This stops the 'grounded' logic from fighting the 'climbing' logic.
        if (isClimbing)
        {
            isGrounded = false;
        }
        else
        {
            isGrounded = Physics2D.Raycast(col.bounds.center, Vector2.down, col.bounds.extents.y + groundCheckDistance, whatisGround);
        }

        isNearLadder = Physics2D.OverlapCircle(transform.position, ladderCheckDistance, whatIsLadder);

        if (!isNearLadder)
        {
            isClimbing = false;
        }
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
    public void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }
    // --- MISSING METHODS ADDED BELOW TO FIX ERRORS ---

    private void TryToJump()
    {
        if (isGrounded && canJump)
        {
            // 1. Play the Jump Sound
            if (JumpSound != null)
            {
                JumpSound.Play();
            }
            else
            {
                Debug.LogWarning("Jump AudioSource is missing!");
            }

            // 2. Apply Jump Force
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            // 3. Parent Logic for moving platforms
            if (transform.parent != null)
            {
                transform.SetParent(null);
            }
        }
    }
    protected virtual void HandleAttack()
    {
        // We only trigger the Routine. The Routine triggers the Animation.
        // The Animation triggers the Sound.
        if (canMove && !isDead)
        {
            StartCoroutine(AttackRoutine());
        }
    }


    private IEnumerator AttackRoutine()
    {
        canMove = false;
        rb.linearVelocity = Vector2.zero;

        // ADD THIS LINE HERE:
        PlayAttackSound();

        anim.SetTrigger("isAttack");
        yield return new WaitForSeconds(0.2f);
        DamageTargets();
        yield return new WaitForSeconds(0.2f);
        canMove = true;

        {
            if (attackSound != null)
            {
                Debug.Log("The sound SHOULD be playing now!"); // Add this line
                attackSound.Play();
            }
            else
            {
                Debug.LogError("The script cannot find the Audio Source!");
            }
        }
    }

    public virtual void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }
    public void DamageTargets()
    {
        Collider2D[] targets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);

        foreach (var target in targets)
        {
            // 1. Try to hit a Golem
            GolemEnemy golem = target.GetComponent<GolemEnemy>();
            if (golem != null)
            {
                golem.TakeDamage(10); // Or whatever damage your player does
                continue; // Skip the rest of the loop for this target
            }

            // 2. Try to hit a Skeleton (or other Entities)
            Entity otherEntity = target.GetComponent<Entity>();
            if (otherEntity != null)
            {
                otherEntity.TakeDamage(1);
            }
        }
    }

    // Add 'int damage' inside the parentheses
    public void TakeDamage(int damage = 1)
    {
        if (isDead) return;

        currentHealth -= damage;

        // Use the refresh function here too!
        UpdateUI();

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
        sr.material = damageMaterial; // Switch to white

        yield return new WaitForSeconds(damageFeedbackDuration);

        sr.material = defaultMaterial; // Always switch back to the saved default
        damageFeedbackCoroutine = null;
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // FIX: Only trigger Game Over if the object dying has the "Player" tag
        if (gameObject.CompareTag("Player"))
        {
            if (GameOver.instance != null)
            {
                GameOver.instance.GameOverPanel();
            }
        }

        // Stop the character
        canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        // Play animation
        anim.SetTrigger("isDead");

        // Clean up materials
        if (damageFeedbackCoroutine != null) StopCoroutine(damageFeedbackCoroutine);
        sr.material = new Material(Shader.Find("Sprites/Default"));
    }
    private IEnumerator SlideRoutine()
    {
        isSliding = true;
        canMove = false;
        slideCooldownTimer = slideCooldown;

        // --- NEW: Play Slide Sound ---
        if (slideSound != null)
        {
            slideSound.Play();
        }

        anim.SetBool("isSliding", true);
        rb.linearVelocity = new Vector2(facingDir * slideSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(slideDuration);

        // --- NEW: Stop Slide Sound (optional, if the clip is long) ---
        if (slideSound != null && slideSound.isPlaying)
        {
            slideSound.Stop();
        }

        anim.SetBool("isSliding", false);
        isSliding = false;
        if (!isDead) canMove = true;
    }

    protected virtual void HandleFlip()
    {
        if (rb.linearVelocity.x > 0.1f && !facingRight) Flip();
        else if (rb.linearVelocity.x < -0.1f && facingRight) Flip();
    }

   protected virtual void Flip()
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
    public void PlayAttackSound()
    {
        if (attackSound != null)
        {
            attackSound.Play();
        }
        else
        {
            Debug.LogWarning("Attack AudioSource is missing from the Player script!");
        }


    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(collision.transform);
            isOnPlatform = true;
            platformRb = collision.gameObject.GetComponent<Rigidbody2D>();
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null);
            isOnPlatform = false;
            platformRb = null;
        }
    }
}