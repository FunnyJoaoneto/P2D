using UnityEngine;

public class GoalObject : MonoBehaviour
{
    public static GoalObject Instance { get; private set; }

    [Header("Target (2D)")]
    [Tooltip("GameObject with a SpriteRenderer. Leave empty to use this GameObject.")]
    public SpriteRenderer targetRenderer;

    [Header("Sprites")]
    public Sprite chestEmpty;
    public Sprite chestSunOnly;
    public Sprite chestMoonOnly;
    public Sprite chestSunAndMoon;

    private bool sunCollected;
    private bool moonCollected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (targetRenderer == null)
            targetRenderer = GetComponent<SpriteRenderer>();

        if (targetRenderer == null)
            Debug.LogError($"{nameof(GoalObject)} needs a SpriteRenderer assigned or on the same GameObject.", this);
    }

    private void Start()
    {
        UpdateSprite();
    }

    public void OnSunCollected()
    {
        sunCollected = true;
        UpdateSprite();
    }

    public void OnMoonCollected()
    {
        moonCollected = true;
        UpdateSprite();
    }

    public void ResetCollected()
    {
        sunCollected = false;
        moonCollected = false;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (targetRenderer == null) return;

        if (sunCollected && moonCollected)
            targetRenderer.sprite = chestSunAndMoon;
        else if (sunCollected)
            targetRenderer.sprite = chestSunOnly;
        else if (moonCollected)
            targetRenderer.sprite = chestMoonOnly;
        else
            targetRenderer.sprite = chestEmpty;
    }
}
