using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicStarter : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    [Tooltip("Tempo em segundos onde a música deve começar")]
    public float startTimeInSeconds;

    private AudioSource _audioSource;

    void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        // 1. Garante que o Play On Awake do componente está desativado no Inspector 
        // para evitar que a música comece no segundo 0 antes do script correr.

        // 2. Define o tempo de início
        if (startTimeInSeconds < _audioSource.clip.length)
        {
            _audioSource.time = startTimeInSeconds;
        }
        else
        {
            Debug.LogWarning("O tempo de início é maior que a duração da música!");
        }

        // 3. Toca a música
        _audioSource.Play();
    }
}