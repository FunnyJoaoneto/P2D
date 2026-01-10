using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSideSwitcher : MonoBehaviour
{
    [Header("Lights")]
    public Light2D lightA;
    public Light2D lightB;

    [Header("Timer")]
    public float switchTime = 15f;

    private float timer;
    private bool flipped;

    void Start()
    {
        ApplyRotation();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= switchTime)
        {
            timer = 0f;
            flipped = !flipped;
            ApplyRotation();
        }
    }

    void ApplyRotation()
    {
        // 2D → eixo Z
        if (lightA)
            lightA.transform.localRotation =
                Quaternion.AngleAxis(flipped ? -90f : 90f, Vector3.forward);

        if (lightB)
            lightB.transform.localRotation =
                Quaternion.AngleAxis(flipped ? 90f : -90f, Vector3.forward);
    }
}
