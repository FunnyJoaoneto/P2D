using UnityEngine;

public class CameraBounds2D : MonoBehaviour
{
    public Camera cam;
    public float padding = 0.7f; // space between player and screen edge

    void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    public Vector2 Clamp(Vector2 pos)
    {
        float camSize = cam.orthographicSize;
        float camWidth = camSize * cam.aspect;

        float left   = cam.transform.position.x - camWidth   + padding;
        float right  = cam.transform.position.x + camWidth   - padding;
        float bottom = cam.transform.position.y - camSize    + padding;
        float top    = cam.transform.position.y + camSize    - padding;

        return new Vector2(
            Mathf.Clamp(pos.x, left, right),
            Mathf.Clamp(pos.y, bottom, top)
        );
    }
}
