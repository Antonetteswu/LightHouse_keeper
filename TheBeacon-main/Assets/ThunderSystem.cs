using UnityEngine;
using UnityEngine.Rendering.Universal; // Required for Light 2D

public class ThunderSystem : MonoBehaviour
{
    public Light2D lightningLight;
    public AudioSource thunderSound;
    public float minTime = 5f;
    public float maxTime = 15f;

    void Start()
    {
        // Start the loop
        StartCoroutine(ThunderLoop());
    }

    System.Collections.IEnumerator ThunderLoop()
    {
        while (true)
        {
            // Wait for a random amount of time
            yield return new WaitForSeconds(Random.Range(minTime, maxTime));

            // 1. The Flash (Lightning)
            lightningLight.intensity = 1.5f;

            // 2. The Sound (Thunder)
            // Real thunder usually happens slightly after the flash!
            yield return new WaitForSeconds(0.2f);
            if (thunderSound != null) thunderSound.Play();

            // 3. Turn the light back down
            yield return new WaitForSeconds(0.1f);
            lightningLight.intensity = 0.2f; // Set this to your normal "dark" intensity
        }
    }
}