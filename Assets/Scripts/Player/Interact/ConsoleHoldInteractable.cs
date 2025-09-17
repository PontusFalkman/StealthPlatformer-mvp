using UnityEngine;
using Stealth.Interact;

[AddComponentMenu("Stealth/Examples/Console Hold Interactable")]
public class ConsoleHoldInteractable : MonoBehaviour, IInteractable, IHoldInteractable, IInterruptibleInteractable, INoiseOverride
{
    public float holdSeconds = 1.2f;
    public string prompt = "Hack";
    public Transform focus;
    public string surfaceTag = "metal";

    public bool CanInteract(InteractionContext ctx) => true;

    public void Interact(InteractionContext ctx)
    {
        // TODO: trigger console action
    }

    public void Cancel(InteractionContext ctx) { /* optional: revert UI */ }

    public string GetPrompt(InteractionContext ctx) => prompt;

    public Vector2 GetFocusPoint() => (focus ? (Vector2)focus.position : (Vector2)transform.position);

    public float HoldSeconds => Mathf.Max(0.01f, holdSeconds);

    public bool TryGetNoise(InteractionContext ctx, out NoiseEvent e)
    {
        e = new NoiseEvent(NoisePresets.MediumMag, NoisePresets.MediumRadius, surfaceTag, (Vector2)transform.position);
        return true;
    }
}
