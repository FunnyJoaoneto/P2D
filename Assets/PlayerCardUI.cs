using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCardUI : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private Image characterImage;
    public Sprite lightSprite;
    public Sprite nightSprite;
    [SerializeField] private TextMeshProUGUI characterLabel;

    [Header("Scheme")]
    [SerializeField] private TextMeshProUGUI schemeLabel;
    public int PlayerId { get; private set; } = 1;

    // 0 = Light, 1 = Night
    public int CharacterIndex { get; private set; } = 0;
    // 0 = WASD, 1 = Arrows
    public int SchemeIndex { get; private set; } = 0;

    public string CharacterId => (CharacterIndex == 0) ? "LightGuy" : "NightGirl";
    public string SchemeId    => (SchemeIndex == 0) ? "KeyboardWASD" : "KeyboardArrows";

    public System.Action<PlayerCardUI> OnChanged; // fired on any change

    public void Init(int playerId)
    {
        PlayerId = playerId;

        // Default choices:
        // P1 → Light + WASD
        // P2 → Night + Arrows
        if (PlayerId == 1)
        {
            CharacterIndex = 0; // Light
            SchemeIndex = 0;    // WASD
        }
        else
        {
            CharacterIndex = 1; // Night
            SchemeIndex = 1;    // Arrows
        }

        RefreshAll();
    }

    // ------------------------------------------------------
    // FIX: These must also notify the Menu that values changed
    // ------------------------------------------------------
    public void SetCharacter(int index)
    {
        CharacterIndex = index;
        RefreshCharacter();
        OnChanged?.Invoke(this);     // <--- IMPORTANT
    }

    public void SetScheme(int index)
    {
        SchemeIndex = index;
        RefreshScheme();
        OnChanged?.Invoke(this);     // <--- IMPORTANT
    }
    // ------------------------------------------------------

    void ChangeCharacter(int dir)
    {
        CharacterIndex = (CharacterIndex + dir + 2) % 2;
        RefreshCharacter();
        OnChanged?.Invoke(this);
    }

    void ChangeScheme()
    {
        SchemeIndex = (SchemeIndex + 1) % 2;
        RefreshScheme();
        OnChanged?.Invoke(this);
    }

    public void ForceNextCharacter()
    {
        ChangeCharacter(+1);
    }

    public void ForceNextScheme()
    {
        ChangeScheme();
    }

    void RefreshAll()
    {
        RefreshCharacter();
        RefreshScheme();
    }

    void RefreshCharacter()
    {
        if (CharacterIndex == 0)
        {
            characterImage.sprite = lightSprite;
            characterLabel.text = "Light";
        }
        else
        {
            characterImage.sprite = nightSprite;
            characterLabel.text = "Night";
        }
    }

    void RefreshScheme()
    {
        schemeLabel.text = SchemeIndex == 0 ? "Keyboard: WASD" : "Keyboard: Arrows";
    }
}
