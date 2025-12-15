using UnityEngine;
using System.Collections; // Necessário para usar Coroutines

public class VinhaDestrutivel : MonoBehaviour
{
    // ===========================================
    // CONFIGURAÇÃO
    // ===========================================
    [Header("Configuração de Destruição")]
    [Tooltip("ID único desta Vinha. Deve corresponder ao 'ID Objeto Alvo' no botão.")]
    public string idVinha; 

    [Tooltip("Tempo em segundos de atraso para a destruição (duração da animação).")]
    public float tempoDeAtraso = 0.5f; // Meio segundo (500ms) por padrão

    private bool jaDestruindo = false;

    // ===========================================
    // MÉTODO DE INTERAÇÃO (Chamado pelo ObjetoInteragivel.cs)
    // ===========================================

    /// <summary>
    /// Inicia o processo de destruição da vinha com um atraso para a animação.
    /// </summary>
    /// <param name="idDeAtivacao">O ID enviado pelo ObjetoInteragivel (para checagem).</param>
    public void Interagir(string idDeAtivacao)
    {
        if (jaDestruindo)
            return;

        if (idDeAtivacao != idVinha)
        {
            Debug.LogWarning($"Tentativa de destruir Vinha ID {idVinha} com ID errado: {idDeAtivacao}");
            return;
        }

        jaDestruindo = true;
        Debug.Log($"Vinha '{idVinha}' ativada. Aguardando {tempoDeAtraso}s para a destruição.");

        // Opcional: Inicie a animação de destruição (ex: um Animator.SetTrigger("Destruir"))

        // Inicia a Coroutine para atrasar a destruição
        StartCoroutine(AtrasarDestruicao());
    }

    /// <summary>
    /// Coroutine para esperar pelo atraso e, em seguida, destruir o objeto.
    /// </summary>
    private IEnumerator AtrasarDestruicao()
    {
        // Espera pelo tempo definido no Inspector
        yield return new WaitForSeconds(tempoDeAtraso);

        // Destrói este GameObject (a Vinha)
        Destroy(gameObject);
        Debug.Log($"Vinha '{idVinha}' destruída após o atraso.");

        // Se o objeto interagível for de uso único, desabilita ele também.
        // O ObjetoInteragivel pode ter o usoUnico=true configurado no Inspector.
    }
}