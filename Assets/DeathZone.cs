using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"DeathZone triggered by {other.name} (tag={other.tag})");

        // Use a generic player tag; make sure the player prefab is tagged appropriately
        if (other.CompareTag("Player"))
        {
            var respawn = other.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                Debug.Log($"Respawning {other.name}");
                respawn.Respawn();
            }
            else
            {
                Debug.LogWarning($"No PlayerRespawn found on {other.name}");
            }
        }
    }
}
