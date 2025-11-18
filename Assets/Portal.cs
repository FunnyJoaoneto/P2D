using UnityEngine;
using System.Collections;

public class Portal: MonoBehaviour
{
    public Transform targetPortal;
    public bool CanEnter = true;

private void  OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Player") && CanEnter)
        {
            Debug.Log("colidiu");
            StartCoroutine(EnterPortal(collision.transform));
        }

    }
    IEnumerator EnterPortal(Transform player)
    {
        Debug.Log("chamou enter portal");
        player.position = targetPortal.position + new Vector3(-2,0,0);
        CanEnter = false;
        yield return new WaitForSeconds(3);
        CanEnter = true;
    }
    
}
