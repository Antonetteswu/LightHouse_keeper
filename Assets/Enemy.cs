using UnityEngine;

public class Enemy : Entity
{
    private bool playerDetected;
    private Transform target;

    [Header("Detection & Chase")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackStopDistance = 1.2f;


    protected override void Awake()
    {
        base.Awake();
        // Automatically find the player by name
        GameObject playerObj = GameObject.Find("Player");
        if (playerObj) target = playerObj.transform;
    }

    protected override void Update()
    {
        if (isDead) return;

        base.Update();

        HandleCollision();
        // FIX: Pass 0, 0 because enemies don't use keyboard input
        HandleAnimations(0, 0);
        HandleFlip();
        HandleAttack();
    }

    private void FixedUpdate()
    {
        if (isDead) return; // Add this!

        HandleMovement();
    }

    protected override void HandleAnimations(float xInput, float yInput)
    {
        if (isDead) return;

        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        // You can also add yVelocity here if your enemy jumps or falls
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }
    protected override void HandleAttack()
    {
        if (isDead) return; // Add this! 

        if (playerDetected)
            anim.SetTrigger("attack");
    }

    protected override void HandleMovement()
    {
        if (target == null || !canMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);

        // Logic: If close enough to "see" but far enough that we need to walk closer
        if (distanceToPlayer < detectionRange && distanceToPlayer > attackStopDistance)
        {
            float directionX = target.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocity = new Vector2(directionX * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // Stop moving if player is too far or close enough to attack
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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

    // This helps you see the ranges in the Scene view!
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