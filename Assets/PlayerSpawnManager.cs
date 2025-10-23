using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Player Prefabs")]
    [SerializeField] private GameObject lightGuyPrefab;
    [SerializeField] private GameObject nightGirlPrefab;

    [Header("Health Bars")]
    [SerializeField] private HealthBarUI lightBar;
    [SerializeField] private HealthBarUI nightBar;

    private int nextSpawnIndex = 0;
    private int nextPrefabIndex = 0;

    private PlayerInputManager pim;

    private void Awake()
    {
        pim = GetComponent<PlayerInputManager>();
    }

    private void OnEnable()
    {
        pim.onPlayerJoined += OnPlayerJoined;
        pim.onPlayerLeft += OnPlayerLeft;

        // Set the first prefab to use
        pim.playerPrefab = GetNextPrefab();
    }

    private void OnDisable()
    {
        pim.onPlayerJoined -= OnPlayerJoined;
        pim.onPlayerLeft -= OnPlayerLeft;
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        Debug.Log($"✅ Player joined: {player.name} ({player.currentControlScheme})");

        // Set spawn point
        if (spawnPoints.Length > 0)
        {
            Transform spawn = spawnPoints[nextSpawnIndex % spawnPoints.Length];
            player.transform.position = spawn.position;
            nextSpawnIndex++;

            var respawn = player.GetComponent<PlayerRespawn>();
            if (respawn != null)
                respawn.SetSpawnPoint(spawn.position);
        }

        // Ignore collisions with other players
        var playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            foreach (var other in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
            {
                if (other != player)
                {
                    var otherCollider = other.GetComponent<Collider2D>();
                    if (otherCollider != null)
                        Physics2D.IgnoreCollision(playerCollider, otherCollider, true);
                }
            }
        }

        var hc = player.GetComponent<HealthController>();
        if (hc != null)
        {
            HealthBarUI targetBar = null;

            if (player.name.Contains("LightGuy"))
                targetBar = lightBar;
            else if (player.name.Contains("NightGirl"))
                targetBar = nightBar;

            if (targetBar != null)
            {
                targetBar.SetTarget(hc);
                Debug.Log($"Linked {targetBar.name} to {player.name}");
            }
            else
            {
                Debug.LogWarning($"No HealthBar found for {player.name}");
            }
            // Subscribe to death event
            hc.OnDeath += HandlePlayerDeath;
        }
        else
        {
            Debug.LogWarning($"No HealthController found on {player.name}");
        }

            // Prepare next prefab for next player that joins
            pim.playerPrefab = GetNextPrefab();
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("💀 A player has died! Respawning both players...");

        // optional: wait a bit before respawning (like 2 seconds)
        Invoke(nameof(RespawnAllPlayers), 2f);
    }

    private void RespawnAllPlayers()
    {
        foreach (var player in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
        {
            var respawn = player.GetComponent<PlayerRespawn>();
            var health = player.GetComponent<HealthController>();

            if (respawn != null)
                respawn.Respawn();

            if (health != null)
            {
                health.AddHealth(health.maximumHealth); // restore full HP
            }
        }
    }

    private void OnPlayerLeft(PlayerInput player)
    {
        Debug.Log($"❌ Player left: {player.name}");
    }

    private GameObject GetNextPrefab()
    {
        // Alternate or choose based on your logic
        GameObject prefab = (nextPrefabIndex % 2 == 0) ? lightGuyPrefab : nightGirlPrefab;
        nextPrefabIndex++;
        return prefab;
    }
}
