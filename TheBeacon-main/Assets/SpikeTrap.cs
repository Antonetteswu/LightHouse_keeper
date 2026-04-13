using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [SerializeField] private int damageAmount = 2;

    // This function runs automatically when something enters the Trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Entity entity = collision.GetComponent<Entity>();

        if (entity != null)
        {
            entity.TakeDamage();

            // Add a little 'bounce' upwards and away
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 10f);
            }
        }
    }
}