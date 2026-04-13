using UnityEngine;

public class MovingUp : MonoBehaviour
{
    public float speed = 2f;
    public float maxDistance = 2f; // REDUCE THIS if it hits the ground!

    private Vector3 startPos;
    private int direction = 1; // 1 for Up, -1 for Down
    private Rigidbody2D rb;

    void Start()
    {
        startPos = transform.position;
        rb = GetComponent<Rigidbody2D>();

        // Ensure Kinematic settings
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.useFullKinematicContacts = true;
    }

    void FixedUpdate()
    {
        // 1. Move the platform
        rb.linearVelocity = new Vector2(0, speed * direction);

        // 2. BOUNCE BACK LOGIC
        // If the platform goes higher than start + distance, go down
        if (transform.position.y >= startPos.y + maxDistance)
        {
            direction = -1;
        }
        // If the platform goes lower than start - distance, go up
        else if (transform.position.y <= startPos.y - maxDistance)
        {
            direction = 1;
        }
    }

    // Stick player to platform
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }
}