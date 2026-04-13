using UnityEngine;

public class SkeletonHammer : Entity
{
    private bool playerDetected;
    private Transform target;

    [Header("Detection & Chase")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackStopDistance = 1.5f;

    [Header("Attack Timing")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int hammerDamage = 2; // <--- ADJUST DAMAGE HERE
    private float lastAttackTime;

    [Header("Audio")]
    [SerializeField] private AudioSource bgMusicSource;

    [Header("Movement Audio")]
    [SerializeField] private AudioSource walkSoundSource;
    [SerializeField] private float stepRate = 0.5f;
    private float lastStepTime;

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

    // --- DAMAGE LOGIC START ---
    // This function should be called by an Animation Event during the swing
    public void AnimationDamageTrigger()
    {
        // Find all targets inside the red attack circle
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);

        foreach (Collider2D enemy in hitTargets)
        {
            // Try to find the Entity script on the player
            Entity player = enemy.GetComponent<Entity>();

            if (player != null)
            {
                player.TakeDamage(hammerDamage); // Deals 2 damage
                Debug.Log("Skeleton hit player for " + hammerDamage + " damage!");
            }
        }
    }
    // --- DAMAGE LOGIC END ---

    protected override void HandleAnimations(float xInput, float yInput)
    {
        if (isDead) return;

        float currentVelocity = Mathf.Abs(rb.linearVelocity.x);

        anim.SetFloat("xVelocity", currentVelocity);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);

        if (currentVelocity > 0.1f && isGrounded)
        {
            if (Time.time >= lastStepTime + stepRate)
            {
                if (walkSoundSource != null)
                {
                    walkSoundSource.pitch = Random.Range(0.8f, 1.1f);
                    walkSoundSource.Play();
                }
                lastStepTime = Time.time;
            }
        }
    }

    protected override void HandleAttack()
    {
        if (isDead) return;

        if (playerDetected && Time.time >= lastAttackTime + attackCooldown)
        {
            anim.SetTrigger("isAttack");
            lastAttackTime = Time.time;

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

        if (bgMusicSource != null)
        {
            if (distanceToPlayer < detectionRange)
            {
                if (!bgMusicSource.isPlaying) bgMusicSource.UnPause();
            }
            else
            {
                if (bgMusicSource.isPlaying) bgMusicSource.Pause();
            }
        }

        if (distanceToPlayer < detectionRange && distanceToPlayer > attackStopDistance)
        {
            float directionX = target.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    protected override void HandleFlip()
    {
        if (isDead || target == null) return;

        if (target.position.x > transform.position.x && !facingRight)
            Flip();
        else if (target.position.x < transform.position.x && facingRight)
            Flip();
    }

    protected override void HandleCollision()
    {
        base.HandleCollision();
        if (attackPoint != null)
        {
            playerDetected = Physics2D.OverlapCircle(attackPoint.position, attackRadius, whatIsTarget);
        }
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