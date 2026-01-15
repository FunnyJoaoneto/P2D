using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;   // CinemachineBrain
using UnityEngine.InputSystem;
using System;
using System.Text;
using System.Text.RegularExpressions;

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

    [Header("Players (for control text)")]
    public PlayerInput p1Input;
    public PlayerInput p2Input;

    // These are filled from menu selection (light vs night swapping)
    private PlayerInput playerAInput;   // Light player
    private PlayerInput playerBInput;   // Night player
    private string playerAGroup;
    private string playerBGroup;

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

    private Dictionary<string, System.Func<string>> tokenMap;
    private static readonly Regex TokenRegex = new Regex(@"\{([A-Z0-9_]+)\}", RegexOptions.Compiled);


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
        SetupPlayersAndSchemesFromMenu();
        BuildTokenMap();


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

        balloonText.text = ResolveTokens(msg ?? "");
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

    private void SetupPlayersAndSchemesFromMenu()
    {
        var data = PlayerSelectionData.Instance;

        bool p1IsLight = data.p1Character == "LightGuy";

        // Player A = LIGHT, Player B = NIGHT
        playerAInput = p1IsLight ? p1Input : p2Input;
        playerBInput = p1IsLight ? p2Input : p1Input;

        // IMPORTANT: use the GROUP names you saved from the menu
        // These must match the Binding Groups in your Input Actions asset
        playerAGroup = p1IsLight ? data.p1Scheme : data.p2Scheme;
        playerBGroup = p1IsLight ? data.p2Scheme : data.p1Scheme;
    }

    private void BuildTokenMap()
    {
        // Assumes you already set:
        // playerAInput/playerAGroup = LIGHT
        // playerBInput/playerBGroup = NIGHT
        tokenMap = new Dictionary<string, System.Func<string>>()
        {
            // LIGHT (Player A)
            ["LIGHT_LEFT"]     = () => GetMovePart(playerAInput, playerAGroup, "left"),
            ["LIGHT_RIGHT"]    = () => GetMovePart(playerAInput, playerAGroup, "right"),
            ["LIGHT_UP"]       = () => GetMovePart(playerAInput, playerAGroup, "up"),
            ["LIGHT_DOWN"]     = () => GetMovePart(playerAInput, playerAGroup, "down"),
            ["LIGHT_JUMP"]     = () => GetActionBinding(playerAInput, playerAGroup, "Jump"),
            ["LIGHT_ABILITY"]  = () => GetActionBinding(playerAInput, playerAGroup, "Ability"),
            ["LIGHT_INTERACT"] = () => GetActionBinding(playerAInput, playerAGroup, "Interact"),

            // NIGHT (Player B)
            ["NIGHT_LEFT"]     = () => GetMovePart(playerBInput, playerBGroup, "left"),
            ["NIGHT_RIGHT"]    = () => GetMovePart(playerBInput, playerBGroup, "right"),
            ["NIGHT_UP"]       = () => GetMovePart(playerBInput, playerBGroup, "up"),
            ["NIGHT_DOWN"]     = () => GetMovePart(playerBInput, playerBGroup, "down"),
            ["NIGHT_JUMP"]     = () => GetActionBinding(playerBInput, playerBGroup, "Jump"),
            ["NIGHT_ABILITY"]  = () => GetActionBinding(playerBInput, playerBGroup, "Ability"),
            ["NIGHT_INTERACT"] = () => GetActionBinding(playerBInput, playerBGroup, "Interact"),
        };
    }

    private string ResolveTokens(string text)
    {
        if (tokenMap == null || tokenMap.Count == 0)
            return text;

        return TokenRegex.Replace(text, match =>
        {
            string key = match.Groups[1].Value; // inside {...}
            if (tokenMap.TryGetValue(key, out var getter))
                return getter?.Invoke() ?? "?";
            return match.Value; // unknown token -> keep as-is
        });
    }

    private string GetActionBinding(PlayerInput p, string group, string actionName)
    {
        if (p == null) return "?";
        var action = p.actions.FindAction(actionName, false);
        if (action == null) return "?";

        // Try find binding IN THIS GROUP (prevents grabbing Gamepad "A")
        int idx = FindFirstBindingIndexInGroup(action, group);
        if (idx >= 0)
            return action.GetBindingDisplayString(idx);

        // Fallback: prefer keyboard if possible
        int kb = FindFirstBindingMatchingPath(action, "<Keyboard>");
        if (kb >= 0)
            return action.GetBindingDisplayString(kb);

        // Last fallback: first non-composite binding
        for (int i = 0; i < action.bindings.Count; i++)
            if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
                return action.GetBindingDisplayString(i);

        return "?";
    }

    private int FindFirstBindingIndexInGroup(InputAction action, string group)
    {
        if (string.IsNullOrWhiteSpace(group))
            return -1;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];
            if (b.isComposite || b.isPartOfComposite) continue;
            if (BindingHasGroup(b, group)) return i;
        }
        return -1;
    }

    private int FindFirstBindingMatchingPath(InputAction action, string contains)
    {
        for (int i = 0; i < action.bindings.Count; i++)
        {
            var b = action.bindings[i];
            if (b.isComposite || b.isPartOfComposite) continue;

            var path = b.effectivePath;
            if (!string.IsNullOrEmpty(path) && path.Contains(contains))
                return i;
        }
        return -1;
    }

    private string GetMovePart(PlayerInput p, string group, string partName)
    {
        if (p == null) return "?";
        var action = p.actions.FindAction("Move", false);
        if (action == null) return "?";

        var bindings = action.bindings;

        for (int i = 0; i < bindings.Count; i++)
        {
            if (!bindings[i].isComposite) continue;

            // We accept a composite if:
            // - composite has the group, OR
            // - ANY of its parts has the group
            bool compositeMatches = BindingHasGroup(bindings[i], group);

            int end = i + 1;
            while (end < bindings.Count && bindings[end].isPartOfComposite) end++;

            bool anyPartMatches = false;
            for (int j = i + 1; j < end; j++)
                if (BindingHasGroup(bindings[j], group))
                    anyPartMatches = true;

            if (!compositeMatches && !anyPartMatches)
                continue;

            // Find the requested part (left/right/up/down)
            for (int j = i + 1; j < end; j++)
            {
                if (string.Equals(bindings[j].name, partName, System.StringComparison.OrdinalIgnoreCase))
                    return action.GetBindingDisplayString(j);
            }
        }

        return "?";
    }

    private bool BindingHasGroup(InputBinding binding, string group)
    {
        if (string.IsNullOrEmpty(group) || string.IsNullOrEmpty(binding.groups))
            return false;

        // groups are separated by ';'
        var groups = binding.groups.Split(InputBinding.Separator);
        for (int i = 0; i < groups.Length; i++)
            if (string.Equals(groups[i], group, System.StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
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
