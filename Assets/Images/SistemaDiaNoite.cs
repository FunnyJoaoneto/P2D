using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SistemaDiaNoite : MonoBehaviour // Manager Global
{
    public static SistemaDiaNoite Instance; // Padr�o Singleton

    [Header("Objetos de Background")]
    [Tooltip("O SpriteRenderer que cobre o lado esquerdo (X < 0)")]
    public SpriteRenderer backgroundLadoEsquerdo;
    [Tooltip("O SpriteRenderer que cobre o lado direito (X >= 0)")]
    public SpriteRenderer backgroundLadoDireito;

    [Header("Configura��es de Cor")]
    public Color corDia = new Color(0.7f, 0.7f, 1f, 1f); // Azul claro
    public Color corNoite = new Color(0.1f, 0.1f, 0.3f, 1f); // Azul escuro

    [Header("Tutorial Control")]
    public bool tutorialMode = false;

    public float tutorialTriggerY = 10f; // Y threshold you want
    private bool tutorialTriggered = false;

    [Header("Spotlight")]
    public Transform spotlight;
    public float spotlightStartY = 0f;
    public float spotlightEndY = 180f;

    [Header("Controle de Tempo e Transi��o")]
    [Tooltip("Dura��o da transi��o em segundos.")]
    public float duracaoTransicao = 3.0f;
    [Tooltip("Tempo em segundos entre as trocas autom�ticas de estado.")]
    public float intervaloTroca = 15f;

    // Vari�veis de controle de estado
    private float proximaTrocaTempo;
    private float inicioTransicaoTempo;
    private bool ladoEsquerdoAtualDia = true; // Lado Esquerdo come�a como Dia
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
        // Define o estado inicial: Esquerda Dia, Direita Noite
        DefinirEstadoInicial();
        proximaTrocaTempo = Time.time + intervaloTroca;
    }

    void Update()
    {
        if (!tutorialMode)
        {
            // Automatic switching (normal mode)
            if (!emTransicao && Time.time >= proximaTrocaTempo)
                IniciarTransicao();
        }
        else
        {
            // Tutorial mode: wait for players
            CheckTutorialTrigger();
        }

        if (emTransicao){
            ExecutarTransicao();
        }
    }

    private void CheckTutorialTrigger()
    {
        if (tutorialTriggered)
            return;

        GameObject player1 = FindPlayerOnLayer("Player1");
        GameObject player2 = FindPlayerOnLayer("Player2");

        if (player1 == null || player2 == null)
            return;

        bool reached1 = player1.transform.position.y >= tutorialTriggerY;
        bool reached2 = player2.transform.position.y >= tutorialTriggerY;

        if (reached1 && reached2)
        {
            tutorialTriggered = true;
            IniciarTransicao();
        }
    }

    private GameObject FindPlayerOnLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
            return null;

        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject obj in allPlayers)
        {
            if (obj.layer == layer)
                return obj;
        }

        return null;
    }

    private void DefinirEstadoInicial()
    {
        if (backgroundLadoEsquerdo != null) backgroundLadoEsquerdo.color = corDia;
        if (backgroundLadoDireito != null) backgroundLadoDireito.color = corNoite;
        ladoEsquerdoAtualDia = true;
    }

    private void IniciarTransicao()
    {
        spotlightInitialY = spotlightStartY;
        spotlightTargetY = spotlightEndY;

        // If direction reversed, swap
        if (!ladoEsquerdoAtualDia)
        {
            spotlightInitialY = spotlightEndY;
            spotlightTargetY = spotlightStartY;
        }
        emTransicao = true;
        inicioTransicaoTempo = Time.time;
        ladoEsquerdoAtualDia = !ladoEsquerdoAtualDia; // Inverte o lado que ser� Dia
    }

    private void ExecutarTransicao()
    {
        float tempoDecorrido = Time.time - inicioTransicaoTempo;
        float percentualCompleto = Mathf.Clamp01(tempoDecorrido / duracaoTransicao);

        if (spotlight != null)
        {
            float y = Mathf.Lerp(spotlightInitialY, spotlightTargetY, percentualCompleto);
            spotlight.rotation = Quaternion.Euler(spotlight.rotation.eulerAngles.x, y, spotlight.rotation.eulerAngles.z);
        }

        Color corAtualEsquerda;
        Color corAtualDireita;

        if (ladoEsquerdoAtualDia)
        {
            // Transi��o para: Esquerda = Dia, Direita = Noite
            corAtualEsquerda = Color.Lerp(corNoite, corDia, percentualCompleto);
            corAtualDireita = Color.Lerp(corDia, corNoite, percentualCompleto);
        }
        else
        {
            // Transi��o para: Esquerda = Noite, Direita = Dia
            corAtualEsquerda = Color.Lerp(corDia, corNoite, percentualCompleto);
            corAtualDireita = Color.Lerp(corNoite, corDia, percentualCompleto);
        }

        if (backgroundLadoEsquerdo != null) backgroundLadoEsquerdo.color = corAtualEsquerda;
        if (backgroundLadoDireito != null) backgroundLadoDireito.color = corAtualDireita;

        if (percentualCompleto >= 1f)
        {
            FinalizarTransicao();
        }
    }

    private void FinalizarTransicao()
    {
        emTransicao = false;

        if (spotlight != null)
        {
            float finalY = ladoEsquerdoAtualDia ? spotlightStartY : spotlightEndY;
            spotlight.rotation = Quaternion.Euler(spotlight.rotation.eulerAngles.x, finalY, spotlight.rotation.eulerAngles.z);
        }

        // Garante as cores finais exatas ap�s a transi��o
        if (ladoEsquerdoAtualDia)
        {
            if (backgroundLadoEsquerdo != null) backgroundLadoEsquerdo.color = corDia;
            if (backgroundLadoDireito != null) backgroundLadoDireito.color = corNoite;
        }
        else
        {
            if (backgroundLadoEsquerdo != null) backgroundLadoEsquerdo.color = corNoite;
            if (backgroundLadoDireito != null) backgroundLadoDireito.color = corDia;
        }

        proximaTrocaTempo = Time.time + intervaloTroca;
    }

    // --- L�gica Simples baseada na Posi��o do Player (X=0 � o centro) ---
    /// <summary>
    /// Verifica se a posi��o dada est� na zona 'Dia'.
    /// </summary>
    /// <param name="pointX">A coordenada X do jogador.</param>
    /// <returns>True se a posi��o estiver na Zona Dia, False se for Zona Noite.</returns>
    public bool IsInBrightZone(float pointX)
    {
        // Se o lado esquerdo (X < 0) � Dia, retornamos true se o jogador estiver � esquerda.
        if (ladoEsquerdoAtualDia)
        {
            return pointX < 0;
        }
        // Se o lado direito (X >= 0) � Dia, retornamos true se o jogador estiver � direita ou no centro.
        else
        {
            return pointX >= 0;
        }
    }
}