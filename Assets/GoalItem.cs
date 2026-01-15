using UnityEngine;

public class GoalItem : MonoBehaviour
{
    [Tooltip("Marque se este item é o Sol (para o Cavaleiro da Luz).")]
    public bool isSun = true;

    [Header("Áudio")]
    public AudioSource collectSoundSource;
    public AudioClip collectSoundClip;
    [Range(0f, 1f)] public float volume = 1f;

    private bool foiColetado = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (foiColetado) return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null && GoalManager.Instance != null)
        {
            bool itemValido = false;

            if (isSun && player.lightPlayer)
            {
                GoalManager.Instance.ColetarSol();
                GoalUI.Instance?.OnSunCollected();
                GoalObject.Instance?.OnSunCollected();
                itemValido = true;
                Debug.Log("SunKnight coletou o Sol.");
            }
            else if (!isSun && !player.lightPlayer)
            {
                GoalManager.Instance.ColetarLua();
                GoalUI.Instance?.OnMoonCollected();
                GoalObject.Instance?.OnMoonCollected();
                itemValido = true;
                Debug.Log("NightGirl coletou a Lua.");
            }

            if (itemValido)
            {
                Coletar();
            }
        }
    }

    private void Coletar()
    {
        foiColetado = true;

        // Toca o som se as referências existirem
        if (collectSoundSource != null && collectSoundClip != null)
        {
            // O erro na linha 38 era aqui. O correto é PlayOneShot.
            collectSoundSource.PlayOneShot(collectSoundClip, volume);
        }

        // --- IMPORTANTE: Para o som não cortar ---
        // Desativamos o visual e o colisor em vez de desativar o objeto inteiro
        if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = false;
        if (GetComponent<Collider2D>() != null) GetComponent<Collider2D>().enabled = false;

        // Destrói o objeto apenas depois que o som acabar (ex: daqui a 2 segundos)
        Destroy(gameObject, collectSoundClip != null ? collectSoundClip.length : 0.1f);
    }
}