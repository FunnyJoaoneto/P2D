using UnityEngine;

public class PlataformaMovel : MonoBehaviour
{
    // ===========================================
    // NOVO: ID DE ATIVAÇÃO
    // ===========================================
    [Header("Configuração de Interação")]
    [Tooltip("ID único desta plataforma. Deve corresponder ao 'ID Objeto Alvo' no botão.")]
    public string idPlataforma; 
    
    // Variável para controlar se a plataforma está autorizada a se mover
    [SerializeField]
    private bool isAtivada = false;

    // ===========================================
    // PARÂMETROS EXISTENTES
    // ===========================================
    [Header("Configuração de Movimento")]
    [Tooltip("Define se o movimento será no eixo X (Horizontal) ou Y (Vertical).")]
    public EixoDeMovimento eixoDeMovimento = EixoDeMovimento.X;
    
    [Tooltip("O número de unidades que a plataforma irá percorrer a partir do seu ponto inicial.")]
    public float distanciaDoMovimento = 10f;
    
    [Tooltip("A velocidade de movimento da plataforma (unidades por segundo).")]
    public float velocidade = 2f;

    // Enum para tornar a escolha do eixo mais clara no Inspetor
    public enum EixoDeMovimento { X, Y }

    private Vector3 posicaoInicial;
    private Vector3 posicaoFinal;

    // Variável para rastrear a direção/destino atual do movimento cíclico
    private Vector3 destinoAtual; 

    void Start()
    {
        // Armazena a posição inicial da plataforma
        posicaoInicial = transform.position;
        destinoAtual = posicaoFinal; // O movimento começará indo para a posição final

        // Calcula a posição final com base no eixo e na distância definidos
        if (eixoDeMovimento == EixoDeMovimento.X)
        {
            posicaoFinal = posicaoInicial + new Vector3(distanciaDoMovimento, 0, 0);
        }
        else // Eixo Y
        {
            posicaoFinal = posicaoInicial + new Vector3(0, distanciaDoMovimento, 0);
        }
        
        // NOVO: Se a plataforma começar desativada, o destino inicial é o próprio ponto inicial
        // (Isso é crucial se o primeiro comando de Interagir for o de ativar/desativar)
        destinoAtual = posicaoFinal;
        transform.position = posicaoInicial; // Garante que ela comece no ponto inicial
    }

    // ===========================================
    // MÉTODO DE INTERAÇÃO (Chamado pelo ObjetoInteragivel.cs)
    // ===========================================
    /// <summary>
    /// Ativa (liga/desliga) o movimento cíclico da plataforma.
    /// </summary>
    /// <param name="idDeAtivacao">O ID enviado pelo ObjetoInteragivel (para checagem).</param>
    public void Interagir(string idDeAtivacao) 
    {
        // Checa se o ID passado é o mesmo do objeto
        if (idDeAtivacao != idPlataforma)
        {
            Debug.LogWarning($"Tentativa de ativar Plataforma ID {idPlataforma} com ID errado: {idDeAtivacao}");
            return;
        }

        // NOVO: Faz um TOGGLE (liga/desliga) no movimento
        isAtivada = !isAtivada; 

        if (isAtivada)
        {
            // Define o destino para onde ela deve ir se estiver no ponto inicial
            // Se já estiver em movimento, o Update cuidará da direção
            Debug.Log($"Plataforma '{idPlataforma}' ATIVADA. Iniciando movimento cíclico.");
        }
        else
        {
            Debug.Log($"Plataforma '{idPlataforma}' DESATIVADA. Parando no local atual.");
        }
    }

    void Update()
    {
        // O movimento só acontece se a plataforma estiver ativada
        if (!isAtivada)
        {
            return;
        }

        // Move a plataforma em direção ao seu destino atual (posicaoInicial ou posicaoFinal)
        transform.position = Vector3.MoveTowards(
            transform.position, 
            destinoAtual, 
            velocidade * Time.deltaTime
        );
        
        // Verifica se o movimento chegou ao destino
        if (transform.position == destinoAtual)
        {
            // Se chegou ao destino, inverte o destino para criar o efeito "vai e volta"
            if (destinoAtual == posicaoFinal)
            {
                destinoAtual = posicaoInicial;
            }
            else if (destinoAtual == posicaoInicial)
            {
                destinoAtual = posicaoFinal;
            }
        }
    }

    // Garante que a plataforma leve objetos (incluindo o jogador) junto
    private void OnCollisionEnter2D(Collision2D other)
    {
        // Apenas faça o jogador (ou outro objeto) filho se for um Rigidbody
        if (other.rigidbody != null)
        {
            other.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        // Garante que o objeto não seja nulo antes de remover o parent
        if (other.rigidbody != null)
        {
            other.transform.SetParent(null);
        }
    }
}