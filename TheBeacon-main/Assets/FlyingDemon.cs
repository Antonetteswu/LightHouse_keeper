using UnityEngine;

public class Demon : Entity
{
    private bool playerDetected;
    private Transform target;

    [Header("Detection & Chase")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackStopDistance = 1.2f;

    [Header("Attack Timing")]
    [SerializeField] private float attackCooldown = 1.5f; // Time between attacks
    private float lastAttackTime;

    [Header("Goblin Music")]
    [SerializeField] private AudioSource bgMusicSource;

    protected override void Awake()
    {
        base.Awake();
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj) target = playerObj.transform;

        if (bgMusicSource != null)
        {
            bgMusicSource.loop = true;
            bgMusicSource.Play();
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        base.Update();

        HandleCollision();
        HandleAnimations(0, 0);
        HandleFlip();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        HandleMovement();
    }

    protected override void HandleAnimations(float xInput, float yInput)
    {
        if (isDead) return;
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    protected override void HandleAttack()
    {
        if (isDead) return;

        // NEW: Only trigger attack if player is detected AND cooldown is over
        if (playerDetected && Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger("isAttack");
            anim.SetTrigger("isAttack");
            lastAttackTime = Time.time;

            // Stop moving during attack so he doesn't slide past the player
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    protected override void HandleMovement()
    {
        if (target == null || !canMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        if (distanceToPlayer < detectionRange)
        {
            // MUSIC LOGIC... (Keep as is)

            // If we are outside stop distance, MOVE
            if (distanceToPlayer > attackStopDistance)
            {
                float directionX = target.position.x > transform.position.x ? 1 : -1;
                rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);
            }
            // If we are inside stop distance, BRAKE HARD
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    protected override void HandleFlip()
    {
        if (isDead || target == null) return;

        // INVERTED LOGIC: If player is to the right and we ARE facing right, flip 
        // (Use this if your sprite is drawn facing LEFT by default)
        if (target.position.x > transform.position.x && facingRight)
        {
            Flip();
        }
        else if (target.position.x < transform.position.x && !facingRight)
        {
            Flip();
        }
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();
        if (attackPoint != null)
        {
            // Ensures the red circle is looking for the "Player" layer
            playerDetected = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsTarget);
        }
    }
    // Change 'public' to 'protected' so it matches the Entity script
    protected override void Die()
    {
        base.Die(); // This runs the original death code from Entity.cs

        // Unlocks the stone portal
        PlayerPrefs.SetInt("DemonDefeated", 1);
        PlayerPrefs.Save();

        Debug.Log("Demon defeated! The stone portal is now unlocked.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}