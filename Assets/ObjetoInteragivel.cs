using UnityEngine;
using System.Collections;

// Este script deve ser anexado ao botão/alavanca/área de interação.
[RequireComponent(typeof(Collider2D))]
public class ObjetoInteragivel : MonoBehaviour
{
    [Header("Configurações de Áudio da Plataforma")]
    [Tooltip("O AudioSource que está lá na PLATAFORMA (não aqui no botão).")]
    public AudioSource audioPlataforma;
    [Tooltip("O som de engrenagens ou motor da plataforma.")]
    public AudioClip somMovimentoPlataforma;
    [Range(0f, 2f)] public float volumeBasePlataforma = 1.0f;


    [Header("Controle de Luz")]
    [Tooltip("Cristal usado para PLATAFORMA (pulsando / par).")]
    public CristalBrilhante controladorDeLuzPlataforma;   // NOVO

    [Tooltip("Cristal usado para VINHA (spotlight que viaja até a vinha).")]
    public Vagalume controladorDeLuzVinha;             // NOVO

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
        if (meuCollider != null && !meuCollider.isTrigger)
        {
            Debug.LogWarning($"O Collider do objeto interagível '{gameObject.name}' NÃO é um Trigger.");
        }

        // Opcional: tentar achar automaticamente o cristal de plataforma se não for atribuído
        if (controladorDeLuzPlataforma == null)
        {
            controladorDeLuzPlataforma = GetComponent<CristalBrilhante>();
        }

        // Opcional: tentar achar automaticamente o cristal de vinha se não for atribuído
        if (controladorDeLuzVinha == null)
        {
            controladorDeLuzVinha = GetComponent<Vagalume>();
        }

        // Se houver cristal de plataforma e a luz já estiver ligada, começa a pulsar.
        if (controladorDeLuzPlataforma != null)
        {
            controladorDeLuzPlataforma.IniciarPulsacao();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagDoJogador))
        {
            Component playerControllerComponent = other.GetComponent("PlayerController");
            if (playerControllerComponent != null)
            {
                other.gameObject.SendMessage("SetProximoInteragivel", this, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(tagDoJogador))
        {
            Component playerControllerComponent = other.GetComponent("PlayerController");
            if (playerControllerComponent != null)
            {
                other.gameObject.SendMessage("ClearProximoInteragivel", this, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    /// <summary>
    /// Chamado pelo PlayerController quando o jogador pressiona o botão de Interagir.
    /// </summary>
    public void PressionarBotao(string tipoAcaoDoJogador)
    {
        if (usoUnico && jaFoiAtivado)
        {
            Debug.Log($"Botão '{gameObject.name}' já foi ativado e é de uso único.");
            return;
        }

        if (tipoAcaoDoJogador != idTipoAcao)
        {
            Debug.Log($"Tentativa de interação: Tipo '{tipoAcaoDoJogador}' não compatível com o botão '{idTipoAcao}'.");
            return;
        }


        bool interacaoComAlvoSucesso = false;

        // =============================
        // CASO 1: PLATAFORMA (Sol)
        // =============================
        if (idTipoAcao == "PLATAFORMA")
        {
            PlataformaMovel alvo = FindTarget<PlataformaMovel>(idDoObjetoAlvo);
            if (alvo != null)
            {
                alvo.Interagir(idDoObjetoAlvo);
                interacaoComAlvoSucesso = true;

                // Cristal de plataforma: para pulsar e ativa o par
                if (controladorDeLuzPlataforma != null)
                {
                    controladorDeLuzPlataforma.AtivarCristal();
                }

                AtivarSomDaPlataforma();
            }
            else
            {
                Debug.LogError($"PlataformaMovel com ID '{idDoObjetoAlvo}' não encontrada.");
            }
        }
        // =============================
        // CASO 2: VINHA (Noite)
        // =============================
        else if (idTipoAcao == "VINHA")
        {
            VinhaDestrutivel alvoVinha = FindTarget<VinhaDestrutivel>(idDoObjetoAlvo);
            if (alvoVinha != null)
            {
                // NÃO chamamos Interagir aqui diretamente.
                // Deixamos o Vagalume mandar a luz e só ao chegar ele chama Interagir.
                if (controladorDeLuzVinha != null)
                {
                    controladorDeLuzVinha.EnviarLuzParaVinha(alvoVinha);
                }
                else
                {
                    // Fallback: se não houver cristal de vinha, usa o comportamento antigo (some na hora).
                    alvoVinha.Interagir(idDoObjetoAlvo);
                }

                interacaoComAlvoSucesso = true;
            }
            else
            {
                Debug.LogError($"VinhaDestrutivel com ID '{idDoObjetoAlvo}' não encontrada.");
            }
        }

        if (interacaoComAlvoSucesso)
        {
            jaFoiAtivado = true;
            if (idTipoAcao == "PLATAFORMA")
            {
                DesativarObjetoVisualmente();
            }
            if (usoUnico && meuCollider != null)
            {
                
            }
        }
    }
    private void DesativarObjetoVisualmente()
    {
        // 1. Esconde o Sprite/Arte
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = false;

        // 2. Desativa Colisor (impede nova interação)
        if (meuCollider != null) meuCollider.enabled = false;

        // 3. Muta o AudioSource LOCAL (o que estiver neste cristal)
        AudioSource audioDesteObjeto = GetComponent<AudioSource>();
        if (audioDesteObjeto != null) audioDesteObjeto.mute = true;

        // 4. (Opcional) Desativa Luzes 2D se existirem
        var lights = GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>();
        foreach (var l in lights) l.enabled = false;
    }

    // Função genérica para encontrar o objeto alvo pelo ID.
    private T FindTarget<T>(string targetID) where T : MonoBehaviour
    {
        foreach (T target in FindObjectsOfType<T>())
        {
            if (typeof(T) == typeof(PlataformaMovel))
            {
                PlataformaMovel pm = target as PlataformaMovel;
                if (pm != null && pm.idPlataforma == targetID)
                    return target;
            }
            else if (typeof(T) == typeof(VinhaDestrutivel))
            {
                VinhaDestrutivel vd = target as VinhaDestrutivel;
                if (vd != null && vd.idVinha == targetID)
                    return target;
            }
        }
        return null;
    }
    private void AtivarSomDaPlataforma()
    {
        if (audioPlataforma != null && somMovimentoPlataforma != null)
        {
            audioPlataforma.clip = somMovimentoPlataforma;
            audioPlataforma.loop = true;
            audioPlataforma.volume = volumeBasePlataforma; // Define o volume inicial alto

            if (!audioPlataforma.isPlaying)
            {
                audioPlataforma.Play();
                StartCoroutine(VariarSomNaturalmente());
            }
        }
    }

    IEnumerator VariarSomNaturalmente()
    {
        while (audioPlataforma.isPlaying)
        {
            // Variação leve de tom (Pitch)
            float novoPitch = UnityEngine.Random.Range(0.87f, 1.13f);

            // Variação leve de volume (sempre próxima ao volume base que você definiu)
            float variacaoVolume = UnityEngine.Random.Range(0.9f, 1.1f);
            float novoVolume = volumeBasePlataforma * variacaoVolume;

            float tempoDeTransicao = 0.8f; // Transição mais lenta para ser menos perceptível
            float tempoPassado = 0;

            float pitchInicial = audioPlataforma.pitch;
            float volumeInicial = audioPlataforma.volume;

            while (tempoPassado < tempoDeTransicao)
            {
                tempoPassado += Time.deltaTime;
                float t = tempoPassado / tempoDeTransicao;

                audioPlataforma.pitch = Mathf.Lerp(pitchInicial, novoPitch, t);
                audioPlataforma.volume = Mathf.Lerp(volumeInicial, novoVolume, t);
                yield return null;
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
        }
    }
}
