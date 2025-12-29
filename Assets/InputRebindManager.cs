using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private void Awake()
    {
        LoadAllRebinds();
    }

    public void LoadAllRebinds()
    {
        foreach (var map in inputActions.actionMaps)
        {
            var key = $"rebinds::{inputActions.name}::{map.name}";

            if (!PlayerPrefs.HasKey(key))
                continue;

            var json = PlayerPrefs.GetString(key);
            if (!string.IsNullOrEmpty(json))
                map.LoadBindingOverridesFromJson(json);
        }
    }
}
