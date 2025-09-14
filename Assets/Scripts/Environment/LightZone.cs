using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Stealth/Light Zone")]
public class LightZone : MonoBehaviour
{
    public static readonly List<LightZone> All = new();

    [Min(0.01f)] public float radius = 3f;
    [Min(0f)] public float intensity = 1f;
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 1, 1, 0); // 0=center -> 1=edge

    void OnEnable() { if (!All.Contains(this)) All.Add(this); }
    void OnDisable() { All.Remove(this); }

    public float VisibilityAt(Vector2 worldPos)
    {
        float d = Vector2.Distance(worldPos, (Vector2)transform.position);
        float t = Mathf.Clamp01(d / radius);         // 0 center, 1 edge
        float f = Mathf.Clamp01(falloff.Evaluate(1f - t));
        return intensity * f;
    }
}
