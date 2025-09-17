#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

namespace Stealth.Interact
{
    [DefaultExecutionOrder(-100)] // poll before Interactor
    [AddComponentMenu("Stealth/Interact/Input System Interact Input")]
    public class InputSystemInteractInput : MonoBehaviour, IInteractInput
    {
        [Header("Binding")]
        [SerializeField] private InputActionReference interact; // optional
        [SerializeField] private string actionNameFallback = "Interact"; // used if reference not set

        InputAction _action;
        int _pf = -1, _rf = -1;
        bool _held;

        void OnEnable()
        {
            ResolveAction();
            _action?.Enable();
        }

        void OnDisable()
        {
            _action?.Disable();
            _held = false; _pf = _rf = -1;
        }

        void Update()
        {
            if (_action == null || !_action.enabled) { _held = false; return; }

            if (_action.WasPressedThisFrame()) _pf = Time.frameCount;
            if (_action.WasReleasedThisFrame()) _rf = Time.frameCount;
            _held = _action.IsPressed();
        }

        public bool InteractPressed => _pf == Time.frameCount;
        public bool InteractReleased => _rf == Time.frameCount;
        public bool InteractHeld => _held;

        void ResolveAction()
        {
            _action = null;
            if (interact != null) _action = interact.action;
            if (_action != null) return;

            var pi = GetComponent<PlayerInput>();
            if (pi != null && pi.actions != null && !string.IsNullOrEmpty(actionNameFallback))
                _action = pi.actions.FindAction(actionNameFallback, throwIfNotFound: false);
        }
    }
}
#endif
