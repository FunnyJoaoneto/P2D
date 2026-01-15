using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroVideoController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("UI")]
    [SerializeField] private GameObject skipButton; // set inactive in scene
    [SerializeField] private bool allowSkip = true; // optional

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName = "Level01";

    [Header("Skip")]
    [SerializeField] private float showSkipAfterSeconds = 1f;

    private float startTime;
    private bool isLoading;

    private void Awake()
    {
        if (!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.playOnAwake = false;

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnFinished;

        if (skipButton) skipButton.SetActive(false);
    }

    private void Start()
    {
        startTime = Time.time;
        videoPlayer.Prepare();
    }

    private void Update()
    {
        if (isLoading) return;

        if (allowSkip)
        {
            if (skipButton && !skipButton.activeSelf && Time.time - startTime >= showSkipAfterSeconds)
                skipButton.SetActive(true);
        }
    }

    private void OnPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    private void OnFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    // Hook this to Skip button OnClick()
    public void LoadNextScene()
    {
        if (isLoading) return;
        isLoading = true;
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (!videoPlayer) return;
        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.loopPointReached -= OnFinished;
    }
}
