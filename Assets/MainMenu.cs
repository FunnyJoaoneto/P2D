using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button newGameButton;
    public Button continueButton;

    [Header("First level scene name")]
    public string firstLevelSceneName = "Level1";

    [Header("Selection UI Reference")]
    public MenuPlayerSelectUI selectionUI;

    public LevelSelectUI levelSelectUI;

    private void Start()
    {
        // Enable or disable Continue depending on whether save exists
        bool hasSave = SaveSystem.SaveExists();
        continueButton.interactable = hasSave;
        PlayerMode.SingleKeyboard = true; // Default to single keyboard mode
    }

    // Called by the New Game button OnClick
    public void OnClickNewGame()
    {
        // 1. Write player selections into PlayerSelectionData
        if (selectionUI != null)
            selectionUI.OnClickPlay();

        // Wipe old save (if any)
        SaveSystem.DeleteSave();

        // Create a fresh save, using the current player selection
        SaveData data = new SaveData();

        // Use your DontDestroyOnLoad singleton for character/control
        if (PlayerSelectionData.Instance != null)
        {
            data.p1Character = PlayerSelectionData.Instance.p1Character;
            data.p1Scheme    = PlayerSelectionData.Instance.p1Scheme;
            data.p2Character = PlayerSelectionData.Instance.p2Character;
            data.p2Scheme    = PlayerSelectionData.Instance.p2Scheme;
        }

        SaveSystem.Save(data);

        // Load first level
        SceneManager.LoadScene(firstLevelSceneName);
    }

    // Called by the Continue button OnClick
    public void OnClickContinue()
    {
        if (!SaveSystem.SaveExists())
            return;

        SaveData data = SaveSystem.Load();
        if (data == null)
            return;

        // Make sure there is a PlayerSelectionData object
        if (PlayerSelectionData.Instance == null)
        {
            GameObject go = new GameObject("PlayerSelectionData");
            var selection = go.AddComponent<PlayerSelectionData>();

            // Awake() in PlayerSelectionData will set Instance and DontDestroyOnLoad

            selection.p1Character = data.p1Character;
            selection.p1Scheme    = data.p1Scheme;
            selection.p2Character = data.p2Character;
            selection.p2Scheme    = data.p2Scheme;
        }
        else
        {
            var selection = PlayerSelectionData.Instance;
            selection.p1Character = data.p1Character;
            selection.p1Scheme    = data.p1Scheme;
            selection.p2Character = data.p2Character;
            selection.p2Scheme    = data.p2Scheme;
        }

        // -------------------------------

        int highest = data.highestUnlockedLevelIndex;

        // Safety check (avoid out-of-range)
        if (highest < 0 || highest >= levelSelectUI.allLevels.Count)
            highest = 0;

        string sceneToLoad = levelSelectUI.allLevels[highest].sceneName;

        SceneManager.LoadScene(sceneToLoad);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
