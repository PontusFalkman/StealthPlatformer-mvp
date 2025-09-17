using UnityEngine;
using Stealth.Interact;

[RequireComponent(typeof(PlayerNoiseMeter))]
public class ActionNoiseEmitter : MonoBehaviour
{
    public NoiseProfile profile;

    [Header("Optional stance")]
    public MonoBehaviour stanceProvider; // IStanceProvider
    IStanceProvider stance;

    PlayerNoiseMeter _meter;

    void Awake()
    {
        _meter = GetComponent<PlayerNoiseMeter>();
        stance = stanceProvider as IStanceProvider;
        if (stance == null) stance = GetComponent<IStanceProvider>();
    }

    float ApplyStance(float loud) =>
        loud * (stance != null ? Mathf.Max(0f, stance.NoiseMult) : 1f);

    public void OnJump()
    {
        if (profile == null) return;
        _meter.AddBurst(ApplyStance(profile.jumpLoudness), Mathf.Max(0.01f, profile.baseDuration));
    }

    public void OnLand(float impact)
    {
        if (profile == null) return;
        float loud = profile.landLoudness * Mathf.Clamp01(impact);
        _meter.AddBurst(ApplyStance(loud), Mathf.Max(0.01f, profile.baseDuration));
    }

    public void OnAttack()
    {
        if (profile == null) return;
        _meter.AddBurst(ApplyStance(profile.runLoudness), Mathf.Max(0.01f, profile.baseDuration));
    }

    public void OnInteract() // back-compat
    {
        if (profile == null) return;
        _meter.AddBurst(ApplyStance(profile.walkLoudness), Mathf.Max(0.01f, profile.baseDuration));
    }

    public void OnInteract(NoiseEvent e)
    {
        float loud = Mathf.Clamp01(e.magnitude);
        float dur = Mathf.Max(0.01f, profile ? profile.baseDuration : 0.2f);
        _meter.AddBurst(ApplyStance(loud), dur);
    }
}
