// Scripts/Stealth/LightZone.cs
using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Stealth/Light Zone")]
public class LightZone : MonoBehaviour
{
    public static readonly List<LightZone> All = new();

    [Header("Shape")]
    [Min(0f)] public float innerRadius = 1f;
    [Min(0.01f)] public float falloffDistance = 3f;

    [Header("Strength")]
    [Range(0f, 1f)] public float intensity = 1f;
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Gameplay")]
    [Tooltip("Gameplay depth for occlusion/light logic.")]
    public int level = 1;

    [Header("Stepped Falloff")]
    public bool useSteppedFalloff = true;
    [Min(1)] public int steps = 3;

    [Header("Occlusion")]
    [Tooltip("Physics layers to check for potential blockers.")]
    public LayerMask occluderMask = ~0;
    [Tooltip("If true, only OcclusionCollider with matching VisibilityLevelTag.level blocks; non-marked colliders always block.")]
    public bool matchLevel = true;

    [Min(16)] public int rays = 128;
    [Tooltip("Per-direction outer radius cap")]
    public float minOuter = 1f, maxOuter = 5f;

    [Header("Occlusion fit")]
    [Min(0f)] public float occlusionInset = 0.05f; // shrink inside blockers

    public enum UpdateMode { OnMove, FixedHz, Manual }
    public UpdateMode updateMode = UpdateMode.OnMove;
    [Min(1f)] public float fixedHz = 20f;

    // Cached polar outline
    float[] outerR;
    Vector2 lastOrigin;
    float nextUpdateTime;
    bool dirty = true;

    void OnEnable() { if (!All.Contains(this)) All.Add(this); EnsureCache(); }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (!Application.isPlaying) { RebuildIfNeeded(true); return; }
        switch (updateMode)
        {
            case UpdateMode.OnMove: RebuildIfNeeded(false); break;
            case UpdateMode.FixedHz:
                if (Time.time >= nextUpdateTime) { RebuildPolarCache(); nextUpdateTime = Time.time + 1f / fixedHz; }
                break;
            case UpdateMode.Manual: break;
        }
    }

    void EnsureCache()
    {
        int n = Mathf.Max(16, rays);
        if (outerR == null || outerR.Length != n) outerR = new float[n];
        dirty = true;
    }

    void RebuildIfNeeded(bool editor)
    {
        EnsureCache();
        Vector2 origin = transform.position;
        if (dirty || origin != lastOrigin || editor) RebuildPolarCache();
    }

    static bool InMask(LayerMask m, int layer) => (m.value & (1 << layer)) != 0;

    void RebuildPolarCache()
    {
        EnsureCache();
        Vector2 origin = transform.position;
        lastOrigin = origin;

        float outerNominal = Mathf.Clamp(innerRadius + Mathf.Max(0.01f, falloffDistance), minOuter, maxOuter);
        int n = outerR.Length;
        float step = Mathf.PI * 2f / n;

        for (int i = 0; i < n; i++)
        {
            float a = i * step;
            Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));
            float r = outerNominal;

            if (occluderMask.value != 0)
            {
                var hits = Physics2D.RaycastAll(origin, dir, outerNominal, occluderMask);
                float best = outerNominal;

                for (int h = 0; h < hits.Length; h++)
                {
                    var hit = hits[h];
                    var col = hit.collider;
                    if (!col || col.isTrigger) continue;

                    // If collider has OcclusionCollider, apply level rule (optional).
                    if (col.GetComponent<OcclusionCollider>())
                    {
                        if (matchLevel)
                        {
                            var tag = col.GetComponentInParent<VisibilityLevelTag>();
                            if (!tag || tag.level != level) continue;
                        }
                        best = Mathf.Min(best, hit.distance);
                        break;
                    }
                    else
                    {
                        // Unmarked collider in occluderMask always blocks (e.g., Ground).
                        best = Mathf.Min(best, hit.distance);
                        break;
                    }
                }

                r = best;
            }

            outerR[i] = Mathf.Clamp((r - occlusionInset), minOuter, maxOuter);
        }

        MedianReject(outerR, 2, 0.5f);
        SmoothClamp(outerR, 0.25f, 2);

        dirty = false;
    }

    static float StepFalloff(float t, int s)
    {
        if (t <= 0f) return 1f;
        if (t >= 1f) return 0f;
        s = Mathf.Max(1, s);
        return Mathf.Ceil((1f - t) * s) / s;
    }

    // Sampling API
    public float VisibilityAt(Vector2 worldPos)
    {
        Vector2 origin = transform.position;
        float d = Vector2.Distance(worldPos, origin);
        if (d <= innerRadius) return intensity;

        float rOuter = GetOuterRadiusAtAngle(Mathf.Atan2(worldPos.y - origin.y, worldPos.x - origin.x));
        if (d >= rOuter) return 0f;

        float t = Mathf.InverseLerp(innerRadius, rOuter, d);
        float f = useSteppedFalloff ? StepFalloff(t, steps) : Mathf.Clamp01(falloff.Evaluate(t));
        return intensity * f;
    }

    public float GetOuterRadiusAtAngle(float angleRad)
    {
        RebuildIfNeeded(!Application.isPlaying);
        int n = outerR.Length;
        float u = Mathf.Repeat(angleRad, Mathf.PI * 2f) / (Mathf.PI * 2f);
        int i = Mathf.Clamp(Mathf.RoundToInt(u * n) % n, 0, n - 1);
        return outerR[i];
    }
    public Vector2 GetOrigin() => transform.position;

    // Filters
    static void MedianReject(float[] R, int halfWindow, float tau)
    {
        int n = R.Length;
        float[] tmp = new float[n];
        float[] buf = new float[9];
        for (int i = 0; i < n; i++)
        {
            int count = 0;
            for (int k = -halfWindow; k <= halfWindow; k++) buf[count++] = R[(i + k + n) % n];
            System.Array.Sort(buf, 0, count);
            float median = buf[count / 2];
            tmp[i] = Mathf.Abs(R[i] - median) > tau ? median : R[i];
        }
        System.Array.Copy(tmp, R, n);
    }
    static void SmoothClamp(float[] R, float dMax, int iterations)
    {
        int n = R.Length;
        float[] src = R, dst = new float[n];
        for (int it = 0; it < iterations; it++)
        {
            for (int i = 0; i < n; i++)
            {
                float left = src[(i - 1 + n) % n], mid = src[i], right = src[(i + 1) % n];
                float target = (left + 2f * mid + right) * 0.25f;
                dst[i] = Mathf.MoveTowards(mid, target, dMax);
            }
            var t = src; src = dst; dst = t;
        }
        if (!ReferenceEquals(src, R)) System.Array.Copy(src, R, n);
    }

#if UNITY_EDITOR
    void OnValidate() { EnsureCache(); dirty = true; }
    void Reset()
    {
        // Sensible defaults
        int walls = LayerMask.NameToLayer("Walls");
        if (walls >= 0) occluderMask = 1 << walls;
        matchLevel = true;
        level = 1;
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0.2f, 0.25f);
        Gizmos.DrawWireSphere(transform.position, innerRadius);
        float cap = Mathf.Clamp(innerRadius + falloffDistance, minOuter, maxOuter);
        Gizmos.DrawWireSphere(transform.position, cap);
    }
#endif
}
