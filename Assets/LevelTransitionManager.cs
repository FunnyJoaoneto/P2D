using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;   // If you use TextMeshPro

public class LevelTransitionManager : MonoBehaviour
{
    public static LevelTransitionManager Instance;

    [Header("UI References")]
    public CanvasGroup fadeGroup;            // Assign Panel's CanvasGroup
    public TextMeshProUGUI levelCompleteText; // Assign TextLevelComplete
    public TextMeshProUGUI nextLevelText;     // Assign TextNextLevel

    [Header("Timings")]
    public float fadeDuration = 1.2f;
    public float textDelay = 1.2f;
    public float waitBeforeScene = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public void StartTransition(string nextScene)
    {
        StartCoroutine(TransitionSequence(nextScene));
    }

    private IEnumerator TransitionSequence(string nextScene)
    {
        // Ensure starting state
        fadeGroup.alpha = 0f;
        levelCompleteText.gameObject.SetActive(false);
        nextLevelText.gameObject.SetActive(false);

        // 1) Fade to black
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = 1f;

        // 2) Show "Level Complete"
        levelCompleteText.gameObject.SetActive(true);
        yield return new WaitForSeconds(textDelay);

        // 3) Show "Going to level X"
        nextLevelText.text = "Going to " + nextScene + "...";
        nextLevelText.gameObject.SetActive(true);
        yield return new WaitForSeconds(waitBeforeScene);

        // 4) Load next scene (async)
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        while (!op.isDone)
            yield return null;

        // 5) Hide texts, then fade from black
        levelCompleteText.gameObject.SetActive(false);
        nextLevelText.gameObject.SetActive(false);

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        fadeGroup.alpha = 0f;
    }
}
