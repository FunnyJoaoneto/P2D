using UnityEngine;
using UnityEngine.InputSystem;

public class InputRebindManager : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private void Awake()
    {
        LoadAllRebinds();
        Debug.Log("Input rebinds loaded.");
    }

    public void LoadAllRebinds()
    {
        foreach (var map in inputActions.actionMaps)
        {
            var key = $"rebinds::{inputActions.name}::{map.name}";
            Debug.Log($"Loading rebinds for {map.name} with key {key}");

            if (!PlayerPrefs.HasKey(key))
                continue;

            var json = PlayerPrefs.GetString(key);
            if (!string.IsNullOrEmpty(json))
                map.LoadBindingOverridesFromJson(json);
        }
    }
}
