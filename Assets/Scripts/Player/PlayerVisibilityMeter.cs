using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisibilityMeter : MonoBehaviour
{
    [Header("Settings")]
    public VisibilitySettings settings;   // ambientDarkness [0..1], smoothing > 0

    public float CurrentValue => _current; // 0 hidden .. 1 visible
    public event Action<float> OnChanged;

    // Debug readouts (Inspector)
    [SerializeField] float dbgLight;
    [SerializeField] float dbgDark;
    [SerializeField] float dbgTarget;
    [SerializeField] float dbgOut;

    float _current;
    DarknessZone[] _darkZones = Array.Empty<DarknessZone>();

    void OnEnable()
    {
        _current = Mathf.Clamp01(1f - (settings ? settings.ambientDarkness : 0.2f));
        _darkZones = UnityEngine.Object.FindObjectsByType<DarknessZone>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        OnChanged?.Invoke(_current);
    }

    void Update()
    {
        if (!settings) return;

        Vector2 p = transform.position;

        // Lights: sample registry
        float lightVis = 0f;
        List<LightZone> zones = LightZone.All;
        for (int i = 0; i < zones.Count; i++)
        {
            var z = zones[i];
            if (!z) continue;
            float c = Mathf.Clamp01(z.VisibilityAt(p));
            // soft-add stacking
            lightVis = 1f - (1f - lightVis) * (1f - c);
            if (lightVis >= 0.999f) break;
        }

        // Darkness: strongest at position
        float dark = settings.ambientDarkness;
        for (int i = 0; i < _darkZones.Length; i++)
        {
            var dz = _darkZones[i];
            if (!dz) continue;
            dark = Mathf.Max(dark, Mathf.Clamp01(dz.DarknessAt(p)));
        }

        // Combine (unambiguous): visible if either light or lack of darkness
        float target = Mathf.Max(lightVis, 1f - dark);
        target = Mathf.Clamp01(target);

        // Smooth
        float lambda = 1f - Mathf.Exp(-Mathf.Max(0f, settings.smoothing) * Time.deltaTime);
        float old = _current;
        _current = Mathf.Lerp(_current, target, lambda);

        // Debug
        dbgLight = lightVis;
        dbgDark = dark;
        dbgTarget = target;
        dbgOut = _current;

        if (!Mathf.Approximately(old, _current))
            OnChanged?.Invoke(_current);
    }
}
