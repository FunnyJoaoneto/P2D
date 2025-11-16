using UnityEngine;
using UnityEngine.SceneManagement;

public class GoalManager : MonoBehaviour
{
    public static GoalManager Instance;

    [Header("Configurações dos Objetivos")]
    private bool solColetado = false;
    private bool luaColetada = false;

    // --- NOVO: LÓGICA DE CONTROLE DE PORTÃO ---
    [Header("Controle de Portão")]
    private int jogadoresNoPortao = 0;
    private const int JOGADORES_NECESSARIOS = 2; // Define que 2 jogadores são necessários
    private bool transicaoIniciada = false;
    // ------------------------------------------

    [Header("Objetos de Verificação")]
    [Tooltip("O GameObject que representa o portão ou zona de vitória.")]
    public GameObject portaDeSaida; // Referência da porta da cena atual

    [Header("Transição de Nível")]
    [Tooltip("Nome da cena do próximo nível (ex: 'SampleScene2').")]
    public string nomeDaProximaCena = "SampleScene2";

    // --- LÓGICA DO SETUP E REDEFINIÇÃO DE NÍVEL ---

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
    /// Chamado sempre que uma nova cena é carregada.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. REDEFINIÇÃO DE ESTADO
        solColetado = false;
        luaColetada = false;
        jogadoresNoPortao = 0; // REINICIALIZA A CONTAGEM
        transicaoIniciada = false; // REINICIALIZA A FLAG DE TRANSIÇÃO

        // 2. ENCONTRA A NOVA PORTA DE SAÍDA NO NOVO NÍVEL
        portaDeSaida = GameObject.FindGameObjectWithTag("ExitGate");

        if (portaDeSaida != null)
        {
            // Garante que a porta comece no estado trancado
            portaDeSaida.tag = "ExitGate";
        }
        else
        {
            Debug.LogError("GoalManager: Objeto 'portaDeSaida' não encontrado com a Tag 'ExitGate' na nova cena.");
        }

        Debug.Log("GoalManager: Estado redefinido para a nova cena.");
    }

    // --- MÉTODOS PÚBLICOS PARA COLETA (MANTIDOS) ---

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

    // --- LÓGICA DE VERIFICAÇÃO E TRANSIÇÃO (MODIFICADA) ---

    private void VerificarCondicaoDePassagem()
    {
        if (solColetado && luaColetada)
        {
            Debug.Log("Objetivos completos! O Portão de Saída está pronto.");

            if (portaDeSaida != null)
            {
                // Mudar a Tag para ser detectada pelo PlayerController
                portaDeSaida.tag = "ExitReady";
                Debug.Log("Portão liberado com a Tag: ExitReady.");
            }
            // Verifica se a transição pode ocorrer imediatamente se os jogadores já estiverem lá
            if (jogadoresNoPortao == JOGADORES_NECESSARIOS)
            {
                IniciarTransicaoNivel();
            }
        }
    }

    /// <summary>
    /// Chamado pelo PlayerController quando ele ENTRA na área da porta.
    /// </summary>
    public void EntrarPortao()
    {
        if (transicaoIniciada) return;

        // Aumenta a contagem de jogadores
        jogadoresNoPortao++;
        Debug.Log($"Jogador entrou no portão. Total: {jogadoresNoPortao} / {JOGADORES_NECESSARIOS}");

        // Verifica se todos os objetivos foram atingidos e se os dois jogadores estão no portão
        if (solColetado && luaColetada && jogadoresNoPortao >= JOGADORES_NECESSARIOS)
        {
            IniciarTransicaoNivel();
        }
        else if (!solColetado || !luaColetada)
        {
            Debug.Log("Faltam colecionáveis.");
        }
        else if (jogadoresNoPortao < JOGADORES_NECESSARIOS)
        {
            Debug.Log($"Aguardando mais {JOGADORES_NECESSARIOS - jogadoresNoPortao} jogador(es).");
        }
    }

    /// <summary>
    /// Chamado quando um PlayerController SAI da área da porta.
    /// (Você deve chamar este método no OnTriggerExit do PlayerController ou da Porta)
    /// </summary>
    public void SairPortao()
    {
        if (transicaoIniciada) return;

        // Diminui a contagem de jogadores
        jogadoresNoPortao--;
        if (jogadoresNoPortao < 0) jogadoresNoPortao = 0;
        Debug.Log($"Jogador saiu do portão. Total: {jogadoresNoPortao} / {JOGADORES_NECESSARIOS}");
    }

    /// <summary>
    /// Inicia a transição de nível após todas as condições serem atendidas.
    /// </summary>
    private void IniciarTransicaoNivel()
    {
        if (transicaoIniciada) return;

        transicaoIniciada = true;
        Debug.Log("CONDIÇÃO DE VITÓRIA ATINGIDA! Transicionando para a próxima cena: " + nomeDaProximaCena);
        SceneManager.LoadScene(nomeDaProximaCena);
    }
}