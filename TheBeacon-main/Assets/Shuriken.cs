using UnityEngine;

public class Shuriken : MonoBehaviour
{
    public float spinSpeed = 500f;
    public float moveSpeed = 2f;
    public float moveDistance = 3f;
    public int damage = 10;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 1. Make it spin
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        // 2. Make it move back and forth (Left and Right)
        float x = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.position = new Vector3(startPos.x + x, startPos.y, startPos.z);
    }

    // 3. Damage the player on touch
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }
}