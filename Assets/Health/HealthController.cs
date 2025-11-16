using UnityEngine;
using System;

public class HealthController : MonoBehaviour
{
    [Header("Health Settings")]
    public float currentHealth = 100f;
    public float maximumHealth = 100f;
    public float RemainingHealthPercentage => currentHealth / maximumHealth;
    public bool isAlive => currentHealth > 0;

    public event Action<float, float> OnHealthChanged;
    public event Action OnDeath;

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
        if (currentHealth <= 0)
            Die();
    }

    public void AddHealth(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maximumHealth)
            currentHealth = maximumHealth;

        NotifyHealthChanged(); // <-- ESSENCIAL para a UI
    }

    public void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maximumHealth);
    }

    private void Die()
    {
        if (currentHealth > 0) return;

        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} has died!");
    }
}