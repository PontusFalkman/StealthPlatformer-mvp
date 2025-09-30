// Scripts/Stealth/VisibilityField.cs
using UnityEngine;
using System.Collections.Generic;

public static class VisibilityField
{
    // Thread-local style sample context for zones.
    public static int SampleLevel { get; private set; } = 1;

    // ORIGINAL signature: unchanged behavior for callers that ignore levels.
    public static float SampleAt(Vector2 worldPos, float ambientDarkness = 0.2f)
    {
        float ambientLight = Mathf.Clamp01(1f - ambientDarkness);

        // Lights: start from ambientLight, then soft-add zones but only those on SampleLevel
        float lightVis = ambientLight;
        var lights = LightZone.All;
        for (int i = 0; i < lights.Count; i++)
        {
            var z = lights[i]; if (!z) continue;
            if (z.level != SampleLevel) continue; // level filter
            float c = Mathf.Clamp01(z.VisibilityAt(worldPos));
            lightVis = 1f - (1f - lightVis) * (1f - c); // saturating add
            if (lightVis >= 0.999f) break;
        }

        // Darkness: local patches only on SampleLevel
        float darkLocal = 0f;
        var dzs = DarknessZone.All;
        for (int i = 0; i < dzs.Count; i++)
        {
            var dz = dzs[i]; if (!dz) continue;
            if (dz.level != SampleLevel) continue; // level filter
            darkLocal = Mathf.Max(darkLocal, Mathf.Clamp01(dz.DarknessAt(worldPos)));
            if (darkLocal >= 0.999f) break;
        }

        const float EPS = 0.01f;
        bool hadLight = lightVis > ambientLight + EPS;
        bool hadDark = darkLocal > EPS;
        if (!hadLight && !hadDark) return ambientLight;

        return Mathf.Clamp01(lightVis * (1f - darkLocal));
    }

    // NEW: level-aware wrapper used by meters/AI.
    public static float SampleAt(Vector2 worldPos, int level, float ambientDarkness = 0.2f)
    {
        int prev = SampleLevel;
        SampleLevel = level;
        float v = SampleAt(worldPos, ambientDarkness);
        SampleLevel = prev;
        return v;
    }
}
