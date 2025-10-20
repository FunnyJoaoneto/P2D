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
        currentHealth -= damageAmount;
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
        OnHealthChanged?.Invoke(currentHealth, maximumHealth);
        //Debug.Log($"Current Listneners: {OnHealthChanged?.GetInvocationList().Length ?? 0}");
    }
}
