using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class LevelAutoSaver : MonoBehaviour
{
    void Start()
    {
        // If there is no save yet (somehow), create one using current
        SaveData data = SaveSystem.Load();

        if (data == null)
        {
            data = new SaveData();
        }

        // Make sure character/control choices from PlayerSelectionData
        // are stored in the save as well.
        if (PlayerSelectionData.Instance != null)
        {
            data.p1Character = PlayerSelectionData.Instance.p1Character;
            data.p1Scheme    = PlayerSelectionData.Instance.p1Scheme;
            data.p2Character = PlayerSelectionData.Instance.p2Character;
            data.p2Scheme    = PlayerSelectionData.Instance.p2Scheme;
        }

        // Current scene becomes the "latest" level
        data.lastScene = SceneManager.GetActiveScene().name;

        SaveSystem.Save(data);
    }

    // Call this when the player finishes the level (optional, for later)
    public void RegisterLevelFinished(float timeInSeconds)
    {
        SaveData data = SaveSystem.Load();
        if (data == null) return;

        string sceneName = SceneManager.GetActiveScene().name;

        LevelResult result = data.levelResults
            .FirstOrDefault(r => r.sceneName == sceneName);

        if (result == null)
        {
            result = new LevelResult { sceneName = sceneName, bestTime = timeInSeconds };
            data.levelResults.Add(result);
        }
        else
        {
            if (timeInSeconds < result.bestTime)
                result.bestTime = timeInSeconds;
        }

        SaveSystem.Save(data);
    }
}
