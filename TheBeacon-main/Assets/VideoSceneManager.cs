using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoSceneManager : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        // This tells the script to run a function when the video ends
        videoPlayer.loopPointReached += LoadNextScene;
    }

    void Update()
    {
        // Optional: Allow the player to skip the video by pressing Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadNextScene(videoPlayer);
        }
    }

    void LoadNextScene(VideoPlayer vp)
    {
        // Loads the next scene in your Build Settings
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}