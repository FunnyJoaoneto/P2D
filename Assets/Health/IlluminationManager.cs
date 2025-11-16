using UnityEngine;

public enum GlobalIlluminationType { Day, Night }

public class IlluminationManager : MonoBehaviour
{
    public static IlluminationManager Instance;

    [Header("Global Lighting Mode")]
    public GlobalIlluminationType currentMode = GlobalIlluminationType.Day;

    private IlluminationZone[] zones;

    private void Awake()
    {
        Instance = this;
        zones = FindObjectsByType<IlluminationZone>(FindObjectsSortMode.None);
    }

    public bool IsDaytime => currentMode == GlobalIlluminationType.Day;
    public bool IsNighttime => currentMode == GlobalIlluminationType.Night;

    // 🔦 Check if a world position is considered bright (true) or dark (false)
    public bool IsPointBright(Vector2 point)
    {
        // Start from global setting
        bool bright = IsDaytime;

        // Override with any local zones that contain this point
        foreach (var zone in zones)
        {
            if (zone.Contains(point))
            {
                //Debug.Log($"Point {point} is inside zone {zone.name}");
                if (zone.isShadowZone) bright = false;
                if (zone.isLightZone)  bright = true;
            }
        }

        return bright;
    }
}
