// Scripts/Stealth/DarknessZone.cs
using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Stealth/Darkness Zone")]
public class DarknessZone : MonoBehaviour
{
    public static readonly List<DarknessZone> All = new();

    [Header("Shape")]
    [Min(0f)] public float innerRadius = 1f;
    [Min(0.01f)] public float falloffDistance = 3f;

    [Header("Strength")]
    [Range(0f, 1f)] public float intensity = 1f;
    public AnimationCurve falloff = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("Occlusion fit")]
    [Min(0f)] public float occlusionInset = 0.05f;

    [Header("Static control")]
    public bool lockToStartPosition = true;
    Vector2 cachedOrigin;

    [Header("Stepped Falloff")]
    public bool useSteppedFalloff = true;
    [Min(1)] public int steps = 5;

    [Header("Asymmetry & Occlusion")]
    public bool useOcclusion = true;
    public LayerMask occluderMask = ~0;
    [Min(16)] public int rays = 128;
    public float minOuter = 1f, maxOuter = 5f;

    public LightZone.UpdateMode updateMode = LightZone.UpdateMode.OnMove;
    [Min(1f)] public float fixedHz = 20f;

    [Header("Light interaction")]
    public bool lightCancelsDarkness = true;                 // lights punch holes in this patch
    [Range(0f, 1f)] public float lightCancelFactor = 1f;     // 1 = full cancel

    float[] outerR;
    Vector2 lastOrigin;
    float nextUpdateTime;
    bool dirty = true;

    void OnEnable() { cachedOrigin = transform.position; if (!All.Contains(this)) All.Add(this); EnsureCache(); }
    void OnDisable() { All.Remove(this); }

    void Update()
    {
        if (!Application.isPlaying) { RebuildIfNeeded(true); return; }
        switch (updateMode)
        {
            case LightZone.UpdateMode.OnMove: RebuildIfNeeded(false); break;
            case LightZone.UpdateMode.FixedHz:
                if (Time.time >= nextUpdateTime) { RebuildPolarCache(); nextUpdateTime = Time.time + 1f / fixedHz; }
                break;
            case LightZone.UpdateMode.Manual: break;
        }
    }

    Vector2 Origin => lockToStartPosition ? cachedOrigin : (Vector2)transform.position;

    void EnsureCache()
    {
        int n = Mathf.Max(16, rays);
        if (outerR == null || outerR.Length != n) outerR = new float[n];
        dirty = true;
    }

    void RebuildIfNeeded(bool editor)
    {
        EnsureCache();
        var origin = Origin;
        if (dirty || origin != lastOrigin || editor) RebuildPolarCache();
    }

    void RebuildPolarCache()
    {
        EnsureCache();
        var origin = Origin;
        lastOrigin = origin;

        float outerNominal = Mathf.Clamp(innerRadius + Mathf.Max(0.01f, falloffDistance), minOuter, maxOuter);
        int n = outerR.Length;
        float step = Mathf.PI * 2f / n;

        for (int i = 0; i < n; i++)
        {
            float a = i * step;
            Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));
            float r = outerNominal;
            if (useOcclusion && occluderMask.value != 0)
            {
                var hit = Physics2D.Raycast(origin, dir, outerNominal, occluderMask);
                if (hit.collider && !hit.collider.isTrigger) r = Mathf.Max(0f, hit.distance);
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

    // Saturating sum of all light contributions at a point
    static float TotalLightAt(Vector2 worldPos)
    {
        float vis = 0f;
        var lights = LightZone.All;
        for (int i = 0; i < lights.Count; i++)
        {
            var z = lights[i]; if (!z) continue;
            float c = Mathf.Clamp01(z.VisibilityAt(worldPos));
            vis = 1f - (1f - vis) * (1f - c);
            if (vis >= 0.999f) break;
        }
        return vis;
    }

    // Darkness API (now reduced by light)
    public float DarknessAt(Vector2 worldPos)
    {
        Vector2 origin = Origin;
        float d = Vector2.Distance(worldPos, origin);

        // inside core
        if (d <= innerRadius)
        {
            float baseDark = intensity;
            if (!lightCancelsDarkness) return baseDark;
            float l = TotalLightAt(worldPos);
            return baseDark * Mathf.Clamp01(1f - lightCancelFactor * l);
        }

        float rOuter = GetOuterRadiusAtAngle(Mathf.Atan2(worldPos.y - origin.y, worldPos.x - origin.x));
        if (d >= rOuter) return 0f;

        float t = Mathf.InverseLerp(innerRadius, rOuter, d);
        float f = useSteppedFalloff ? StepFalloff(t, steps) : Mathf.Clamp01(falloff.Evaluate(t));
        float baseFalloffDark = intensity * f;

        if (!lightCancelsDarkness) return baseFalloffDark;
        float lightVis = TotalLightAt(worldPos);
        return baseFalloffDark * Mathf.Clamp01(1f - lightCancelFactor * lightVis);
    }

    // Accessors for visualizers
    public float GetOuterRadiusAtAngle(float angleRad)
    {
        RebuildIfNeeded(!Application.isPlaying);
        int n = outerR.Length;
        float u = Mathf.Repeat(angleRad, Mathf.PI * 2f) / (Mathf.PI * 2f);
        int i = Mathf.Clamp(Mathf.RoundToInt(u * n) % n, 0, n - 1);
        return outerR[i];
    }
    public Vector2 GetOrigin() => Origin;

    // Filters (same as LightZone)
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
    void OnValidate() { EnsureCache(); dirty = true; if (lockToStartPosition) cachedOrigin = transform.position; }
    void OnDrawGizmosSelected()
    {
        Vector3 o = Origin;
        Gizmos.color = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        Gizmos.DrawWireSphere(o, innerRadius);
        float cap = Mathf.Clamp(innerRadius + falloffDistance, minOuter, maxOuter);
        Gizmos.DrawWireSphere(o, cap);
    }
#endif
}
