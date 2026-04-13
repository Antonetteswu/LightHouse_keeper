using UnityEngine;
using System.Collections;

public class GolemEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float attackRange = 2.5f;

    [Header("Combat Settings")]
    public int damageAmount = 20;
    public float attackRate = 1.5f;
    public Transform attackPoint;
    public float attackRadius = 1.0f;
    public LayerMask whatIsTarget;

    [Header("Health System")]
    public int maxHealth = 100;
    [SerializeField] private int currentHealth;

    [Header("Audio")]
    [SerializeField] private AudioSource attackSound;
    [SerializeField] private AudioSource walkSound;

    private float nextAttackTime = 0f;
    private Transform player;
    private Animator anim;
    private Rigidbody2D rb;
    private bool isDead = false;
    private Vector3 baseScale;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        baseScale = transform.localScale;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            Move();
        }
        else
        {
            StopMoving();
            if (Time.time >= nextAttackTime)
            {
                StartAttack();
                nextAttackTime = Time.time + attackRate;
            }
        }
    }

    void Move()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        anim.SetBool("isWalking", true);

        if (walkSound != null && !walkSound.isPlaying) walkSound.Play();

        transform.localScale = new Vector3(direction.x > 0 ? baseScale.x : -baseScale.x, baseScale.y, baseScale.z);
    }

    void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        anim.SetBool("isWalking", false);
        if (walkSound != null) walkSound.Stop();
    }

    void StartAttack()
    {
        anim.SetTrigger("isAttack");
    }

    public void Hit()
    {
        if (isDead) return;
        if (attackSound != null) attackSound.Play();

        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsTarget);

        foreach (Collider2D p in hitTargets)
        {
            // This shouts to the player to take damage
            p.gameObject.SendMessage("TakeDamage", damageAmount, SendMessageOptions.DontRequireReceiver);
            Debug.Log("Golem swinging at: " + p.name);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("Golem took damage! Current HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        anim.SetTrigger("isDead");

        if (GetComponent<Collider2D>() != null)
            GetComponent<Collider2D>().enabled = false;

        PlayerPrefs.SetInt("GolemDefeated", 1);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}