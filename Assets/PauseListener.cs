using UnityEngine;
using UnityEngine.InputSystem;

public class PauseListener : MonoBehaviour
{
    [SerializeField] GameObject pauseMenu;

    public PlayerInput playerInput;

    public void HandlePause()
    {
        Debug.Log("Pause triggered!");
        TogglePause();
    }

    void TogglePause()
    {
        bool isActive = pauseMenu.activeSelf;
        pauseMenu.SetActive(!isActive);
        Time.timeScale = isActive ? 1f : 0f;
    }
}
