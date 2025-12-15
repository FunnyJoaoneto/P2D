using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class InstantDeath : MonoBehaviour
{
    public string playerTag = "Player";
    public float activationDelay = 1.0f;

    private bool active = false;
    private float timer = 0f;

    private void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (active) return;

        timer += Time.deltaTime;
        if (timer >= activationDelay)
            active = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;                   // ignore before activation

        if (!other.CompareTag(playerTag))
            return;

        var health = other.GetComponent<HealthController>();
        if (health != null)
        {
            Debug.Log($"InstantDeath: {other.name} fell below the camera, killing instantly.");
            health.KillInstantly();
        }
    }
}
