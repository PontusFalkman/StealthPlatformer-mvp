// Scripts/Player/InputSystemAimInput.cs
#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;
using Stealth.Inputs; // use the shared interface

[AddComponentMenu("Stealth/Input/Input System Aim Input")]
public class InputSystemAimInput : MonoBehaviour, IAimInput
{
    [SerializeField] InputActionReference aim; // Player/Aim (Vector2)
    Vector2 v;

    void OnEnable() { aim?.action.Enable(); }
    void OnDisable() { aim?.action.Disable(); v = Vector2.zero; }
    void Update() { var a = aim ? aim.action : null; v = (a != null && a.enabled) ? a.ReadValue<Vector2>() : Vector2.zero; }

    public Vector2 Aim => v;            // required by Stealth.Inputs.IAimInput
    public bool IsActive => v.sqrMagnitude > 0.01f; // optional helper
}
#endif
