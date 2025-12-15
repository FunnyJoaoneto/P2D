using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button button;
    public TextMeshProUGUI label;

    public Color normalColor   = Color.white;
    public Color hoverColor    = Color.yellow;
    public Color disabledColor = Color.gray;

    void Awake()
    {
        if (!button) button = GetComponent<Button>();
        if (!label)  label  = GetComponentInChildren<TextMeshProUGUI>();

        if (button == null)
            Debug.LogError($"[ButtonTextColor] No Button found on {name}");
        if (label == null)
            Debug.LogError($"[ButtonTextColor] No TextMeshProUGUI found on {name}");
    }

    void Start()
    {
        Debug.Log($"[ButtonTextColor] Start on {name}. Interactable = {button?.interactable}");
        UpdateColor();
    }

    void Update()
    {
        // Optional: comment this out later, just useful if interactable is changed from code.
        //UpdateColor();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[ButtonTextColor] Pointer ENTER on {name}");

        if (button != null && button.interactable && label != null)
            label.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[ButtonTextColor] Pointer EXIT on {name}");
        UpdateColor();
    }

    void UpdateColor()
    {
        if (button == null || label == null) return;

        if (!button.interactable)
            label.color = disabledColor;
        else
            label.color = normalColor;
    }
}
