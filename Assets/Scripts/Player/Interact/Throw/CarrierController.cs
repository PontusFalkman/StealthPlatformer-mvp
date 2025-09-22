// Scripts/Interact/Throw/CarrierController.cs
using UnityEngine;
using Stealth.Inputs;

[AddComponentMenu("Stealth/Interact/Carrier Controller")]
[RequireComponent(typeof(CarryTargetFinder), typeof(CarryHands), typeof(CarryPhysics))]
public class CarrierController : MonoBehaviour
{
    [Header("Input")]
    public MonoBehaviour interactInputProvider; // IInteractInput
    public MonoBehaviour aimInputProvider;      // IAimInput (optional)
    public MonoBehaviour moveInputProvider;     // IPlayerInput (optional)

    [Header("Throw")]
    public float minThrowSpeed = 6f;
    public float maxThrowSpeed = 16f;
    public float chargeTime = 0.6f;
    public float dropPush = 1.5f;
    public float postThrowCooldown = 0.1f;

    [Header("Drop safety")]
    public LayerMask groundMask;
    public float dropProbe = 0.75f;
    public float dropSkin = 0.05f;

    IInteractInput input;
    IAimInput aim;
    IPlayerInput moveInput;

    CarryTargetFinder finder;
    CarryHands hands;
    CarryPhysics phys;

    // held state
    Carryable held;
    CarryPhysics.Snapshot snap;
    float holdT, cooldown;
    bool swallowRelease;

    void Awake()
    {
        finder = GetComponent<CarryTargetFinder>();
        hands = GetComponent<CarryHands>();
        phys = GetComponent<CarryPhysics>();

        input = interactInputProvider as IInteractInput;
        aim = aimInputProvider as IAimInput;
        moveInput = moveInputProvider as IPlayerInput;

        if (groundMask.value == 0) groundMask = LayerMask.GetMask("Ground");
    }

    [System.Obsolete]
    void Update()
    {
        cooldown = Mathf.Max(0, cooldown - Time.deltaTime);
        if (input == null) return;

        if (held)
        {
            if (swallowRelease && input.InteractReleased) { swallowRelease = false; holdT = 0; return; }
            if (input.InteractPressed) holdT = 0;
            if (input.InteractHeld) holdT += Time.deltaTime;

            if (input.InteractReleased)
            {
                if (holdT >= 0.12f) Throw();
                else Drop();
                holdT = 0;
            }
            return;
        }

        if (cooldown <= 0 && input.InteractPressed) TryPick();
    }

    // —— actions ——
    void TryPick()
    {
        if (!finder.TryFind(out var c) || c == null) return;
        held = c;
        snap = phys.MakeHeld(c);
        hands.Attach(c.transform);
        c.OnPicked(hands.hand);
        cooldown = 0.05f;
        swallowRelease = true;
    }

    [System.Obsolete]
    void Drop()
    {
        PlaceSafely();
        phys.ReleaseHeld(snap);

        Vector2 v = AimDir() * dropPush;
        // if grounding directly, kill lateral drift
        bool grounded = Physics2D.Raycast(snap.rb.position, Vector2.down, (HeldBounds().extents.y + 0.08f), groundMask);
        if (grounded) v.x = 0f;

        snap.rb.linearVelocity = v;
        held.OnDropped(false, v);
        ClearHeld();
        cooldown = postThrowCooldown;
    }

    [System.Obsolete]
    void Throw()
    {
        PlaceSafely();
        phys.ReleaseHeld(snap);

        float t = Mathf.Clamp01(holdT / Mathf.Max(0.01f, chargeTime));
        float speed = Mathf.Lerp(minThrowSpeed, maxThrowSpeed, t);
        Vector2 v = AimDir() * speed;

        snap.rb.linearVelocity = v;
        held.OnDropped(true, v);
        ClearHeld();
        cooldown = postThrowCooldown;
    }

    // —— helpers ——
    [System.Obsolete]
    void PlaceSafely()
    {
        hands.Detach(held.transform, snap.originalScene);

        Bounds b = HeldBounds();
        float probe = DynamicProbe();

        // boxcast to find ground
        Vector2 from = snap.rb.position;
        Vector2 box = new(b.size.x * 0.95f, 0.02f);
        var hitBox = Physics2D.BoxCast(from, box, 0f, Vector2.down, probe, groundMask);
        if (hitBox.collider)
            from = hitBox.point + Vector2.up * (dropSkin + b.extents.y);
        else
        {
            Vector2 bottom = (Vector2)b.center + Vector2.down * (b.extents.y - 0.005f);
            var hitRay = Physics2D.Raycast(bottom, Vector2.down, probe, groundMask);
            if (hitRay.collider) from = hitRay.point + Vector2.up * (dropSkin + b.extents.y);
        }

        snap.rb.position = from;
        snap.rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (snap.rb.drag < 0.2f) snap.rb.drag = 0.2f;
    }

    Bounds HeldBounds() => new Bounds(snap.rb ? (Vector2)snap.rb.position : (Vector2)transform.position, Vector3.one * 0.1f);

    float DynamicProbe()
    {
        Vector2 start = hands.hand ? (Vector2)hands.hand.position : (Vector2)transform.position;
        var hit = Physics2D.Raycast(start, Vector2.down, 50f, groundMask);
        float d = hit.collider ? Mathf.Max(0.25f, hit.distance + 0.25f) : 5f;
        return Mathf.Max(d, dropProbe);
    }

    Vector2 AimDir()
    {
        if (aim != null)
        {
            Vector2 a = aim.Aim;
            if (a.sqrMagnitude > 0.0004f) return a.normalized;
        }
        if (moveInput != null && Mathf.Abs(moveInput.MoveX) > 0.1f)
            return new Vector2(Mathf.Sign(moveInput.MoveX), 0f);
        return Vector2.right;
    }

    void ClearHeld()
    {
        held = null;
        snap = default;
        swallowRelease = false;
    }
}
