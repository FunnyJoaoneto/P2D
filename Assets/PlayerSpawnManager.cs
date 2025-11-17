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
        if (!PlayerMode.SingleKeyboard)
        {
            Debug.Log("Subscribing to PlayerInputManager events");
            pim.onPlayerJoined += OnPlayerJoined;
            pim.onPlayerLeft += OnPlayerLeft;
            pim.playerPrefab = GetNextPrefab(); // first prefab
        }
    }

    private void OnDisable()
    {
        if (!PlayerMode.SingleKeyboard)
        {
            pim.onPlayerJoined -= OnPlayerJoined;
            pim.onPlayerLeft -= OnPlayerLeft;
        }
    }

    private void Start()
    {
        //if (PlayerMode.SingleKeyboar)
        //{
            Debug.Log("Spawning players in Single Keyboard mode");
            // Disable join-on-press flow
            pim.enabled = false;

            // Spawn two players immediately with different keyboard schemes
            SpawnFromMenuSelections();
        //}
    }

    // -------- Single Keyboard path --------
    private void SpawnFromMenuSelections()
    {
        var data = PlayerSelectionData.Instance;

        // Detect which selection is Light and which is Night
        bool p1IsLight = data.p1Character == "LightGuy";
        bool p2IsLight = data.p2Character == "LightGuy";

        // Choose prefabs
        GameObject lightPrefab = lightGuyPrefab;
        GameObject nightPrefab = nightGirlPrefab;

        // Prepare spawn variables
        GameObject firstPrefab;
        string firstScheme;
        GameObject secondPrefab;
        string secondScheme;

        if (p1IsLight)
        {
            // P1 = Light, P2 = Night
            firstPrefab = lightPrefab;
            firstScheme = data.p1Scheme;

            secondPrefab = nightPrefab;
            secondScheme = data.p2Scheme;
        }
        else
        {
            // P2 = Light, P1 = Night
            firstPrefab = lightPrefab;
            firstScheme = data.p2Scheme;

            secondPrefab = nightPrefab;
            secondScheme = data.p1Scheme;
        }

        Debug.Log($"Spawning players: 1st={firstPrefab.name}({firstScheme}), 2nd={secondPrefab.name}({secondScheme})");

        // --- Spawn Light Player first ---
        var pLight = PlayerInput.Instantiate(
            firstPrefab,
            controlScheme: firstScheme,
            pairWithDevice: Keyboard.current
        );
        SetupPlayer(pLight);

        // --- Spawn Night Player second ---
        var pNight = PlayerInput.Instantiate(
            secondPrefab,
            controlScheme: secondScheme,
            pairWithDevice: Keyboard.current
        );
        SetupPlayer(pNight);
    }
    // -------- Two Devices path (existing) --------
    private void OnPlayerJoined(PlayerInput player)
    {
        Debug.Log($"✅ Player joined: {player.name} ({player.currentControlScheme})");
        // Position and spawnpoint assign
        PlaceAtNextSpawn(player);

        // Collision ignore and UI hookup
        PostJoinWiring(player);

        // Prepare next prefab for the next join
        if (pim) pim.playerPrefab = GetNextPrefab();
    }

    private void OnPlayerLeft(PlayerInput player)
    {
        Debug.Log($"❌ Player left: {player.name}");
    }

    // -------- Shared helpers --------
    private Vector3 GetNextSpawn()
    {
        if (spawnPoints.Length == 0) return Vector3.zero;
        var spawn = spawnPoints[nextSpawnIndex % spawnPoints.Length].position;
        nextSpawnIndex++;
        return spawn;
    }

    private void PlaceAtNextSpawn(PlayerInput player)
    {
        var pos = GetNextSpawn();
        player.transform.position = pos;

        var respawn = player.GetComponent<PlayerRespawn>();
        if (respawn != null) respawn.SetSpawnPoint(pos);
    }

    private void SetupPlayer(PlayerInput player)
    {
        PlaceAtNextSpawn(player);
        PostJoinWiring(player);
    }

    private void PostJoinWiring(PlayerInput player)
    {
        // Ignore collisions between players
        var playerCollider = player.GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            foreach (var other in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
            {
                if (other == player) continue;
                var otherCol = other.GetComponent<Collider2D>();
                if (otherCol != null) Physics2D.IgnoreCollision(playerCollider, otherCol, true);
            }
        }

        // Health bar link and death handling
        var hc = player.GetComponent<HealthController>();
        if (hc != null)
        {
            HealthBarUI targetBar = null;
            if (player.name.Contains("LightGuy")) targetBar = lightBar;
            else if (player.name.Contains("NightGirl")) targetBar = nightBar;

            if (targetBar != null) targetBar.SetTarget(hc);
            hc.OnDeath -= HandlePlayerDeath;
            hc.OnDeath += HandlePlayerDeath;
        }
    }

    private void HandlePlayerDeath()
    {
        Invoke(nameof(RespawnAllPlayers), 2f);
    }

    private void RespawnAllPlayers()
    {
        foreach (var player in FindObjectsByType<PlayerInput>(FindObjectsSortMode.None))
        {
            var respawn = player.GetComponent<PlayerRespawn>();
            var health = player.GetComponent<HealthController>();
            if (respawn != null) respawn.Respawn();
            if (health != null) health.AddHealth(health.maximumHealth);
        }
    }

    private GameObject GetNextPrefab()
    {
        var prefab = (nextPrefabIndex % 2 == 0) ? lightGuyPrefab : nightGirlPrefab;
        nextPrefabIndex++;
        return prefab;
    }
}
