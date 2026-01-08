using UnityEngine;
public class GoalItem : MonoBehaviour
{
    [Tooltip("Marque se este item � o Sol (para o Cavaleiro da Luz).")]
    public bool isSun = true;

    [SerializeField] private AudioClip collectSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null && GoalManager.Instance != null)
        {
            bool itemColetado = false;

            if (isSun && player.lightPlayer)
            {
                GoalManager.Instance.ColetarSol();
                GoalUI.Instance?.OnSunCollected();   // <- update UI
                GoalObject.Instance?.OnSunCollected();
                itemColetado = true;
                Debug.Log("SunKnight coletou o Sol.");
            }
            else if (!isSun && !player.lightPlayer)
            {
                GoalManager.Instance.ColetarLua();
                GoalUI.Instance?.OnMoonCollected();   // <- update UI
                GoalObject.Instance?.OnMoonCollected();
                itemColetado = true;
                Debug.Log("NightGirl coletou a Lua.");
            }

            if (itemColetado)
            {
                if (collectSound != null)
                {
                    AudioSource.PlayClipAtPoint(collectSound, transform.position, volume);
                }
                gameObject.SetActive(false);
            }
        }
    }

}