using UnityEngine;

public class GoalItem : MonoBehaviour
{
    [Tooltip("Marque se este item é o Sol (para o Cavaleiro da Luz).")]
    public bool isSun = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Tenta obter o componente PlayerController do objeto que colidiu
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null && GoalManager.Instance != null)
        {
            bool itemColetado = false;

            // --- LÓGICA DO SOL (Item é Sol E o Personagem é da Luz) ---
            if (isSun && player.lightPlayer)
            {
                GoalManager.Instance.ColetarSol();
                itemColetado = true;
                Debug.Log("SunKnight coletou o Sol.");
            }
            // --- LÓGICA DA LUA (Item NÃO é Sol E o Personagem NÃO é da Luz) ---
            else if (!isSun && !player.lightPlayer)
            {
                GoalManager.Instance.ColetarLua();
                itemColetado = true;
                Debug.Log("NightGirl coletou a Lua.");
            }

            // 3. Se o item foi coletado pelo personagem correto, desativa o item
            if (itemColetado)
            {
                // AÇÃO CHAVE: Desativa o GameObject (faz o Sol/Lua sumir)
                gameObject.SetActive(false);
            }
        }
    }
}