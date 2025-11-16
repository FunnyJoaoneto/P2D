using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SistemaDiaNoite : MonoBehaviour // Manager Global
{
    public static SistemaDiaNoite Instance; // Padrão Singleton

    [Header("Objetos de Background")]
    [Tooltip("O SpriteRenderer que cobre o lado esquerdo (X < 0)")]
    public SpriteRenderer backgroundLadoEsquerdo;
    [Tooltip("O SpriteRenderer que cobre o lado direito (X >= 0)")]
    public SpriteRenderer backgroundLadoDireito;

    [Header("Configurações de Cor")]
    public Color corDia = new Color(0.7f, 0.7f, 1f, 1f); // Azul claro
    public Color corNoite = new Color(0.1f, 0.1f, 0.3f, 1f); // Azul escuro

    [Header("Controle de Tempo e Transição")]
    [Tooltip("Duração da transição em segundos.")]
    public float duracaoTransicao = 3.0f;
    [Tooltip("Tempo em segundos entre as trocas automáticas de estado.")]
    public float intervaloTroca = 15f;

    // Variáveis de controle de estado
    private float proximaTrocaTempo;
    private float inicioTransicaoTempo;
    private bool ladoEsquerdoAtualDia = true; // Lado Esquerdo começa como Dia
    private bool emTransicao = false;

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
        // Lógica de transição automática
        if (!emTransicao && Time.time >= proximaTrocaTempo)
        {
            IniciarTransicao();
        }

        // Executa a transição suave
        if (emTransicao)
        {
            ExecutarTransicao();
        }
    }

    private void DefinirEstadoInicial()
    {
        if (backgroundLadoEsquerdo != null) backgroundLadoEsquerdo.color = corDia;
        if (backgroundLadoDireito != null) backgroundLadoDireito.color = corNoite;
        ladoEsquerdoAtualDia = true;
    }

    private void IniciarTransicao()
    {
        emTransicao = true;
        inicioTransicaoTempo = Time.time;
        ladoEsquerdoAtualDia = !ladoEsquerdoAtualDia; // Inverte o lado que será Dia
    }

    private void ExecutarTransicao()
    {
        float tempoDecorrido = Time.time - inicioTransicaoTempo;
        float percentualCompleto = Mathf.Clamp01(tempoDecorrido / duracaoTransicao);

        Color corAtualEsquerda;
        Color corAtualDireita;

        if (ladoEsquerdoAtualDia)
        {
            // Transição para: Esquerda = Dia, Direita = Noite
            corAtualEsquerda = Color.Lerp(corNoite, corDia, percentualCompleto);
            corAtualDireita = Color.Lerp(corDia, corNoite, percentualCompleto);
        }
        else
        {
            // Transição para: Esquerda = Noite, Direita = Dia
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

        // Garante as cores finais exatas após a transição
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

    // --- Lógica Simples baseada na Posição do Player (X=0 é o centro) ---
    /// <summary>
    /// Verifica se a posição dada está na zona 'Dia'.
    /// </summary>
    /// <param name="pointX">A coordenada X do jogador.</param>
    /// <returns>True se a posição estiver na Zona Dia, False se for Zona Noite.</returns>
    public bool IsInBrightZone(float pointX)
    {
        // Se o lado esquerdo (X < 0) é Dia, retornamos true se o jogador estiver à esquerda.
        if (ladoEsquerdoAtualDia)
        {
            return pointX < 0;
        }
        // Se o lado direito (X >= 0) é Dia, retornamos true se o jogador estiver à direita ou no centro.
        else
        {
            return pointX >= 0;
        }
    }
}