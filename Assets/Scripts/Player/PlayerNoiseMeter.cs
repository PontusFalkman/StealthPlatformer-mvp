// Scripts/Player/PlayerNoiseMeter.cs
using System;
using UnityEngine;

public class PlayerNoiseMeter : MonoBehaviour
{
    [Min(0f)] public float decayPerSecond = 1.5f;
    [Min(0.01f)] public float maxStack = 2.0f;

    public float CurrentValue => Mathf.Clamp01(_value / maxStack);
    public event Action<float> OnChanged;

    float _value;

    void Update()
    {
        if (_value <= 0f) return;

        float k = Mathf.Exp(-decayPerSecond * Time.deltaTime);
        float old = _value;
        _value *= k;

        if (!Mathf.Approximately(old, _value))
            OnChanged?.Invoke(CurrentValue);
    }

    public void AddBurst(float loudness, float durationSeconds)
    {
        float add = Mathf.Max(0f, loudness) * Mathf.Clamp01(durationSeconds <= 0f ? 1f : durationSeconds);
        float old = _value;
        _value = Mathf.Min(maxStack, _value + add);
        if (!Mathf.Approximately(old, _value))
            OnChanged?.Invoke(CurrentValue);
    }
}
