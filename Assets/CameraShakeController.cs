using System.Collections;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    public static CameraShakeController Instance;

    private Vector3 originalLocalPos;
    private Coroutine shakeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartShake(float duration, AnimationCurve intensityCurve)
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine(duration, intensityCurve));
    }

    public void StopShake()
    {
        if (shakeRoutine != null)
        {
            StopCoroutine(shakeRoutine);
            shakeRoutine = null;
        }

        transform.localPosition = originalLocalPos;
    }

    private IEnumerator ShakeRoutine(float duration, AnimationCurve intensityCurve)
    {
        originalLocalPos = transform.localPosition;
        float t = 0f;

        while (t < duration)
        {
            float normalized = t / duration;
            float strength = intensityCurve != null ? intensityCurve.Evaluate(normalized) * 0.5f : 0f;

            float offsetX = Random.Range(-1f, 1f) * strength;
            float offsetY = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);

            t += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalLocalPos;
        shakeRoutine = null;
    }
}
