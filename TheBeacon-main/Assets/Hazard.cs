using UnityEngine;

public class Hazard : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private float damageDelay = 1.0f;
    private float lastDamageTime;

    [Header("Animation Settings")]
    [SerializeField] private float waterAnimSpeed = 1.0f; // Control speed here!
    private Animator anim;

    void Start()
    {
        // Get the Animator component attached to the water
        anim = GetComponent<Animator>();

        if (anim != null)
        {
            anim.speed = waterAnimSpeed;
        }
    }

    private void Update()
    {
        // This allows you to change the speed in the Inspector while the game is running
        if (anim != null && anim.speed != waterAnimSpeed)
        {
            anim.speed = waterAnimSpeed;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Entity player = collision.GetComponent<Entity>();

            if (player != null && Time.time > lastDamageTime + damageDelay)
            {
                player.TakeDamage();
                lastDamageTime = Time.time;
            }
        }
    }
}