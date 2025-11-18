using UnityEngine;

public class PlayerSelectionData : MonoBehaviour
{
    public static PlayerSelectionData Instance;

    [Header("Player 1 Selection")]
    public string p1Character;   // "LightGuy" or "NightGirl"
    public string p1Scheme;      // "KeyboardWASD" or "KeyboardArrows"

    [Header("Player 2 Selection")]
    public string p2Character;   // must be different
    public string p2Scheme;      // must be different

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
