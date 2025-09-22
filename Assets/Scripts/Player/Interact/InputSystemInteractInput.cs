// Scripts/Input/InputSystemInteractInput.cs
#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stealth.Inputs
{
    [AddComponentMenu("Stealth/Input/Input System Interact Input")]
    public class InputSystemInteractInput : MonoBehaviour, IInteractInput
    {
        [SerializeField] InputActionReference interact; // Button

        void OnEnable() { interact?.action.Enable(); }
        void OnDisable() { interact?.action.Disable(); }

        public bool InteractPressed => interact && interact.action.WasPressedThisFrame();
        public bool InteractHeld => interact && interact.action.IsPressed();
        public bool InteractReleased => interact && interact.action.WasReleasedThisFrame();
    }
}
#endif
