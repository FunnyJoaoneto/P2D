using UnityEngine;
using System.Collections; // Adicionado para uso futuro, se necessário

// Este script deve ser anexado ao botão/alavanca/área de interação.
[RequireComponent(typeof(Collider2D))] // Garante que o objeto tem um collider (deve ser Trigger)
public class ObjetoInteragivel : MonoBehaviour
{
    // --- Configurações no Inspector ---

    [Tooltip("ID único do objeto alvo (ex: Plataforma_A, Vinha_B). Deve corresponder ao ID do alvo.")]
    public string idDoObjetoAlvo;
    
    [Tooltip("Tipo de Ação que este botão realiza (ex: PLATAFORMA para o Luz, VINHA para o Noite).")]
    public string idTipoAcao = "PLATAFORMA"; 

    [Tooltip("A Tag do Player que pode interagir (geralmente 'Player').")]
    public string tagDoJogador = "Player";
    
    [Tooltip("Se o botão só pode ser usado uma vez.")]
    public bool usoUnico = true;

    // --- Variáveis de Estado ---

    private bool jaFoiAtivado = false;
    private Collider2D meuCollider;

    void Awake()
    {
        meuCollider = GetComponent<Collider2D>();
        // Garante que o Collider é um Trigger para funcionar como área de proximidade
        if (meuCollider != null && !meuCollider.isTrigger)
        {
            Debug.LogWarning($"O Collider do objeto interagível '{gameObject.name}' NÃO é um Trigger. Ele deve ser configurado como Trigger para funcionar corretamente.");
        }
    }

    // --- Comunicação com o PlayerController (Proximidade) ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagDoJogador))
        {
            // Tenta obter o PlayerController do objeto que entrou na área
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Notifica o jogador que ele está perto de um objeto interagível
                playerController.SetProximoInteragivel(this);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(tagDoJogador))
        {
            // Tenta obter o PlayerController do objeto que saiu da área
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Notifica o jogador que ele se afastou
                playerController.ClearProximoInteragivel(this);
            }
        }
    }

    // --- Lógica de Interação (Chamada pelo PlayerController via Input) ---

    /// <summary>
    /// Este método é chamado pelo PlayerController quando o jogador pressiona o botão de Interagir (E/L).
    /// </summary>
    /// <param name="tipoAcaoDoJogador">O ID de ação que o jogador está tentando executar (ex: "PLATAFORMA" ou "VINHA").</param>
    public void PressionarBotao(string tipoAcaoDoJogador)
    {
        if (usoUnico && jaFoiAtivado)
        {
            Debug.Log($"Botão '{gameObject.name}' já foi ativado e é de uso único.");
            return;
        }

        // 1. CHECA COMPATIBILIDADE: O tipo de ação do jogador deve corresponder ao tipo do botão.
        if (tipoAcaoDoJogador != idTipoAcao)
        {
            Debug.Log($"Tentativa de interação: Tipo de ação '{tipoAcaoDoJogador}' do jogador não é compatível com o tipo de botão '{idTipoAcao}'.");
            return;
        }

        // 2. BUSCA E ATIVA O ALVO: Procura o script de controle do alvo pelo ID.
        
        // Se for um botão de Plataforma (Jogador Luz)
        if (idTipoAcao == "PLATAFORMA")
        {
            PlataformaMovel alvo = FindTarget<PlataformaMovel>(idDoObjetoAlvo);
            if (alvo != null)
            {
                alvo.Interagir(idDoObjetoAlvo);
                jaFoiAtivado = true;
            }
            else
            {
                Debug.LogError($"Alvo PlataformaMovel com ID '{idDoObjetoAlvo}' não encontrado na cena! Verifique o script PlataformaMovel.");
            }
        }
        // SE FOR UM BOTÃO DE VINHA (Jogador Noite)
        else if (idTipoAcao == "VINHA")
        {
            // Busca o script VinhaDestrutivel (que contém a lógica de atraso e destruição)
            VinhaDestrutivel alvoVinha = FindTarget<VinhaDestrutivel>(idDoObjetoAlvo);
            if (alvoVinha != null)
            {
                alvoVinha.Interagir(idDoObjetoAlvo);
                jaFoiAtivado = true;
            }
            else
            {
                Debug.LogError($"Alvo VinhaDestrutivel com ID '{idDoObjetoAlvo}' não encontrado na cena! Verifique o script VinhaDestrutivel.");
            }
        }
        // Se for de uso único, desabilita o collider ou o script após a ativação
        if (usoUnico && jaFoiAtivado)
        {
            // Opcional: desabilita o collider para não interagir novamente
            // meuCollider.enabled = false;
        }
    }

    // Função Genérica para encontrar o objeto alvo pelo ID.
    private T FindTarget<T>(string targetID) where T : MonoBehaviour
    {
        foreach (T target in FindObjectsOfType<T>())
        {
            // Checagem específica para PlataformaMovel
            if (typeof(T) == typeof(PlataformaMovel))
            {
                PlataformaMovel pm = target as PlataformaMovel;
                if (pm != null && pm.idPlataforma == targetID)
                {
                    return target;
                }
            }
            // NOVO: Checagem específica para VinhaDestrutivel
            else if (typeof(T) == typeof(VinhaDestrutivel))
            {
                VinhaDestrutivel vd = target as VinhaDestrutivel;
                if (vd != null && vd.idVinha == targetID)
                {
                    return target;
                }
            }
            // Adicionar mais checagens para outros tipos de alvo (ex: PortaMovel) aqui
        }
        return null;
    }
}