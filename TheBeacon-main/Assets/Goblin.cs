using UnityEngine;

public class Goblin : Entity
{
    private bool playerDetected;
    private Transform target;

    [Header("Detection & Chase")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackStopDistance = 1.2f;

    [Header("Goblin Music")]
    [SerializeField] private AudioSource bgMusicSource;

    protected override void Awake()
    {
        base.Awake();
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj) target = playerObj.transform;

        if (bgMusicSource != null)
        {
            bgMusicSource.loop = true; // Ensure it stays on loop
            bgMusicSource.Play();
        }
    }

    protected override void Update()
    {
        if (isDead) return;

        base.Update();

        HandleCollision();
        // FIX: Pass 0, 0 because the goblin doesn't use player keyboard input
        HandleAnimations(0, 0);
        HandleFlip();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        HandleMovement();
    }

    // FIX: Add (float xInput, float yInput) to match the Entity script signature
    protected override void HandleAnimations(float xInput, float yInput)
    {
        if (isDead) return;

        // Using Mathf.Abs ensures the walking animation plays regardless of direction
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    protected override void HandleAttack()
    {
        if (isDead) return;

        if (playerDetected)
            anim.SetTrigger("attack");
    }


    protected override void HandleMovement()
    {
        // 1. Check if the target exists first to avoid errors
        if (target == null || !canMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }




        // 2. Calculate distance ONCE
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // 3. Handle the music based on that distance
        if (distanceToPlayer < detectionRange)
        {
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

            // 4. Handle the actual movement
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