using UnityEngine;

namespace Stealth.Interact
{
    public struct NoiseEvent
    {
        public float magnitude;       // 0..1 suggested
        public float radius;          // world units
        public string surfaceTag;     // optional acoustics tag
        public Vector2 position;      // source

        public NoiseEvent(float magnitude, float radius, string surfaceTag, Vector2 position)
        {
            this.magnitude = magnitude;
            this.radius = radius;
            this.surfaceTag = surfaceTag;
            this.position = position;
        }
    }
}
