using UnityEngine;

public class LevelTimer : MonoBehaviour
{
    public static float timeElapsed;

    void OnEnable()
    {
        timeElapsed = 0f;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
    }
}
