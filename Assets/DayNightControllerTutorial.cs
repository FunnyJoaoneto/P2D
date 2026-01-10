using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class SistemaDiaNoiteLuzes : MonoBehaviour
{
    public static SistemaDiaNoiteLuzes Instance;

    [Header("Lights 2D")]
    public Light2D lightLadoEsquerdo;
    public Light2D lightLadoDireito;

    [Header("Tutorial Control")]
    public bool tutorialMode = false;
    public float tutorialTriggerY = 10f;
    private bool tutorialTriggered = false;

    [Header("Spotlight (rotates during transition)")]
    public Transform spotlight;
    public float spotlightStartY = 0f;
    public float spotlightEndY = 180f;

    [Header("Tutorial Behaviour")]
    public TMPro.TextMeshProUGUI tutorialText;
    public string tutorialText_Wait = "Wait... something is happening";
    public string tutorialText_Explain = "The world is turning, get to your zone fast";

    [Header("Transition Settings")]
    public float duracaoTransicao = 3f;
    public float intervaloTroca = 15f;

    private float proximaTrocaTempo;
    private float inicioTransicaoTempo;
    private bool ladoEsquerdoAtualDia = true;
    private bool emTransicao = false;
    private float spotlightInitialY;
    private float spotlightTargetY;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        DefinirEstadoInicial();
        proximaTrocaTempo = Time.time + intervaloTroca;
    }

    void Update()
    {
        if (!tutorialMode)
        {
            if (!emTransicao && Time.time >= proximaTrocaTempo)
                IniciarTransicao();
        }
        else
        {
            CheckTutorialTrigger();
        }

        if (emTransicao)
            ExecutarTransicao();
    }

    private void CheckTutorialTrigger()
    {
        if (tutorialTriggered) return;

        GameObject player1 = FindPlayerOnLayer("Player1");
        GameObject player2 = FindPlayerOnLayer("Player2");

        if (player1 == null || player2 == null) return;

        if (player1.transform.position.y >= tutorialTriggerY && player2.transform.position.y >= tutorialTriggerY)
        {
            tutorialTriggered = true;
            StartCoroutine(TutorialSequence());
        }
    }

    private IEnumerator TutorialSequence()
    {
        PlayerGlobalLock.movementLocked = true;

        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(true);
            tutorialText.text = tutorialText_Wait;
        }

        yield return new WaitForSeconds(2f);

        if (tutorialText != null)
            tutorialText.text = tutorialText_Explain;

        yield return new WaitForSeconds(2f);

        if (tutorialText != null)
        {
            tutorialText.text = "";
            tutorialText.gameObject.SetActive(false);
        }

        PlayerGlobalLock.movementLocked = false;

        IniciarTransicao();

        while (emTransicao)
            yield return null;
    }

    private GameObject FindPlayerOnLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1) return null;

        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in allPlayers)
            if (obj.layer == layer)
                return obj;

        return null;
    }

    private void DefinirEstadoInicial()
    {
        ApplyLightRotation(ladoEsquerdoAtualDia);
    }

    private void IniciarTransicao()
    {
        spotlightInitialY = spotlightStartY;
        spotlightTargetY = spotlightEndY;

        if (!ladoEsquerdoAtualDia)
        {
            spotlightInitialY = spotlightEndY;
            spotlightTargetY = spotlightStartY;
        }

        emTransicao = true;
        inicioTransicaoTempo = Time.time;
        ladoEsquerdoAtualDia = !ladoEsquerdoAtualDia;
    }

    private void ExecutarTransicao()
    {
        float tempoDecorrido = Time.time - inicioTransicaoTempo;
        float percentual = Mathf.Clamp01(tempoDecorrido / duracaoTransicao);

        if (spotlight != null)
        {
            float y = Mathf.Lerp(spotlightInitialY, spotlightTargetY, percentual);
            spotlight.rotation = Quaternion.Euler(spotlight.rotation.eulerAngles.x, y, spotlight.rotation.eulerAngles.z);
        }

        // Luzes: interpolação simples de rotação para simular Dia/Noite
        ApplyLightRotation(ladoEsquerdoAtualDia);

        if (percentual >= 1f)
            FinalizarTransicao();
    }

    private void FinalizarTransicao()
    {
        emTransicao = false;
        ApplyLightRotation(ladoEsquerdoAtualDia);
        proximaTrocaTempo = Time.time + intervaloTroca;
    }

    private void ApplyLightRotation(bool leftIsDay)
    {
        if (lightLadoEsquerdo)
            lightLadoEsquerdo.transform.localRotation =
                Quaternion.AngleAxis(leftIsDay ? 90f : -90f, Vector3.forward);

        if (lightLadoDireito)
            lightLadoDireito.transform.localRotation =
                Quaternion.AngleAxis(leftIsDay ? -90f : 90f, Vector3.forward);
    }

    /// <summary>
    /// Verifica se a posição está na zona Dia
    /// </summary>
    public bool IsInBrightZone(float pointX)
    {
        return ladoEsquerdoAtualDia ? pointX < 0 : pointX >= 0;
    }
}
