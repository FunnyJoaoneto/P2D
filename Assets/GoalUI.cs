using UnityEngine;
using UnityEngine.UI;

public class GoalUI : MonoBehaviour
{
    public static GoalUI Instance { get; private set; }

    [Header("UI")]
    public Image chestImage;

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
    }

    public void OnSunCollected()
    {
        sunCollected = true;
        UpdateChestSprite();
    }

    public void OnMoonCollected()
    {
        moonCollected = true;
        UpdateChestSprite();
    }

    public void ResetCollected()
    {
        sunCollected = false;
        moonCollected = false;
        UpdateChestSprite();
    }

    private void Start()
    {
        // start with empty chest
        UpdateChestSprite();
    }

    private void UpdateChestSprite()
    {
        if (sunCollected && moonCollected)
            chestImage.sprite = chestSunAndMoon;
        else if (sunCollected)
            chestImage.sprite = chestSunOnly;
        else if (moonCollected)
            chestImage.sprite = chestMoonOnly;
        else
            chestImage.sprite = chestEmpty;
    }
}
