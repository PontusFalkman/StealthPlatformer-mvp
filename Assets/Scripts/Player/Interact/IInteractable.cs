using UnityEngine;

namespace Stealth.Interact
{
    /// Implement on world objects to be “usable”.
    public interface IInteractable
    {
        bool CanInteract(InteractionContext ctx);
        void Interact(InteractionContext ctx);
        string GetPrompt(InteractionContext ctx);   // short label like "Open"
        Vector2 GetFocusPoint();                    // for prompt placement
    }
}
