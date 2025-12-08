using UnityEngine;

public class GoalItem : MonoBehaviour
{
    [Tooltip("Marque se este item � o Sol (para o Cavaleiro da Luz).")]
    public bool isSun = true;

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
                itemColetado = true;
                Debug.Log("SunKnight coletou o Sol.");
            }
            else if (!isSun && !player.lightPlayer)
            {
                GoalManager.Instance.ColetarLua();
                GoalUI.Instance?.OnMoonCollected();   // <- update UI
                itemColetado = true;
                Debug.Log("NightGirl coletou a Lua.");
            }

            if (itemColetado)
            {
                gameObject.SetActive(false);
            }
        }
    }

}