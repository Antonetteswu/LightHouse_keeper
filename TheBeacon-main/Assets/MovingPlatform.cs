using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 2f;
    public float maxDistance = 3f;

    // FIX: Define these variables so the script knows what they are
    [SerializeField] private LayerMask whatisGround;
    [SerializeField] private float rayDistance = 1.0f;

    private Vector3 startPos;
    private int direction = 1;
    private Rigidbody2D rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
    }

    void FixedUpdate()
    {
        // 1. Move the platform (Physics-based velocity)
        rb.linearVelocity = new Vector2(speed * direction, 0);

        // 2. Look ahead for a wall (The Raycast)
        Vector2 rayDirection = (direction == 1) ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, rayDistance, whatisGround);

        // 3. If the ray hits a solid platform (Wall), flip direction
        if (hit.collider != null && !hit.collider.CompareTag("Player"))
        {
            direction *= -1;
            // Slightly nudge position so it doesn't get stuck in the wall
            transform.position += new Vector3(direction * 0.05f, 0, 0);
        }

        // 4. Distance-based fallback (Normal patrol)
        if (transform.position.x >= startPos.x + maxDistance && direction == 1)
            direction = -1;
        else if (transform.position.x <= startPos.x - maxDistance && direction == -1)
            direction = 1;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we hit the Ground layer
        if (((1 << collision.gameObject.layer) & whatisGround) != 0)
        {
            // 1. Flip direction
            direction *= -1;

            // 2. IMPORTANT: Move it slightly away from the wall immediately 
            // This prevents the platform from getting stuck inside the wall's collider
            transform.position += new Vector3(direction * 0.1f, 0, 0);

            // 3. Optional: Reset the startPos if you want the distance 
            // to count from the wall instead of the original spawn point
            startPos = transform.position;
        }
    }
}