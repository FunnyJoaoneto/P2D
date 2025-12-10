using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class Vagalume : MonoBehaviour
{
    [Header("Referências")]
    [Tooltip("Spot Light 2D que irá 'viajar' até a vinha.")]
    [SerializeField] private Light2D minhaLuz;

    [Header("Configuração do Feixe")]
    [Tooltip("Velocidade da luz ao viajar até a vinha (unidades por segundo).")]
    public float velocidadeFeixe = 8f;

    [Tooltip("Tempo adicional após chegar na vinha antes de chamá-la para destruir.")]
    public float atrasoAoChegar = 0.1f;

    [Tooltip("Curva opcional para controlar a aceleração da luz (0-1). Se vazio, usa linear.")]
    public AnimationCurve curvaProgresso;

    private void Awake()
    {
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
            Debug.LogError($"'{name}' precisa de um Light2D (Spot) para funcionar como CristalVinha.");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Chamado pelo ObjetoInteragivel quando o jogador interage.
    /// A luz sai do cristal e vai até a vinha; ao chegar, a vinha é destruída.
    /// </summary>
    public void EnviarLuzParaVinha(VinhaDestrutivel vinha)
    {
        if (vinha == null)
        {
            Debug.LogWarning($"CristalVinha '{name}' recebeu vinha nula.");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(MoverLuzAteVinhaCoroutine(vinha));
    }

    private IEnumerator MoverLuzAteVinhaCoroutine(VinhaDestrutivel vinha)
    {
        if (minhaLuz == null || vinha == null)
            yield break;

        minhaLuz.enabled = true;

        Vector3 origem = minhaLuz.transform.position;
        Vector3 destino = vinha.transform.position;

        float distancia = Vector3.Distance(origem, destino);
        if (distancia <= 0.01f)
        {
            // Já está praticamente em cima da vinha.
            vinha.Interagir(vinha.idVinha);
            yield break;
        }

        float duracao = distancia / Mathf.Max(velocidadeFeixe, 0.01f);
        float t = 0f;

        while (t < duracao)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracao);

            // Se tiver curva, usa; senão fica linear.
            if (curvaProgresso != null && curvaProgresso.length > 0)
                p = curvaProgresso.Evaluate(p);

            Vector3 pos = Vector3.Lerp(origem, destino, p);
            minhaLuz.transform.position = pos;

            // Opcional: fazer o spotlight olhar na direção da vinha.
            Vector3 dir = destino - pos;
            float angulo = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            minhaLuz.transform.rotation = Quaternion.AngleAxis(angulo - 90f, Vector3.forward);

            yield return null;
        }

        // Garante que terminou exatamente no alvo
        minhaLuz.transform.position = destino;

        // Aguarda um pequeno atraso, se desejado
        if (atrasoAoChegar > 0f)
            yield return new WaitForSeconds(atrasoAoChegar);

        // Agora sim pede para a vinha se destruir
        if (vinha != null)
        {
            // Usando o mesmo fluxo de ID que você já tem
            vinha.Interagir(vinha.idVinha);
        }

        // Opcional: apagar a luz depois
        minhaLuz.enabled = false;
    }
}
