using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    private Vector3 spawnPoint;

    // Called by PlayerSpawnManager after the player is spawned
    public void SetSpawnPoint(Vector3 point)
    {
        spawnPoint = point;
        // optional: set the transform immediately if you want start there
        // transform.position = spawnPoint;
    }

    public void Respawn()
    {
        // Stop any velocity and reset position
        if (TryGetComponent<Rigidbody2D>(out var rb))
            rb.linearVelocity = Vector2.zero;   // <-- correct API for Rigidbody2D

        transform.position = spawnPoint;
    }
}
