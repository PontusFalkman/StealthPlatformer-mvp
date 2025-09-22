// Scripts/Interact/Throw/InputSystemThrowInput.cs
#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("Stealth/Interact/Input System Throw Input")]
public class InputSystemThrowInput : MonoBehaviour, IThrowInput
{
    [SerializeField] InputActionReference aim;    // Vector2
    [SerializeField] InputActionReference throwAct; // Button
    [SerializeField] InputActionReference dropAct;  // Button

    Vector2 _aim; bool _tp, _dp;

    void OnEnable() { aim?.action.Enable(); throwAct?.action.Enable(); dropAct?.action.Enable(); }
    void OnDisable() { aim?.action.Disable(); throwAct?.action.Disable(); dropAct?.action.Disable(); _aim = Vector2.zero; _tp = _dp = false; }

    void Update()
    {
        var a = aim ? aim.action : null;
        _aim = (a != null && a.enabled) ? a.ReadValue<Vector2>() : Vector2.zero;

        var t = throwAct ? throwAct.action : null;
        if (t != null && t.enabled) _tp |= t.WasPressedThisFrame();

        var d = dropAct ? dropAct.action : null;
        if (d != null && d.enabled) _dp |= d.WasPressedThisFrame();
    }

    public bool ThrowPressed => Consume(ref _tp);
    public bool DropPressed => Consume(ref _dp);
    public Vector2 Aim => _aim.sqrMagnitude < 0.01f ? Vector2.right : Vector2.ClampMagnitude(_aim, 1f);

    static bool Consume(ref bool b) { var v = b; b = false; return v; }
}
#endif
