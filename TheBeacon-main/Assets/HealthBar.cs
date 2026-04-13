using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    // NEW: Variables to track actual health numbers
    [SerializeField] private int maxHealth = 10;
    private int currentHealth;

    private void Awake()
    {
        if (slider == null) slider = GetComponent<Slider>();
        if (fill == null && slider != null)
        {
            fill = slider.fillRect.GetComponent<Image>();
        }
    }

    private void Start()
    {
        // Initialize health when the game starts
        currentHealth = maxHealth;
        SetMaxHealth(maxHealth);
    }

    // NEW: The function your Skeleton is looking for!
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Update the UI slider
        SetHealth(currentHealth);

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        // Add your death logic here (e.g., reload scene or play animation)
    }

    public void SetHealth(int health)
    {
        if (slider == null) return;
        slider.value = health;

        if (gradient != null && fill != null)
            fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    public void SetMaxHealth(int health)
    {
        if (slider == null) return;

        slider.maxValue = health;
        slider.value = health;

        if (gradient != null && fill != null)
            fill.color = gradient.Evaluate(1f);
    }
}