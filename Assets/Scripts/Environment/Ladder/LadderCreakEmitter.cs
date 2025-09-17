using UnityEngine;

[RequireComponent(typeof(PlayerClimber))]
[RequireComponent(typeof(PlayerNoiseMeter))]
[AddComponentMenu("Stealth/Traversal/Ladder Creak Emitter")]
public class LadderCreakEmitter : MonoBehaviour
{
    public NoiseProfile profile;
    [Tooltip("Ladder loudness = walkLoudness * creakFactor")]
    public float creakFactor = 0.6f;

    PlayerClimber climber;
    PlayerNoiseMeter meter;

    void Awake()
    {
        climber = GetComponent<PlayerClimber>();
        meter = GetComponent<PlayerNoiseMeter>();
    }

    void OnEnable() { if (climber) climber.OnClimbStep += EmitCreak; }
    void OnDisable() { if (climber) climber.OnClimbStep -= EmitCreak; }

    void EmitCreak()
    {
        if (!profile || !meter) return;
        float loud = Mathf.Max(0f, profile.walkLoudness * creakFactor);
        meter.AddBurst(loud, Mathf.Max(0.05f, profile.baseDuration * 0.75f));
    }
}
