using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints; // assign in Inspector
    private int nextSpawnIndex = 0;

    private void OnEnable()
    {
        var pim = GetComponent<PlayerInputManager>();
        pim.onPlayerJoined += OnPlayerJoined;

        Debug.Log("Available control schemes: " + string.Join(", ", pim.playerPrefab.GetComponent<PlayerInput>().actions.controlSchemes));
    }

    private void OnDisable()
    {
        var pim = GetComponent<PlayerInputManager>();
        pim.onPlayerJoined -= OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        Debug.Log($"Player joined: {player.name}, device: {player.devices.Count} devices");
        Debug.Log($"{player.name} joined with control scheme: {player.currentControlScheme}");

        // ✅ Set spawn position
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawn = spawnPoints[nextSpawnIndex % spawnPoints.Length];
            player.transform.position = spawn.position;

            var respawn = player.GetComponent<PlayerRespawn>();
            if (respawn != null)
                respawn.SetSpawnPoint(spawn.position);
        }

        // ✅ Assign a color
        var renderer = player.GetComponentInChildren<SpriteRenderer>();
        if (renderer != null)
        {
            Color[] colors = { Color.red, Color.blue, Color.green, Color.yellow };
            renderer.color = colors[nextSpawnIndex % colors.Length];
        }

        // ✅ Prevent collisions between all players
        var playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            var allPlayers = Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
            foreach (var other in allPlayers)
            {
                if (other != player)
                {
                    var otherCollider = other.GetComponent<Collider2D>();
                    if (otherCollider != null)
                    {
                        Physics2D.IgnoreCollision(playerCollider, otherCollider, true);
                    }
                }
            }
        }

        nextSpawnIndex++;
    }
}
