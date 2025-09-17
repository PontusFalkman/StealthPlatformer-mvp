using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DefaultExecutionOrder(-10)]
public class PlayerMovement : MonoBehaviour
{
    [Header("Refs")]
    public MonoBehaviour inputProvider; // must implement IPlayerInput
    public MonoBehaviour stanceProvider; // must implement IStanceProvider (optional)
    IPlayerInput input;
    IStanceProvider stance;
    Rigidbody2D rb;
    FootstepEmitter steps;                 // optional
    ActionNoiseEmitter actions;            // optional

    [Header("Move")]
    public float maxSpeed = 6f;
    public float accel = 40f;
    public float decel = 50f;
    public float airAccel = 20f;
    public float airDecel = 25f;

    [Header("Jump")]
    public float jumpHeight = 3f;
    public float timeToApex = 0.35f;
    public float coyoteTime = 0.1f;
    public float jumpBuffer = 0.1f;
    public float cutGravityMultiplier = 2.5f;

    [Header("Grounding")]
    public LayerMask groundMask = ~0;
    public Vector2 groundCheckOffset = new Vector2(0f, -0.51f);
    public float groundCheckRadius = 0.1f;

    [Header("Limits")]
    public float terminalFallSpeed = 20f;

    bool grounded, wasGrounded;
    Collider2D groundedCol;
    float coyoteTimer, bufferTimer;
    float gravity, jumpVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        steps = GetComponent<FootstepEmitter>();
        actions = GetComponent<ActionNoiseEmitter>();

        // prefer explicit assignment, else fallback to same GameObject
        input = inputProvider as IPlayerInput;
        if (input == null) input = GetComponent<IPlayerInput>();
        if (input == null && inputProvider != null)
            Debug.LogError("inputProvider must implement IPlayerInput", this);
        if (input == null)
            Debug.LogError("No IPlayerInput found. Add InputSystemPlayerInput or assign inputProvider.", this);

        stance = stanceProvider as IStanceProvider;
        if (stance == null) stance = GetComponent<IStanceProvider>();

        gravity = (2f * jumpHeight) / (timeToApex * timeToApex);
        jumpVel = gravity * timeToApex;

        rb.gravityScale = gravity / Physics2D.gravity.magnitude;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        float prevVy = rb.linearVelocity.y;

        grounded = CheckGrounded(out groundedCol);
        if (grounded) coyoteTimer = coyoteTime; else coyoteTimer -= Time.deltaTime;

        // pass ground tag to footstep noise profile
        if (grounded && steps != null && groundedCol != null)
            steps.currentSurfaceTag = groundedCol.tag;

        bufferTimer -= Time.deltaTime;
        if (input != null && input.JumpPressed) bufferTimer = jumpBuffer;

        if (bufferTimer > 0f && coyoteTimer > 0f)
        {
            bufferTimer = 0f;
            coyoteTimer = 0f;

            var v = rb.linearVelocity;
            v.y = jumpVel;
            rb.linearVelocity = v;

            // noise burst for jump
            actions?.OnJump();
        }

        // variable jump height cut
        if (!grounded && input != null && !input.JumpHeld && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity += Vector2.up * (-gravity * (cutGravityMultiplier - 1f)) * Time.deltaTime;
        }

        // terminal fall clamp
        if (rb.linearVelocity.y < -terminalFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -terminalFallSpeed);

        // land burst when transitioning to grounded
        if (!wasGrounded && grounded)
        {
            float impact = Mathf.Abs(prevVy);
            actions?.OnLand(impact);
        }
        wasGrounded = grounded;

        input?.Consume();
    }

    void FixedUpdate()
    {
        float x = input != null ? Mathf.Clamp(input.MoveX, -1f, 1f) : 0f;
        float stanceMult = stance != null ? stance.SpeedMult : 1f;

        float target = x * maxSpeed * stanceMult;

        float a = grounded
            ? (Mathf.Abs(target) > 0.01f ? accel : decel)
            : (Mathf.Abs(target) > 0.01f ? airAccel : airDecel);

        float vX = Mathf.MoveTowards(rb.linearVelocity.x, target, a * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(vX, rb.linearVelocity.y);

        if (steps != null) steps.currentSpeed = Mathf.Abs(vX);
    }

    bool CheckGrounded(out Collider2D col)
    {
        Vector2 p = (Vector2)transform.position + groundCheckOffset;
        col = Physics2D.OverlapCircle(p, groundCheckRadius, groundMask);
        return col != null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = grounded ? Color.green : Color.red;
        Vector2 p = (Vector2)transform.position + groundCheckOffset;
        Gizmos.DrawWireSphere(p, groundCheckRadius);
    }
#endif
}
