using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class IlluminationZone : MonoBehaviour
{
    public bool isShadowZone = false;
    public bool isLightZone = false;

    private Collider2D col;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    // Check if a given position is inside this zone
    public bool Contains(Vector2 point)
    {
        return col != null && col.bounds.Contains(point);
    }
}
