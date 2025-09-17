#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Input System provider for stance controls (crouch toggle, run hold).
/// Attach to the Player and bind actions in the Input Action Asset.
/// </summary>
[AddComponentMenu("Stealth/Input/Input System Stance Input")]
public class InputSystemStanceInput : MonoBehaviour, IStanceInput
{
    [SerializeField] InputActionReference crouch; // Button (toggle on press)
    [SerializeField] InputActionReference run;    // Button or axis (hold to run)

    bool crouchTogglePressed;
    bool runHeld;

    void OnEnable()
    {
        crouch?.action.Enable();
        run?.action.Enable();
    }

    void OnDisable()
    {
        crouch?.action.Disable();
        run?.action.Disable();
        crouchTogglePressed = false;
        runHeld = false;
    }

    void Update()
    {
        var ca = crouch ? crouch.action : null;
        if (ca != null && ca.enabled)
        {
            if (ca.WasPressedThisFrame())
                crouchTogglePressed = true;
        }

        var ra = run ? run.action : null;
        runHeld = (ra != null && ra.enabled) && ra.IsPressed();
    }

    public bool CrouchTogglePressed
    {
        get
        {
            bool v = crouchTogglePressed;
            crouchTogglePressed = false; // consume edge
            return v;
        }
    }

    public bool RunHeld => runHeld;
}
#endif
