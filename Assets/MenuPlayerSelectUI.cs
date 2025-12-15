using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPlayerSelectUI : MonoBehaviour
{
    public PlayerCardUI p1;
    public PlayerCardUI p2;

    public void Start()
    {
        // Initialize defaults:
        p1.Init(1); // Light + WASD
        p2.Init(2); // Night + Arrows

        // Subscribe to changes
        p1.OnChanged += OnCardChanged;
        p2.OnChanged += OnCardChanged;
    }

    void OnCardChanged(PlayerCardUI card)
    {
        // Always enforce unique characters and schemes
        if (p1.CharacterIndex == p2.CharacterIndex)
            p2.ForceNextCharacter();

        if (p1.SchemeIndex == p2.SchemeIndex)
            p2.ForceNextScheme();
    }

    // --------------------
    // Swap CHARACTERS
    // --------------------
    public void OnSwapCharacters()
    {
        int temp = p1.CharacterIndex;
        p1.SetCharacter(p2.CharacterIndex);
        p2.SetCharacter(temp);
    }

    // --------------------
    // Swap SCHEMES
    // --------------------
    public void OnSwapSchemes()
    {
        int temp = p1.SchemeIndex;
        p1.SetScheme(p2.SchemeIndex);
        p2.SetScheme(temp);
    }

    public void OnClickPlay()
    {
        var data = PlayerSelectionData.Instance;

        data.p1Character = p1.CharacterId;
        data.p1Scheme    = p1.SchemeId;

        data.p2Character = p2.CharacterId;
        data.p2Scheme    = p2.SchemeId;
    }
}
