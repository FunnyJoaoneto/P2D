using UnityEngine;
using UnityEngine.InputSystem;

public class PauseRelay : MonoBehaviour
{
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        var listener = FindFirstObjectByType<PauseListener>();
        if (listener != null)
            listener.HandlePause();
    }
}
