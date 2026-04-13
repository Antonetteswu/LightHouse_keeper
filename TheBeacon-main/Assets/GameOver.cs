using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // This allows other scripts to find this one easily
    public static GameOver instance;

    [SerializeField] private GameObject gameOverPanel;

    void Awake()
    {
        // Setup the Singleton
        if (instance == null) instance = this;
    }

    public void GameOverPanel()
    {
        gameOverPanel.SetActive(true);
        // Optional: Pause the game when the panel shows
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        // Reset time before restarting
        Time.timeScale = 1;
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(sceneIndex);
    }
}