using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    public Button button;
    public TMP_Text levelNameText;
    public TMP_Text bestTimeText;

    private string sceneName;

    public void Setup(string sceneName, string displayName, bool unlocked, float bestTime)
    {
        this.sceneName = sceneName;
        levelNameText.text = displayName;

        bestTimeText.text = bestTime < 0 ? "-" : $"{bestTime:0.0}s";

        button.interactable = unlocked;
    }

    public void OnClick()
    {
        FindFirstObjectByType<LevelSelectUI>().OnLevelButtonClicked(sceneName);
    }
}
