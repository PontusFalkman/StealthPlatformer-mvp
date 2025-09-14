using UnityEngine;
using Project.Player.Stealth;

namespace Project.Enemies.Shared.Perception
{
    [System.Serializable]
    public struct PerceptionTuning
    {
        public float kv;          // vision gain
        public float kn;          // noise gain
        public float visRange;    // vision distance
        public float sndRef;      // sound reference distance
        public float fovDeg;      // field of view
        public float hardProx;    // instant-spot distance
        public float confirmTime; // time to confirm when alerted
        public static PerceptionTuning Normal => new PerceptionTuning
        {
            kv = 1.2f,
            kn = 1.0f,
            visRange = 8f,
            sndRef = 4f,
            fovDeg = 90f,
            hardProx = 2f,
            confirmTime = 0.6f
        };
    }

    public static class PerceptionMath
    {
        public static float VisFalloff(float dist, float visRange) =>
            Mathf.Clamp01((visRange - dist) / Mathf.Max(0.0001f, visRange));

        public static float SndFalloff(float dist, float sndRef) =>
            Mathf.Clamp01(1f / (1f + Mathf.Pow(dist / Mathf.Max(0.0001f, sndRef), 2f)));

        public static bool InFOV(Vector2 forward, Vector2 toTarget, float fovDeg) =>
            Vector2.Angle(forward, toTarget) <= fovDeg * 0.5f;

        public static float TickProb(float rate, float dt) => 1f - Mathf.Exp(-Mathf.Max(0f, rate) * dt);
    }
}
