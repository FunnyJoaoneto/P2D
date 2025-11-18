using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public void SelectSingleKeyboard()
    {
        PlayerMode.SingleKeyboard = true;
        SceneManager.LoadSceneAsync(3);
    }

    public void SelectTwoDevices()
    {
        //Needs to be true for now, because the game only supports single keyboard mode.
        PlayerMode.SingleKeyboard = true;
        SceneManager.LoadSceneAsync(3);
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
