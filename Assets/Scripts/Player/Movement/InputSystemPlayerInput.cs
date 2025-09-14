#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemPlayerInput : MonoBehaviour, IPlayerInput
{
    [SerializeField] InputActionReference move;   // Player/Move (Value, Vector2)
    [SerializeField] InputActionReference jump;   // Player/Jump (Button)

    float _moveX;
    bool _jumpPressed, _jumpHeld;

    void OnEnable() { move?.action.Enable(); jump?.action.Enable(); }
    void OnDisable() { move?.action.Disable(); jump?.action.Disable(); _moveX = 0f; _jumpHeld = _jumpPressed = false; }

    void Update()
    {
        _moveX = 0f; _jumpHeld = false;

        var ma = move ? move.action : null;
        if (ma != null && ma.enabled)
            _moveX = ma.ReadValue<Vector2>().x;   // default template uses Vector2

        var ja = jump ? jump.action : null;
        if (ja != null && ja.enabled)
        {
            _jumpPressed |= ja.WasPressedThisFrame();
            _jumpHeld = ja.IsPressed();
        }
    }

    public float MoveX => _moveX;
    public bool JumpPressed => _jumpPressed;
    public bool JumpHeld => _jumpHeld;
    public void Consume() { _jumpPressed = false; }
}
#endif
