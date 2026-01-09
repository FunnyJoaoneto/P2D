using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;   // CinemachineBrain

public class TutorialCameraSequence : MonoBehaviour
{
    [Header("Camera References")]
    public Camera cam;

    [Header("Camera Walls")]
    public GameObject cameraWallsRoot;

    [Header("Timing")]
    public float startDelay = 0.5f;

    [System.Serializable]
    public class StepMessage
    {
        [TextArea] public string message;

        [Header("Ping Targets")]
        public Transform[] pingTargets;
    }

    [System.Serializable]
    public class Step
    {
        public Transform target;
        public float size = 20f;
        public float moveTime = 1.5f;

        [Header("Messages (multiple per step)")]
        public StepMessage[] messages;
    }

    [Header("Intro Messages (optional)")]
    public StepMessage[] introMessages;

    [Header("Sequence")]
    public Step[] steps;

    [Header("Balloon UI (single)")]
    public GameObject balloonRoot;         // Panel behind the text
    public TextMeshProUGUI balloonText;    // TMP text inside the panel
    public Button nextButton;              // button bottom-right

    [Header("Typewriter")]
    public float charsPerSecond = 40f;     // 0 = instant
    public float letterFadeTime = 0.03f;   // 0 = no fade

    [Header("Ping UI")]
    public GameObject pingPrefab;
    public Canvas uiCanvas;

    // internal
    private bool wallsInitiallyActive = true;

    private Vector3 initialCamPos;
    private float initialCamSize;

    // Cinemachine brain on the main camera
    private CinemachineBrain brain;

    // Next logic
    private bool nextRequested = false;

    private readonly List<GameObject> activeStepPings = new List<GameObject>();
    private readonly HashSet<Transform> activeStepPingTargets = new HashSet<Transform>();

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnClickNext);

        if (balloonRoot != null)
            balloonRoot.SetActive(true); // balloon stays up
    }

    public void OnClickNext()
    {
        // Next ALWAYS advances (even mid-typing)
        nextRequested = true;
    }

    private IEnumerator Start()
    {
        if (cam == null)
            cam = Camera.main;

        brain = cam != null ? cam.GetComponent<CinemachineBrain>() : null;

        PlayerGlobalLock.movementLocked = true;

        // Let Cinemachine settle for 1 frame
        yield return null;

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        initialCamPos = cam.transform.position;
        initialCamSize = cam.orthographicSize;

        // Disable Cinemachine while we drive the camera
        if (brain != null)
            brain.enabled = false;

        // DISABLE CAMERA WALLS
        if (cameraWallsRoot != null)
        {
            wallsInitiallyActive = cameraWallsRoot.activeSelf;
            cameraWallsRoot.SetActive(false);
        }

        // Ensure balloon visible
        if (balloonRoot != null)
            balloonRoot.SetActive(true);

        // INTRO messages
        if (introMessages != null && introMessages.Length > 0)
            yield return PlayMessages(introMessages);

        // STEPS
        foreach (var step in steps)
        {
            // Move camera to the step target (balloon remains visible)
            if (step != null && step.target != null)
                yield return MoveToStep(step);

            // Then play that step's message sequence
            if (step != null && step.messages != null && step.messages.Length > 0)
                yield return PlayMessages(step.messages);
        }

        // Cleanup / restore
        ClearBalloon();

        if (balloonRoot != null)
            balloonRoot.SetActive(false);

        yield return SmoothBackToInitial(1.5f);

        if (brain != null)
            brain.enabled = true;

        if (cameraWallsRoot != null)
            cameraWallsRoot.SetActive(wallsInitiallyActive);

        PlayerGlobalLock.movementLocked = false;
    }

    // Plays an array of messages; Next always jumps to the next message.
    private IEnumerator PlayMessages(StepMessage[] messages)
    {
        ClearStepPings();

        for (int i = 0; i < messages.Length; i++)
        {
            StepMessage m = messages[i];
            if (m == null) continue;

            // Spawn pings for this message
            if (m.pingTargets != null)
            {
                foreach (var p in m.pingTargets)
                {
                    if (p == null) continue;

                    // avoid duplicates if multiple messages reference same target
                    if (activeStepPingTargets.Add(p))
                        CreatePing(p);
                }
            }

            // Show this message (typing) until Next is pressed
            yield return PlayOneMessage(m.message);
        }

        // Optional: clear between steps/message blocks
        // (If you want the last text to stay on screen, remove this line)
        ClearBalloon();
        ClearStepPings();
    }

    private IEnumerator PlayOneMessage(string msg)
    {
        nextRequested = false;

        if (balloonText == null)
            yield break;

        balloonText.text = msg ?? "";
        balloonText.maxVisibleCharacters = 0;

        // Setup mesh/colors for fade
        balloonText.ForceMeshUpdate();
        int totalChars = balloonText.textInfo.characterCount;

        SetAllVisibleCharsAlpha(0);
        balloonText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Typewriter with optional fade-per-letter
        float delayPerChar = (charsPerSecond <= 0f) ? 0f : (1f / charsPerSecond);

        for (int i = 0; i < totalChars; i++)
        {
            if (nextRequested)
                break; // skip current message immediately

            balloonText.maxVisibleCharacters = i + 1;

            if (letterFadeTime > 0f)
                yield return FadeChar(i, letterFadeTime);
            else
            {
                SetCharAlpha(i, 255);
                balloonText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            }

            if (delayPerChar > 0f)
            {
                float t = 0f;
                while (t < delayPerChar)
                {
                    if (nextRequested) yield break; // jump instantly to next message
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }
        }

        // If we finished typing naturally, wait for Next to go to next message
        while (!nextRequested)
            yield return null;
    }

    private IEnumerator FadeChar(int charIndex, float fadeTime)
    {
        if (fadeTime <= 0f)
            yield break;

        float t = 0f;
        while (t < 1f)
        {
            if (nextRequested)
                yield break;

            t += Time.deltaTime / fadeTime;
            byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(0f, 255f, t)), 0, 255);
            SetCharAlpha(charIndex, a);
            balloonText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }
    }

    private void ClearBalloon()
    {
        if (balloonText != null)
        {
            balloonText.text = "";
            balloonText.maxVisibleCharacters = 0;
        }
    }

    // CAMERA MOVEMENT
    private IEnumerator MoveToStep(Step step)
    {
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;

        Vector3 targetPos = new Vector3(
            step.target.position.x,
            step.target.position.y,
            cam.transform.position.z
        );

        float t = 0f;
        float duration = Mathf.Max(step.moveTime, 0.01f);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
            cam.orthographicSize = Mathf.Lerp(startSize, step.size, t);
            yield return null;
        }
    }

    private IEnumerator SmoothBackToInitial(float duration)
    {
        Vector3 startPos = cam.transform.position;
        float startSize = cam.orthographicSize;

        float t = 0f;
        duration = Mathf.Max(duration, 0.01f);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            cam.transform.position = Vector3.Lerp(startPos, initialCamPos, t);
            cam.orthographicSize = Mathf.Lerp(startSize, initialCamSize, t);
            yield return null;
        }
    }

    // PINGS
    private void CreatePing(Transform target)
    {
        if (pingPrefab == null || uiCanvas == null || cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(target.position);
        GameObject ping = Instantiate(pingPrefab, uiCanvas.transform);
        ping.transform.position = screenPos;
        activeStepPings.Add(ping);
    }

    private void ClearStepPings()
    {
        for (int i = 0; i < activeStepPings.Count; i++)
        {
            if (activeStepPings[i] != null)
                Destroy(activeStepPings[i]);
        }

        activeStepPings.Clear();
        activeStepPingTargets.Clear();
    }

    // TMP Fade Helpers
    private void SetAllVisibleCharsAlpha(byte alpha)
    {
        if (balloonText == null) return;

        balloonText.ForceMeshUpdate();
        var textInfo = balloonText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
            SetCharAlpha(i, alpha);
    }

    private void SetCharAlpha(int charIndex, byte alpha)
    {
        var textInfo = balloonText.textInfo;
        if (charIndex < 0 || charIndex >= textInfo.characterCount) return;

        var c = textInfo.characterInfo[charIndex];
        if (!c.isVisible) return;

        int matIndex = c.materialReferenceIndex;
        int vertexIndex = c.vertexIndex;

        var colors = textInfo.meshInfo[matIndex].colors32;
        colors[vertexIndex + 0].a = alpha;
        colors[vertexIndex + 1].a = alpha;
        colors[vertexIndex + 2].a = alpha;
        colors[vertexIndex + 3].a = alpha;
    }
}
