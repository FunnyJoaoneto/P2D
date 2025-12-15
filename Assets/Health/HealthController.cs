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
    // args: (currentHealth, maximumHealth)

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
        NotifyHealthChanged();
    }

    /// <summary>
    /// Instantly kills the character (used by death zones, spikes, etc.).
    /// </summary>
    public void KillInstantly()
    {
        if (!isAlive) return;

        currentHealth = 0f;
        NotifyHealthChanged();
        Die();
    }

    public void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maximumHealth);
        //Debug.Log($"Current Listneners: {OnHealthChanged?.GetInvocationList().Length ?? 0}");
    }

    private void Die()
    {
        if (isAlive) return; // prevents multiple triggers
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} has died!");
    }
}
