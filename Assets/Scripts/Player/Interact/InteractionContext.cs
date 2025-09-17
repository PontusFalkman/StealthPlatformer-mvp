using UnityEngine;

namespace Stealth.Interact
{
    /// Lightweight data passed into interactables.
    public struct InteractionContext
    {
        public Transform interactor;   // player transform
        public Vector2 position;       // world pos of interactor
        public object payload;         // optional (keys, items)

        public static InteractionContext Create(Transform interactor, object payload = null)
        {
            return new InteractionContext
            {
                interactor = interactor,
                position = interactor ? (Vector2)interactor.position : Vector2.zero,
                payload = payload
            };
        }
    }
}
