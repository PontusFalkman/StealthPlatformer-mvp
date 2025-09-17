// Scripts/Player/PlayerVisibilityMeter.cs
using System;
using UnityEngine;

public class PlayerVisibilityMeter : MonoBehaviour
{
    [Header("Settings")]
    public VisibilitySettings settings;

    public float CurrentValue => _current;
    public event Action<float> OnChanged;

    [SerializeField] float dbgEnv, dbgTarget, dbgOut;

    float _current;
    VisibilityLevelTag levelTag;  // NEW

    void Awake() { levelTag = GetComponentInParent<VisibilityLevelTag>(); }

    void OnEnable()
    {
        float env0 = SampleEnv(transform.position);
        _current = Mathf.Clamp01(env0);
        OnChanged?.Invoke(_current);
    }

    void Update()
    {
        if (!settings) return;
        Vector2 p = transform.position;

        float env = SampleEnv(p);
        float target = Mathf.Clamp01(env);

        float lambda = 1f - Mathf.Exp(-Mathf.Max(0f, settings.smoothing) * Time.deltaTime);
        float old = _current;
        _current = Mathf.Lerp(_current, target, lambda);

        dbgEnv = env; dbgTarget = target; dbgOut = _current;
        if (!Mathf.Approximately(old, _current)) OnChanged?.Invoke(_current);
    }

    float SampleEnv(Vector2 p)
    {
        int lvl = levelTag ? levelTag.level : 1;   // NEW
        float ambient = settings ? settings.ambientDarkness : 0.2f;
        float r = settings ? Mathf.Max(0f, settings.sampleRadius) : 0f;

        if (r <= 0.0001f) return VisibilityField.SampleAt(p, lvl, ambient);

        const int RING = 6;
        float sum = VisibilityField.SampleAt(p, lvl, ambient);
        for (int i = 0; i < RING; i++)
        {
            float ang = (Mathf.PI * 2f / RING) * i;
            Vector2 o = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * r;
            sum += VisibilityField.SampleAt(p + o, lvl, ambient);
        }
        return sum / (RING + 1);
    }
}
