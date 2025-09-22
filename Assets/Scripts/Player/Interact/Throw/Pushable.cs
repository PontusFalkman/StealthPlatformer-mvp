// Scripts/Interact/Push/Pushable.cs
using UnityEngine;
using Stealth.Interact;

[RequireComponent(typeof(Rigidbody2D))]
[AddComponentMenu("Stealth/Interact/Pushable")]
public class Pushable : MonoBehaviour, IInteractable, IHoldInteractable, IInterruptibleInteractable
{
    [Header("Prompt")]
    public string prompt = "Push";
    public Transform focus;

    [Header("Tuning")]
    public float holdSeconds = 0.0f;          // press or hold-to-grab
    public float moveForce = 40f;
    public float maxSpeed = 2.5f;
    public float detachMoveAway = 1.2f;       // meters
    public float playerSideOffset = 0.6f;
    public string surfaceTag = "heavy";

    [Header("Noise")]
    public float moveNoiseEvery = 0.35f;
    public float moveNoiseMag = NoisePresets.MediumMag;
    public float moveNoiseRadius = NoisePresets.MediumRadius;

    Rigidbody2D rb;
    Transform player;
    IPlayerInput playerInput; // reuse your player input
    float lastNoiseT;

    void Awake() { rb = GetComponent<Rigidbody2D>(); rb.constraints = RigidbodyConstraints2D.FreezeRotation; }

    public bool CanInteract(InteractionContext ctx) => player == null; // free to attach
    public string GetPrompt(InteractionContext ctx) => prompt;
    public Vector2 GetFocusPoint() => focus ? (Vector2)focus.position : (Vector2)transform.position;
    public float HoldSeconds => Mathf.Max(0.0f, holdSeconds);

    public void Interact(InteractionContext ctx)
    {
        if (!ctx.interactor || player != null) return;
        player = ctx.interactor;
        playerInput = player.GetComponent<IPlayerInput>();
        SnapPlayerToSide();
    }

    public void Cancel(InteractionContext ctx) { Detach(); }

    void Update()
    {
        if (!player) return;

        // auto detach if far
        if (Vector2.Distance(player.position, transform.position) > detachMoveAway) { Detach(); return; }

        // apply push from player input
        float x = playerInput != null ? Mathf.Clamp(playerInput.MoveX, -1f, 1f) : 0f;
        Vector2 v = rb.linearVelocity;

        // limit max speed
        if (Mathf.Abs(v.x) < maxSpeed || Mathf.Sign(x) != Mathf.Sign(v.x))
            rb.AddForce(new Vector2(x * moveForce, 0f), ForceMode2D.Force);

        // emit rolling noise while moving
        if (Mathf.Abs(rb.linearVelocity.x) > 0.2f && (Time.time - lastNoiseT) >= moveNoiseEvery)
        {
            lastNoiseT = Time.time;
            NoiseBus.Raise(new NoiseEvent(moveNoiseMag, moveNoiseRadius, surfaceTag, rb.position));
        }
    }

    void SnapPlayerToSide()
    {
        // place player to the nearest side once engaged
        float side = (player.position.x < transform.position.x) ? -1f : 1f;
        var p = player.position; p.x = transform.position.x + side * playerSideOffset; player.position = p;
    }

    void Detach() { player = null; playerInput = null; }
}
