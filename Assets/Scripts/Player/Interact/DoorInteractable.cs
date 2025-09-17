using UnityEngine;
using Stealth.Interact;

[AddComponentMenu("Stealth/Examples/Door Interactable")]
[RequireComponent(typeof(SpriteRenderer))]
public class DoorInteractable : MonoBehaviour, IInteractable, INoiseOverride
{
    [Header("State")]
    public bool locked = false;
    public string lockedPrompt = "Locked";

    [Header("Noise")]
    public string surfaceTag = "wood";

    [Header("Focus point")]
    public Transform focus; // optional prompt point

    [Header("Refs")]
    public Collider2D doorCollider;   // assign the physical blocking collider
    public SpriteRenderer doorSprite; // assign the visual door sprite

    bool open;

    void Reset()
    {
        doorCollider = GetComponent<Collider2D>();
        doorSprite = GetComponent<SpriteRenderer>();
    }

    public bool CanInteract(InteractionContext ctx) => !locked;

    public void Interact(InteractionContext ctx)
    {
        if (locked) return;

        open = !open;
        ApplyState();
    }

    void ApplyState()
    {
        if (doorCollider) doorCollider.enabled = !open; // toggle blocking collider
        if (doorSprite)
        {
            Color c = doorSprite.color;
            c.a = open ? 0.5f : 1f; // fade when open
            doorSprite.color = c;
        }
    }

    public string GetPrompt(InteractionContext ctx)
    {
        if (locked) return lockedPrompt;
        return open ? "Close" : "Open";
    }

    public Vector2 GetFocusPoint()
    {
        return focus ? (Vector2)focus.position : (Vector2)transform.position;
    }

    public bool TryGetNoise(InteractionContext ctx, out NoiseEvent e)
    {
        e = new NoiseEvent(
            NoisePresets.MediumMag,
            NoisePresets.MediumRadius,
            surfaceTag,
            (Vector2)transform.position
        );
        return true;
    }
}
