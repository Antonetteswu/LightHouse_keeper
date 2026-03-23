using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    private void Awake()
    {
        // If you forgot to drag them in, this tries to find them for you
        if (slider == null) slider = GetComponent<Slider>();

        // If fill is still null, try to find it in the children (standard Slider path)
        if (fill == null && slider != null)
        {
            // Typical path: Slider -> Fill Area -> Fill
            fill = slider.fillRect.GetComponent<Image>();
        }
    }

    public void SetHealth(int health)
    {
        if (slider == null || fill == null) return; // Safety check

        slider.value = health;

        if (gradient != null)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealth(int health)
    {
        if (slider == null || fill == null) return; // Safety check

        slider.maxValue = health;
        slider.value = health;

        if (gradient != null)
            fill.color = gradient.Evaluate(1f);
    }
}
