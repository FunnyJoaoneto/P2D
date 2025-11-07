using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class DebugInputProbe : MonoBehaviour
{
    [Tooltip("Seconds between logs for the same control to avoid spam.")]
    public float cooldown = 0.25f;

    private float _nextLogTime;

    private void Start()
    {
        Debug.Log("=== Input devices detected ===");
        foreach (var d in InputSystem.devices)
            Debug.Log(DeviceSummary(d));
        Debug.Log("==============================");
    }

    private void Update()
    {
        // throttle logs
        if (Time.unscaledTime < _nextLogTime) return;

        bool logged = false;

        // 1) Buttons on ANY device
        foreach (var device in InputSystem.devices)
        {
            foreach (var c in device.allControls)
            {
                if (c is ButtonControl b && b.wasPressedThisFrame)
                {
                    Debug.Log($"[BUTTON] {DeviceSummary(device)}  Control={c.path}  DisplayName={c.displayName}");
                    logged = true;
                }
            }
        }

        // 2) Sticks/D-pad movement on gamepads (useful if buttons don’t register)
        foreach (var gp in Gamepad.all)
        {
            if (gp.leftStick.ReadValue().sqrMagnitude > 0.25f)
            {
                Debug.Log($"[STICK] {DeviceSummary(gp)}  leftStick={gp.leftStick.ReadValue()}");
                logged = true;
            }
            if (gp.rightStick.ReadValue().sqrMagnitude > 0.25f)
            {
                Debug.Log($"[STICK] {DeviceSummary(gp)}  rightStick={gp.rightStick.ReadValue()}");
                logged = true;
            }
            if (gp.dpad.up.wasPressedThisFrame || gp.dpad.down.wasPressedThisFrame ||
                gp.dpad.left.wasPressedThisFrame || gp.dpad.right.wasPressedThisFrame)
            {
                var v = gp.dpad.ReadValue();
                Debug.Log($"[DPAD]  {DeviceSummary(gp)}  dpad={v}");
                logged = true;
            }
        }

        if (logged) _nextLogTime = Time.unscaledTime + cooldown;
    }

    private static string DeviceSummary(InputDevice d)
    {
        // layout = device type Unity mapped it to (e.g., Gamepad, DualShockGamepadHID, XInputControllerWindows)
        // interface/product give OS-level details
        var desc = d.description;
        return $"Device='{d.displayName}' Layout={d.layout} Usages=[{string.Join(",", d.usages)}] " +
               $"Interface='{desc.interfaceName}' Product='{desc.product}' Manufacturer='{desc.manufacturer}'";
    }
}
