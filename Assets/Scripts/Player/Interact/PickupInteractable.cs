using UnityEngine;
using Stealth.Interact;

[AddComponentMenu("Stealth/Examples/Pickup Interactable")]
public class PickupInteractable : MonoBehaviour, IInteractable, INoiseOverride
{
    public string itemId = "item";
    public Transform focus;
    public string pickupPrompt = "Pick up";
    public string surfaceTag = "default";

    public bool CanInteract(InteractionContext ctx) => true;

    public void Interact(InteractionContext ctx)
    {
        // TODO: give item to inventory via ctx.payload if you add one
        gameObject.SetActive(false);
    }

    public string GetPrompt(InteractionContext ctx) => pickupPrompt;

    public Vector2 GetFocusPoint() => (focus ? (Vector2)focus.position : (Vector2)transform.position);

    public bool TryGetNoise(InteractionContext ctx, out NoiseEvent e)
    {
        e = new NoiseEvent(NoisePresets.SmallMag, NoisePresets.SmallRadius, surfaceTag, (Vector2)transform.position);
        return true;
    }
}
