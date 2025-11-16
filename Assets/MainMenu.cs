using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void SelectSingleKeyboard()
    {
        PlayerMode.SingleKeyboard = true;
        SceneManager.LoadSceneAsync(1);
    }

    public void SelectTwoDevices()
    {
        PlayerMode.SingleKeyboard = false;
        SceneManager.LoadSceneAsync(1);
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
