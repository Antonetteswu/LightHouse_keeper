using UnityEngine;

public class FanJump : MonoBehaviour
{
    public float fanForce = 20f; // How high the fan pushes you
    public string jumpTriggerName = "Jump"; // The name of your parameter in the Animator

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            Animator anim = other.GetComponent<Animator>();

            if (rb != null)
            {
                // Reset velocity so the fan push is consistent
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

                // Push the player up
                rb.AddForce(Vector2.up * fanForce, ForceMode2D.Impulse);
            }

            if (anim != null)
            {
                // Trigger the jump animation
                anim.SetTrigger(jumpTriggerName);
            }
        }
    }
}