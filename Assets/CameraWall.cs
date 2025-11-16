using UnityEngine;

public class CameraWall : MonoBehaviour
{
    public bool playerTouching = false;

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
            playerTouching = true;
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.CompareTag("Player"))
            playerTouching = false;
    }
}
