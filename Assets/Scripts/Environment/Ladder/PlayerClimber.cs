using System;
using UnityEngine;

[DefaultExecutionOrder(10)]
[RequireComponent(typeof(Rigidbody2D))]
[AddComponentMenu("Stealth/Traversal/Player Climber")]
public class PlayerClimber : MonoBehaviour
{
    public enum LadderIdle { Hold, Slide, GraceThenSlide }

    [Header("Input")]
    public MonoBehaviour climbInputProvider;        // IClimbInput (reads Move.y)
    public MonoBehaviour moveInputProvider;         // IPlayerInput (reads Move.x)  << NEW
    IClimbInput climbInput;
    IPlayerInput moveInput;                         // << NEW

    [Header("Tune")]
    public float climbSpeed = 3.5f;
    public float enterThreshold = 0.2f;
    public float exitGrace = 0.15f;
    public float stepEveryUnits = 0.45f;

    [Header("Idle behavior")]
    public LadderIdle idleMode = LadderIdle.Hold;
    public float slideSpeed = 2.0f;
    public float idleGrace = 0.35f;

    [Header("Exit control")]
    public bool zeroYOnExit = true;
    [Tooltip("Break climb if |MoveX| exceeds this while climbing.")]
    public float sidewaysExitThreshold = 0.2f;      // << NEW

    [Header("State (read-only)")]
    public bool isInLadder;
    public bool isClimbing;
    public LadderZone currentLadder;

    public event Action OnClimbStep;
    public event Action<bool> OnClimbStateChanged;

    Rigidbody2D rb;
    float graceTimer, stepAccum, idleTimer;
    bool wasClimbing;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        climbInput = climbInputProvider as IClimbInput;
        if (climbInput == null && climbInputProvider != null)
            Debug.LogError("climbInputProvider must implement IClimbInput", this);

        moveInput = moveInputProvider as IPlayerInput;             // << NEW
        if (moveInputProvider && moveInput == null)
            Debug.LogError("moveInputProvider must implement IPlayerInput", this);
    }

    void Update()
    {
        float v = climbInput != null ? climbInput.Vertical : 0f;
        bool hold = climbInput != null && climbInput.ClimbHeld;

        if (!isInLadder) graceTimer -= Time.deltaTime;
        bool wantClimb = (Mathf.Abs(v) > enterThreshold) || hold;

        if (!isClimbing)
        {
            if (currentLadder && (isInLadder || graceTimer > 0f) && wantClimb)
                BeginClimb();
        }

        if (isClimbing)
        {
            // exit if left ladder volume
            if (!currentLadder || (!isInLadder && graceTimer <= 0f))
            {
                EndClimb();
            }
            else
            {
                // NEW: exit on sideways input
                float hx = moveInput != null ? moveInput.MoveX : 0f;
                if (Mathf.Abs(hx) >= sidewaysExitThreshold)
                {
                    EndClimb();
                }
                else
                {
                    Vector2 up = currentLadder.UpNorm;
                    rb.gravityScale = 0f;

                    if (Mathf.Abs(v) <= 0.01f)
                    {
                        switch (idleMode)
                        {
                            case LadderIdle.Hold:
                                rb.linearVelocity = Vector2.zero; idleTimer = 0f; break;
                            case LadderIdle.Slide:
                                rb.linearVelocity = new Vector2(0f, -Mathf.Abs(slideSpeed)); idleTimer = 0f; break;
                            case LadderIdle.GraceThenSlide:
                                idleTimer += Time.deltaTime;
                                rb.linearVelocity = (idleTimer >= idleGrace)
                                    ? new Vector2(0f, -Mathf.Abs(slideSpeed))
                                    : Vector2.zero;
                                break;
                        }
                    }
                    else
                    {
                        idleTimer = 0f;
                        Vector2 vel = up * (v * climbSpeed);
                        rb.linearVelocity = new Vector2(0f, vel.y);
                    }

                    if (currentLadder.snapXToCenter)
                    {
                        var p = rb.position; p.x = currentLadder.CenterX; rb.position = p;
                    }

                    float vyAbs = Mathf.Abs(rb.linearVelocity.y);
                    stepAccum += vyAbs * Time.deltaTime;
                    if (stepEveryUnits > 0f && stepAccum >= stepEveryUnits)
                    {
                        stepAccum = 0f; OnClimbStep?.Invoke();
                    }
                }
            }
        }

        if (zeroYOnExit && wasClimbing && !isClimbing)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        wasClimbing = isClimbing;
    }

    void BeginClimb()
    {
        isClimbing = true;
        rb.gravityScale = 0f;
        stepAccum = 0f; idleTimer = 0f;
        OnClimbStateChanged?.Invoke(true);
    }

    void EndClimb()
    {
        isClimbing = false;
        rb.gravityScale = 1f;
        if (zeroYOnExit) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        OnClimbStateChanged?.Invoke(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var ladder = other.GetComponent<LadderZone>();
        if (!ladder) return;
        currentLadder = ladder; isInLadder = true; graceTimer = exitGrace;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var ladder = other.GetComponent<LadderZone>();
        if (!ladder || ladder != currentLadder) return;
        isInLadder = false; graceTimer = exitGrace;
    }
}
