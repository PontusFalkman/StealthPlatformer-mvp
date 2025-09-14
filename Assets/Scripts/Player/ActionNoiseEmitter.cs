// Scripts/Player/ActionNoiseEmitter.cs
using UnityEngine;

[RequireComponent(typeof(PlayerNoiseMeter))]
public class ActionNoiseEmitter : MonoBehaviour
{
    public NoiseProfile profile;

    PlayerNoiseMeter _meter;

    void Awake()
    {
        _meter = GetComponent<PlayerNoiseMeter>();
    }

    public void OnJump()
    {
        if (profile == null) return;
        _meter.AddBurst(profile.jumpLoudness, Mathf.Max(0.01f, profile.baseDuration));
    }

    public void OnLand(float impact)
    {
        if (profile == null) return;
        float loud = profile.landLoudness * Mathf.Clamp01(impact);
        _meter.AddBurst(loud, Mathf.Max(0.01f, profile.baseDuration));
    }

    public void OnAttack()
    {
        if (profile == null) return;
        // reuse run loudness as placeholder for attack; adjust later if desired
        _meter.AddBurst(profile.runLoudness, Mathf.Max(0.01f, profile.baseDuration));
    }

    public void OnInteract()
    {
        if (profile == null) return;
        _meter.AddBurst(profile.walkLoudness, Mathf.Max(0.01f, profile.baseDuration));
    }
}
