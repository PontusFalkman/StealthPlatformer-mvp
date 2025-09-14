// Scripts/Environment/DarknessZone.cs
using UnityEngine;

public class DarknessZone : MonoBehaviour
{
    [Min(0f)] public float radius = 3f;
    public AnimationCurve cover = AnimationCurve.EaseInOut(0, 1, 1, 0); // input: 0..1 normalized distance

    public float DarknessAt(Vector2 worldPos)
    {
        float d = Vector2.Distance(worldPos, (Vector2)transform.position);
        if (radius <= 0f) return 0f;
        float t = Mathf.Clamp01(d / radius);
        float v = Mathf.Clamp01(cover.Evaluate(t));
        return v;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
