using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Image healthBarForegroundImage;
    public HealthController targetHealth;

    private void OnEnable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(targetHealth.currentHealth, targetHealth.maximumHealth);
            //Debug.Log("HealthBarUI: Subscribed to OnHealthChanged event.");
            //Debug.Log($"HealthBar linked to {targetHealth.name}");
        }
    }

    public void SetTarget(HealthController newTarget)
    {
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= UpdateHealthBar;

        targetHealth = newTarget;

        if (targetHealth != null)
        {
            Debug.Log($"HealthBar linked to {targetHealth.name}");
            targetHealth.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(targetHealth.currentHealth, targetHealth.maximumHealth);
            //Debug.Log("HealthBarUI: Target changed and subscribed to OnHealthChanged event.");
        }
    }

    private void OnDisable()
    {
        Debug.Log("HealthBarUI: Unsubscribing from OnHealthChanged event.");
        if (targetHealth != null)
            targetHealth.OnHealthChanged -= UpdateHealthBar;
    }

    private void UpdateHealthBar(float current, float max)
    {
        Debug.Log($"Updating health bar: {current}/{max}");
        healthBarForegroundImage.fillAmount = current / max;
    }
}
