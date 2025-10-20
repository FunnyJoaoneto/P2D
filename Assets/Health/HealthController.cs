using UnityEngine;
using System;

public class HealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public float currentHealth = 100f;
    public float maximumHealth = 100f;
    public float RemainingHealthPercentage => currentHealth / maximumHealth;

    public event Action<float, float> OnHealthChanged;
    // args: (currentHealth, maximumHealth)

    private void Awake()
    {
        currentHealth = maximumHealth;
        NotifyHealthChanged();
    }

    public void TakeDamage(float damageAmount)
    {
        Debug.Log($"Taking damage: {damageAmount}");
        Debug.Log($"Health before damage: {currentHealth}");
        currentHealth -= damageAmount;
        Debug.Log($"Health after damage: {currentHealth}");
        if (currentHealth < 0)
            currentHealth = 0;
        NotifyHealthChanged();
    }

    public void AddHealth(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maximumHealth)
            currentHealth = maximumHealth;
        NotifyHealthChanged();
    }

    public void NotifyHealthChanged()
    {
        Debug.Log("Notifying health change.");
        OnHealthChanged?.Invoke(currentHealth, maximumHealth);
        Debug.Log($"Current Listneners: {OnHealthChanged?.GetInvocationList().Length ?? 0}");
    }
}
