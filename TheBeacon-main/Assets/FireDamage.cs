using UnityEngine;

public class FireDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageDelay = 1.0f;
    private float lastDamageTime;

    [Header("Animation Settings")]
    [SerializeField] private float fireanimSpeed = 1.0f;
    private Animator anim;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource fireBurnSfx; // Drag a "Hurt/Sizzle" sound here

    void Start()
    {
        anim = GetComponent<Animator>();
        if (anim != null) anim.speed = fireanimSpeed;
    }

    private void Update()
    {
        if (anim != null && anim.speed != fireanimSpeed)
        {
            anim.speed = fireanimSpeed;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Entity player = collision.GetComponent<Entity>();

            if (player != null && Time.time > lastDamageTime + damageDelay)
            {
                player.TakeDamage(damageAmount); // Ensure you pass damageAmount
                lastDamageTime = Time.time;

                // --- PLAY SOUND WHEN PLAYER IS DAMAGED ---
                if (fireBurnSfx != null)
                {
                    fireBurnSfx.Play();
                }
            }
        }
    }
}