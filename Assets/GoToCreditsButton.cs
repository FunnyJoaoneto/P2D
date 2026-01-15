using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToCreditsButton : MonoBehaviour
{
    [SerializeField] private string creditsSceneName = "Credits";

    public void GoToCredits()
    {
        SceneManager.LoadScene(creditsSceneName);
    }
}
