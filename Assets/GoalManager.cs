using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;

    [Header("Configura��es dos Objetivos")]
    private bool solColetado = false;
    private bool luaColetada = false;

    // --- NOVO: L�GICA DE CONTROLE DE PORT�O ---
    [Header("Controle de Port�o")]
    private int jogadoresNoPortao = 0;
    private const int JOGADORES_NECESSARIOS = 2; // Define que 2 jogadores s�o necess�rios
    private bool transicaoIniciada = false;
    // ------------------------------------------

    [Header("Objetos de Verifica��o")]
    [Tooltip("O GameObject que representa o port�o ou zona de vit�ria.")]
    public GameObject portaDeSaida; // Refer�ncia da porta da cena atual

    [Header("Transi��o de N�vel")]
    [Tooltip("Nome da cena do pr�ximo n�vel (ex: 'SampleScene2').")]
    public string nomeDaProximaCena = "SampleScene2";

    // --- L�GICA DO SETUP E REDEFINI��O DE N�VEL ---

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Chamado sempre que uma nova cena � carregada.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. REDEFINI��O DE ESTADO
        solColetado = false;
        luaColetada = false;
        jogadoresNoPortao = 0; // REINICIALIZA A CONTAGEM
        transicaoIniciada = false; // REINICIALIZA A FLAG DE TRANSI��O

        // 2. ENCONTRA A NOVA PORTA DE SA�DA NO NOVO N�VEL
        portaDeSaida = GameObject.FindGameObjectWithTag("ExitGate");

        if (portaDeSaida != null)
        {
            // Garante que a porta comece no estado trancado
            portaDeSaida.tag = "ExitGate";
        }
        else
        {
            Debug.LogError("GoalManager: Objeto 'portaDeSaida' n�o encontrado com a Tag 'ExitGate' na nova cena.");
        }

        Debug.Log("GoalManager: Estado redefinido para a nova cena.");
    }

    // --- M�TODOS P�BLICOS PARA COLETA (MANTIDOS) ---

    public void ColetarSol()
    {
        if (solColetado) return;
        solColetado = true;
        Debug.Log("Sol coletado! Resta a Lua.");
        VerificarCondicaoDePassagem();
    }

    public void ColetarLua()
    {
        if (luaColetada) return;
        luaColetada = true;
        Debug.Log("Lua coletada! Resta o Sol.");
        VerificarCondicaoDePassagem();
    }

    // --- L�GICA DE VERIFICA��O E TRANSI��O (MODIFICADA) ---

    private void VerificarCondicaoDePassagem()
    {
        if (solColetado && luaColetada)
        {
            Debug.Log("Objetivos completos! O Port�o de Sa�da est� pronto.");

            if (portaDeSaida != null)
            {
                // Mudar a Tag para ser detectada pelo PlayerController
                portaDeSaida.tag = "ExitReady";
                Debug.Log("Port�o liberado com a Tag: ExitReady.");
            }
            // Verifica se a transi��o pode ocorrer imediatamente se os jogadores j� estiverem l�
            if (jogadoresNoPortao == JOGADORES_NECESSARIOS)
            {
                IniciarTransicaoNivel();
            }
        }
    }

    /// <summary>
    /// Chamado pelo PlayerController quando ele ENTRA na �rea da porta.
    /// </summary>
    public void EntrarPortao()
    {
        if (transicaoIniciada) return;

        // Aumenta a contagem de jogadores
        jogadoresNoPortao++;
        Debug.Log($"Jogador entrou no port�o. Total: {jogadoresNoPortao} / {JOGADORES_NECESSARIOS}");

        // Verifica se todos os objetivos foram atingidos e se os dois jogadores est�o no port�o
        if (solColetado && luaColetada && jogadoresNoPortao >= JOGADORES_NECESSARIOS)
        {
            IniciarTransicaoNivel();
        }
        else if (!solColetado || !luaColetada)
        {
            Debug.Log("Faltam colecion�veis.");
        }
        else if (jogadoresNoPortao < JOGADORES_NECESSARIOS)
        {
            Debug.Log($"Aguardando mais {JOGADORES_NECESSARIOS - jogadoresNoPortao} jogador(es).");
        }
    }

    /// <summary>
    /// Chamado quando um PlayerController SAI da �rea da porta.
    /// (Voc� deve chamar este m�todo no OnTriggerExit do PlayerController ou da Porta)
    /// </summary>
    public void SairPortao()
    {
        if (transicaoIniciada) return;

        // Diminui a contagem de jogadores
        jogadoresNoPortao--;
        if (jogadoresNoPortao < 0) jogadoresNoPortao = 0;
        Debug.Log($"Jogador saiu do port�o. Total: {jogadoresNoPortao} / {JOGADORES_NECESSARIOS}");
    }

    /// <summary>
    /// Inicia a transi��o de n�vel ap�s todas as condi��es serem atendidas.
    /// </summary>
    private void IniciarTransicaoNivel()
    {
        if (transicaoIniciada) return;
        transicaoIniciada = true;

        Debug.Log("CONDIÇÃO DE VITÓRIA ATINGIDA! Transicionando para a próxima cena: " + nomeDaProximaCena);

        // 1) Stop players
        PlayerGlobalLock.movementLocked = true;

        // 2) Start UI transition instead of instant load
        if (LevelTransitionManager.Instance != null)
        {
            LevelTransitionManager.Instance.StartTransition(nomeDaProximaCena);
        }
        else
        {
            // Fallback in case something is misconfigured
            SceneManager.LoadScene(nomeDaProximaCena);
        }
    }
}