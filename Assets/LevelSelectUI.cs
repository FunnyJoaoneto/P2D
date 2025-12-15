using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LevelSelectUI : MonoBehaviour
{
    public List<LevelDefinition> allLevels;

    public Transform gridParent;
    public GameObject levelButtonPrefab;

    public Button prevPageButton;
    public Button nextPageButton;
    public TMP_Text pageLabel;

    public int levelsPerPage = 6;

    private int currentPage = 0;
    private SaveData cachedSave;

    void OnEnable()
    {
        cachedSave = SaveSystem.Load() ?? new SaveData();
        RefreshPage();
    }

    public void OnPrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshPage();
        }
    }

    public void OnNextPage()
    {
        int maxPage = (allLevels.Count - 1) / levelsPerPage;
        if (currentPage < maxPage)
        {
            currentPage++;
            RefreshPage();
        }
    }

    private void RefreshPage()
    {
        foreach (Transform t in gridParent)
            Destroy(t.gameObject);

        int start = currentPage * levelsPerPage;
        int end = Mathf.Min(start + levelsPerPage, allLevels.Count);

        for (int i = start; i < end; i++)
        {
            var level = allLevels[i];
            GameObject obj = Instantiate(levelButtonPrefab, gridParent);
            LevelButtonUI ui = obj.GetComponent<LevelButtonUI>();

            float best = -1;
            var result = cachedSave.levelResults.FirstOrDefault(r => r.sceneName == level.sceneName);
            if (result != null)
                best = result.bestTime;

            bool unlocked = (i <= cachedSave.highestUnlockedLevelIndex);

            ui.Setup(level.sceneName, level.displayName, unlocked, best);
        }

        int maxPage = (allLevels.Count - 1) / levelsPerPage;
        pageLabel.text = $"{currentPage + 1} / {maxPage + 1}";
        prevPageButton.interactable = currentPage > 0;
        nextPageButton.interactable = currentPage < maxPage;
    }

    public void OnLevelButtonClicked(string sceneName)
    {

        if (!SaveSystem.SaveExists())
            return;
        SaveData data = SaveSystem.Load();

        if (data == null)
            return;

        // Restore PlayerSelectionData
        if (PlayerSelectionData.Instance == null)
        {
            GameObject go = new GameObject("PlayerSelectionData");
            var p = go.AddComponent<PlayerSelectionData>();

            p.p1Character = data.p1Character;
            p.p1Scheme    = data.p1Scheme;
            p.p2Character = data.p2Character;
            p.p2Scheme    = data.p2Scheme;
        }
        else
        {
            var selection = PlayerSelectionData.Instance;
            selection.p1Character = data.p1Character;
            selection.p1Scheme    = data.p1Scheme;
            selection.p2Character = data.p2Character;
            selection.p2Scheme    = data.p2Scheme;
        }

        SceneManager.LoadScene(sceneName);
    }
}
