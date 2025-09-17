#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

[AddComponentMenu("Stealth/Traversal/Input System Climb Input")]
public class InputSystemClimbInput : MonoBehaviour, IClimbInput
{
    [SerializeField] InputActionReference vertical; // bind to Player/Move (Vector2)

    float v;

    void OnEnable() { vertical?.action.Enable(); }
    void OnDisable() { vertical?.action.Disable(); v = 0f; }

    void Update()
    {
        v = 0f;
        var a = vertical ? vertical.action : null;
        if (a != null && a.enabled)
        {
            if (a.expectedControlType == "Vector2") v = a.ReadValue<Vector2>().y;
            else v = a.ReadValue<float>(); // if someone bound an Axis
        }
    }

    public float Vertical => v;
    public bool ClimbHeld => v > 0.2f; // Up = climb
}
#endif
