using UnityEngine;

public class CameraWallFollower : MonoBehaviour
{
    public Camera cam;
    public float thickness = 1f; // how thick the walls should be

    public Transform leftWall;
    public Transform rightWall;
    public Transform topWall;
    public Transform bottomWall;

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void LateUpdate()
    {
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // Positions
        float left = -camWidth / 2f;
        float right = camWidth / 2f;
        float top = camHeight / 2f;
        float bottom = -camHeight / 2f;

        // Move children (local space!)
        leftWall.localPosition = new Vector3(left - thickness / 2f, 0, 0);
        rightWall.localPosition = new Vector3(right + thickness / 2f, 0, 0);
        topWall.localPosition = new Vector3(0, top + thickness / 2f, 0);
        bottomWall.localPosition = new Vector3(0, bottom - thickness / 2f, 0);

        // Resize colliders
        leftWall.GetComponent<BoxCollider2D>().size =
            new Vector2(thickness, camHeight);
        rightWall.GetComponent<BoxCollider2D>().size =
            new Vector2(thickness, camHeight);
        topWall.GetComponent<BoxCollider2D>().size =
            new Vector2(camWidth, thickness);
        bottomWall.GetComponent<BoxCollider2D>().size =
            new Vector2(camWidth, thickness);
    }
}
