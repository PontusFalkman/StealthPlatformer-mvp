using UnityEngine;

namespace Stealth.Interact
{
    public interface IDroppable
    {
        // called by your carrier when released
        void OnDropped(bool wasThrown, Vector2 linearVelocity);
    }
}
