using UnityEngine;

public class EnemyToad : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform attackPoint;
    public float attackRadius = 1f; // Size of the damage circle
    public LayerMask playerLayer;

    [Header("Detection Settings")]
    public float detectionRange = 10f; // How far the toad can "see" the player
    public float attackRange = 1.5f;   // How close to be to start biting

    [Header("Hopping Settings")]
    public float hopForce = 5f;
    public float hopUpForce = 4f;
    public float hopInterval = 2f;
    public int damage = 10;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isAttacking = false;
    private float hopTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        hopTimer = hopInterval;
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        // 1. If very close, stop and attack
        if (distance < attackRange && !isAttacking)
        {
            StartAttack();
        }
        // 2. If the player is near but not close enough to hit, follow them
        else if (distance < detectionRange && !isAttacking)
        {
            MoveToPlayer();
        }
        // 3. Optional: If player is too far, stop moving
        else if (!isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // Update Animator
        anim.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    void MoveToPlayer()
    {
        hopTimer -= Time.deltaTime;

        if (hopTimer <= 0)
        {
            float dir = player.position.x > transform.position.x ? 1 : -1;
            transform.localScale = new Vector3(dir, 1, 1);
            rb.AddForce(new Vector2(dir * hopForce, hopUpForce), ForceMode2D.Impulse);
            hopTimer = hopInterval;
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        anim.SetTrigger("isAttack");
        Invoke("PerformDamage", 0.5f);
        Invoke("ResetAttack", 1.2f);
    }

    void PerformDamage()
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, attackRadius, playerLayer);
        if (hitPlayer != null)
        {
            hitPlayer.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    void ResetAttack() => isAttacking = false;

    private void OnDrawGizmos()
    {
        if (attackPoint == null) return;

        // Draw the Attack Circle (Red)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        // Draw the Detection Range (Yellow) so you can see it in the editor
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}