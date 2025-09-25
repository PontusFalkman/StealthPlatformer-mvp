using UnityEngine;
using Stealth.Interact;

[RequireComponent(typeof(LadderZone))]
[AddComponentMenu("Stealth/Examples/Ladder Attach Interactable")]
public class LadderAttachInteractable : MonoBehaviour, IInteractable, INoiseOverride
{
    public Transform focus;
    public string prompt = "Climb";
    public string surfaceTag = "wood";
    LadderZone ladder;

    void Awake() { ladder = GetComponent<LadderZone>(); }

    public bool CanInteract(InteractionContext ctx) => ladder != null;

    public void Interact(InteractionContext ctx)
    {
        if (!ladder || !ctx.interactor) return;
        var p = ctx.interactor.position;
        ctx.interactor.position = new Vector3(ladder.CenterX, p.y, p.z);
        ctx.interactor.position += Vector3.down * 0.01f; // nudge to ensure trigger enter
    }

    public string GetPrompt(InteractionContext ctx) => prompt;
    public Vector2 GetFocusPoint() => (focus ? (Vector2)focus.position : (Vector2)transform.position);

    public bool TryGetNoise(InteractionContext ctx, out NoiseEvent e)
    {
        e = new NoiseEvent(NoisePresets.SmallMag, NoisePresets.SmallRadius, surfaceTag, (Vector2)transform.position);
        return true;
    }
}
