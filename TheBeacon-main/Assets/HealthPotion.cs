using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [SerializeField] private int healAmount = 5;
    [SerializeField] private AudioSource healSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Entity player = collision.GetComponent<Entity>();

            if (player != null && !player.isDead)
            {
                if (player.currentHealth < player.maxHealth)
                {
                    player.currentHealth = Mathf.Min(player.currentHealth + healAmount, player.maxHealth);

                    // --- THE MISSING LINK ---
                    // This tells the player script to refresh the green bar!
                    if (player.healthBar != null)
                    {
                        player.healthBar.SetHealth(player.currentHealth);
                    }

                    if (healSound != null)
                    {
                        AudioSource.PlayClipAtPoint(healSound.clip, transform.position);
                    }

                    Debug.Log("Potion Picked Up! UI Updated.");
                    Destroy(gameObject);
                }
            }
        }
    }
}