using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
  
     public void PlayGame()
    {
        SceneManager.LoadScene("Story"); // Added the space here
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}



