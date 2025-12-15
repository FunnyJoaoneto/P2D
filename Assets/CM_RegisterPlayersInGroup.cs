using UnityEngine;
using Unity.Cinemachine;   // Cinemachine 3.x namespace

public class CM_RegisterPlayersInGroup : MonoBehaviour
{
    public CinemachineTargetGroup targetGroup;
    public string playerTag = "Player";
    public float defaultWeight = 1f;
    public float defaultRadius = 0.5f;

    private bool registered = false;

    private void Awake()
    {
        if (targetGroup == null)
            targetGroup = GetComponent<CinemachineTargetGroup>();
    }

    private void Update()
    {
        if (registered || targetGroup == null)
            return;

        var players = GameObject.FindGameObjectsWithTag(playerTag);
        if (players.Length < 2)
            return; // wait until both players exist

        // Clear any targets that might be set in the inspector
        targetGroup.Targets.Clear();

        // Add all players as members
        foreach (var go in players)
        {
            targetGroup.AddMember(go.transform, defaultWeight, defaultRadius);
        }

        Debug.Log($"CM_RegisterPlayersInGroup: added {players.Length} players to target group");
        registered = true;
    }
}
