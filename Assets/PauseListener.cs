using UnityEngine;
using UnityEngine.InputSystem;

public class PauseListener : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;

    public PlayerInput playerInput;

    void OnEnable()
    {
        var actions = playerInput.actions;
        actions["Pause"].performed += _ => TogglePause();
    }

    void OnDisable()
    {
        var actions = playerInput.actions;
        actions["Pause"].performed -= _ => TogglePause();
    }

    void TogglePause()
    {
        bool isActive = pauseMenu.activeSelf;
        pauseMenu.SetActive(!isActive);
        Time.timeScale = isActive ? 1f : 0f;
    }
}
