using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CristalBrilhante : MonoBehaviour
{
    [Header("Configurações da Pulsação")]
    [Tooltip("A Light 2D que será controlada (deve ser um componente filho ou no mesmo objeto).")]
    [SerializeField] private Light2D minhaLuz;

    [Tooltip("Intensidade máxima da pulsação.")]
    public float intensidadeMaxima = 2.0f;

    [Tooltip("Intensidade mínima da pulsação.")]
    public float intensidadeMinima = 1.0f;

    [Tooltip("Velocidade do ciclo de pulsação (maior valor = pulsação mais rápida).")]
    public float velocidadePulsacao = 1.5f;

    [Header("Configurações do Cristal Par (Opcional)")]
    [Tooltip("Referência ao Cristal Brilhante par (use APENAS para cristais tipo PLATAFORMA).")]
    public CristalBrilhante cristalPar; // Se for null, não fará nada.

    // A pulsação começa como false e só é ativada se a luz estiver ligada no Awake, ou por LigarCristalPar()
    private bool estaPulsando = false; 

    void Awake()
    {
        // 1. Garante que a referência à luz exista
        if (minhaLuz == null)
        {
            minhaLuz = GetComponentInChildren<Light2D>();
            if (minhaLuz == null)
            {
                minhaLuz = GetComponent<Light2D>();
            }
        }

        if (minhaLuz == null)
        {
            Debug.LogError($"O objeto '{gameObject.name}' precisa de um Light2D atribuído ou como componente filho.");
            enabled = false;
            return;
        }

        // 2. CORREÇÃO: Inicia a pulsação APENAS se o componente Light 2D já estiver ativo no Inspector.
        // Isso garante que o cristal par (com Light 2D desativado) comece apagado.
        if (minhaLuz.enabled)
        {
            IniciarPulsacao();
            // Define a intensidade inicial para o valor mínimo para começar o ciclo suavemente.
            minhaLuz.intensity = intensidadeMinima; 
        }
    }

    void Update()
    {
        if (estaPulsando)
        {
            // Usa a função seno para criar um movimento suave de ida e volta (pulsação)
            // t varia de 0 a 1
            float t = (Mathf.Sin(Time.time * velocidadePulsacao) + 1f) / 2f; 

            // Interpola a intensidade entre o mínimo e o máximo
            minhaLuz.intensity = Mathf.Lerp(intensidadeMinima, intensidadeMaxima, t);
        }
    }

    /// <summary>
    /// Inicia a pulsação da luz.
    /// </summary>
    public void IniciarPulsacao()
    {
        if (minhaLuz == null) return;
        estaPulsando = true;
        minhaLuz.enabled = true;
    }

    /// <summary>
    /// Para a pulsação e desliga a luz deste cristal.
    /// Chamado pelo ObjetoInteragivel após a interação.
    /// </summary>
    public void AtivarCristal()
    {
        if (minhaLuz == null) return;

        // 1. Para a Pulsação
        estaPulsando = false;

        // 2. Desliga a Luz deste cristal
        minhaLuz.enabled = false;
        minhaLuz.intensity = 0f;

        // 3. Ativa o cristal par, SE HOUVER.
        if (cristalPar != null)
        {
            cristalPar.LigarCristalPar();
        }
    }

    /// <summary>
    /// Usado pelo cristal par para ligar a sua própria luz e começar a pulsar.
    /// </summary>
    public void LigarCristalPar()
    {
        minhaLuz.enabled = true;
        // Chama IniciarPulsacao para ligar a luz e ativar a flag 'estaPulsando'
        IniciarPulsacao(); 
    }
}