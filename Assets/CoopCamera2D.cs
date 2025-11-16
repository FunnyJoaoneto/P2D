using UnityEngine;

public class CoopCamera2D : MonoBehaviour
{
    [Header("References")]
    public Camera cam;  // Drag Main Camera here (optional)

    [Header("Zoom settings")]
    public float minSize = 5f;
    public float maxSize = 12f;
    public float forward = 3f;  // How much space in front/behind each player

    [Header("Smoothing")]
    [Range(0f, 1f)]
    public float verticalLerp = 0.05f;
    [Range(0f, 1f)]
    public float horizontalLerp = 0.15f; // new: smooth X to avoid jumps

    [Header("Wall Locks")]
    public CameraWall leftWallLock;
    public CameraWall rightWallLock;

    private Transform p1;
    private Transform p2;
    private float targetY;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;

        cam.orthographic = true;
    }

    void LateUpdate()
    {
        // Make sure we have both players
        if (p1 == null || p2 == null)
        {
            FindPlayers();
            if (p1 == null || p2 == null)
                return;

            Vector3 mid = (p1.position + p2.position) * 0.5f;
            targetY = mid.y;
        }

        Vector3 pos1 = p1.position;
        Vector3 pos2 = p2.position;

        // -------------------------------------------------
        // HORIZONTAL CAMERA POSITION (midpoint + wall lock)
        // -------------------------------------------------

        float currentX = cam.transform.position.x;
        float desiredX = (pos1.x + pos2.x) * 0.5f; // ideal center between players

        // If someone is touching the left wall, do NOT let the camera move further left
        if (leftWallLock != null && leftWallLock.playerTouching && desiredX < currentX)
        {
            desiredX = currentX;
        }

        // If someone is touching the right wall, do NOT let the camera move further right
        if (rightWallLock != null && rightWallLock.playerTouching && desiredX > currentX)
        {
            desiredX = currentX;
        }

        // Smooth horizontal movement to avoid any sudden jumps
        float finalX = Mathf.Lerp(currentX, desiredX, horizontalLerp);

        // -------------------------------------------------
        // CAMERA ZOOM (based on players + forward)
        // -------------------------------------------------

        // Space we want to see, including "forward" in front and behind each player
        float leftBound  = Mathf.Min(pos1.x - forward, pos2.x - forward);
        float rightBound = Mathf.Max(pos1.x + forward, pos2.x + forward);
        float requiredWidth = rightBound - leftBound;

        float aspect = cam.aspect; // 1920/1080 = 1.777...
        float desiredSize = requiredWidth / (2f * aspect);
        cam.orthographicSize = Mathf.Clamp(desiredSize, minSize, maxSize);

        // -------------------------------------------------
        // VERTICAL CAMERA (SMOOTH)
        // -------------------------------------------------

        float midY = (pos1.y + pos2.y) * 0.5f;
        targetY = Mathf.Lerp(targetY, midY, verticalLerp);

        // -------------------------------------------------
        // APPLY FINAL CAMERA POSITION
        // -------------------------------------------------

        cam.transform.position = new Vector3(
            finalX,
            targetY,
            cam.transform.position.z
        );
    }

    private void FindPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (players.Length >= 2)
        {
            p1 = players[0].transform;
            p2 = players[1].transform;
        }
    }
}
