using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private const string PREF_KEY = "MasterVolume";

    private void Awake()
    {
        if (!slider) slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        float v = PlayerPrefs.GetFloat(PREF_KEY, 1f);
        slider.SetValueWithoutNotify(v);
        Apply(v);
        slider.onValueChanged.AddListener(Apply);
    }

    private void OnDisable()
    {
        slider.onValueChanged.RemoveListener(Apply);
    }

    private void Apply(float v)
    {
        AudioListener.volume = v;           // 0..1
        PlayerPrefs.SetFloat(PREF_KEY, v);  // save
    }
}
