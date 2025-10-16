using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Player Prefabs")]
    [SerializeField] private GameObject lightGuyPrefab;
    [SerializeField] private GameObject nightGirlPrefab;

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

        // Prepare next prefab for next player that joins
        pim.playerPrefab = GetNextPrefab();
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
