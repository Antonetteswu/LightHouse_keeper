using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private Demon bossDemon; // Drag your Demon object here in the Inspector
    [SerializeField] private GameObject visualIndicator; // Optional: A glow or arrow that appears when ready

    private void Update()
    {
        // Optional: Show a visual hint when the demon is dead
        if (bossDemon != null && bossDemon.isDead && visualIndicator != null)
        {
            visualIndicator.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the thing touching the stone is the Player
        if (collision.CompareTag("Player"))
        {
            // Check if the boss is assigned and dead
            if (bossDemon != null && bossDemon.isDead)
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.Log("The Demon is still alive! You cannot pass.");
                // You could trigger a UI message here
            }
        }
    }
}