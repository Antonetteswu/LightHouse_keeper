using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneStone : MonoBehaviour
{
    // Fix: Removed the space in the name
    public string finalBossSceneName;

    void Start()
    {
        // Reset the win condition when the level starts
        PlayerPrefs.SetInt("GolemDefeated", 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            bool canProceed = PlayerPrefs.GetInt("GolemDefeated") == 1;

            if (canProceed)
            {
                // Fix: Match the variable name here too
                SceneManager.LoadScene(finalBossSceneName);
            }
            else
            {
                Debug.Log("The Golem is still guarding this path!");
            }
        }
    }
}