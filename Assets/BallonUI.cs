using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BallonUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private RectTransform root;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button nextButton;

    private Camera cam;
    private bool advanceRequested;
    private bool forceComplete;

    private void Reset()
    {
        root = GetComponent<RectTransform>();
        messageText = GetComponentInChildren<TMP_Text>();
        nextButton = GetComponentInChildren<Button>();
    }

    private void Awake()
    {
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextPressed);
    }

    public void Setup(Camera cameraToUse)
    {
        cam = cameraToUse;
    }

    public void SetScreenPosition(Vector2 screenPos)
    {
        // Works best when Canvas is Screen Space Overlay
        root.position = screenPos;
    }

    public void OnNextPressed()
    {
        // First click: finish typing instantly
        // Second click: advance to next balloon
        if (!forceComplete)
        {
            forceComplete = true;
            return;
        }
        advanceRequested = true;
    }

    public IEnumerator PlayMessage(string msg, float charsPerSecond, float letterFadeTime)
    {
        advanceRequested = false;
        forceComplete = false;

        messageText.text = msg;
        messageText.maxVisibleCharacters = 0;

        // Build mesh info so we can edit vertex colors
        messageText.ForceMeshUpdate();
        TMP_TextInfo textInfo = messageText.textInfo;
        int totalChars = textInfo.characterCount;

        // Hide all chars (alpha 0)
        SetAllVisibleCharsAlpha(0);
        messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        float delayPerChar = (charsPerSecond <= 0f) ? 0f : (1f / charsPerSecond);

        for (int i = 0; i < totalChars; i++)
        {
            if (forceComplete)
                break;

            messageText.maxVisibleCharacters = i + 1;

            // fade just this character in
            yield return FadeChar(i, letterFadeTime);

            if (delayPerChar > 0f)
                yield return new WaitForSeconds(delayPerChar);
        }

        // If user forced completion, reveal everything at once
        messageText.maxVisibleCharacters = totalChars;
        SetAllVisibleCharsAlpha(255);
        messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

        // Now wait for Next click to advance
        while (!advanceRequested)
            yield return null;
    }

    private IEnumerator FadeChar(int charIndex, float fadeTime)
    {
        if (fadeTime <= 0f)
        {
            SetCharAlpha(charIndex, 255);
            messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            if (forceComplete)
                yield break;

            t += Time.deltaTime / fadeTime;
            byte a = (byte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(0f, 255f, t)), 0, 255);
            SetCharAlpha(charIndex, a);
            messageText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            yield return null;
        }
    }

    private void SetAllVisibleCharsAlpha(byte alpha)
    {
        messageText.ForceMeshUpdate();
        var textInfo = messageText.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
            SetCharAlpha(i, alpha);
    }

    private void SetCharAlpha(int charIndex, byte alpha)
    {
        var textInfo = messageText.textInfo;
        if (charIndex < 0 || charIndex >= textInfo.characterCount)
            return;

        var c = textInfo.characterInfo[charIndex];
        if (!c.isVisible)
            return;

        int matIndex = c.materialReferenceIndex;
        int vertexIndex = c.vertexIndex;
        var colors = textInfo.meshInfo[matIndex].colors32;

        colors[vertexIndex + 0].a = alpha;
        colors[vertexIndex + 1].a = alpha;
        colors[vertexIndex + 2].a = alpha;
        colors[vertexIndex + 3].a = alpha;
    }
}
