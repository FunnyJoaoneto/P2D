using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class CameraFollowsPlayers : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    private Transform[] playerTransforms;

    void LateUpdate()
    {
        // Find players (can be cached if you prefer performance)
        var players = Object.FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        if (players.Length == 0) return;

        // Cache their transforms
        playerTransforms = players.Select(p => p.transform).ToArray();

        // Calculate center point
        Vector3 center = Vector3.zero;
        foreach (var t in playerTransforms)
            center += t.position;
        center /= playerTransforms.Length;

        // Smooth follow
        Vector3 desired = center + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
