// Scripts/Player/FootstepEmitter.cs
using UnityEngine;

[RequireComponent(typeof(PlayerNoiseMeter))]
public class FootstepEmitter : MonoBehaviour
{
    public NoiseProfile profile;
    public string currentSurfaceTag = "Default";
    public float stepIntervalWalk = 0.5f;
    public float stepIntervalRun = 0.33f;

    [Header("Optional timer mode")]
    public bool useTimer = false;
    public float currentSpeed = 0f;          // set from your movement script
    public float runSpeedThreshold = 3.5f;   // speed that counts as running

    PlayerNoiseMeter _meter;
    float _timer;

    void Awake()
    {
        _meter = GetComponent<PlayerNoiseMeter>();
    }

    void Update()
    {
        if (!useTimer || profile == null) return;

        float isRun = currentSpeed >= runSpeedThreshold ? 1f : 0f;
        float interval = Mathf.Lerp(stepIntervalWalk, stepIntervalRun, isRun);
        if (interval <= 0f || currentSpeed <= 0.01f) { _timer = 0f; return; }

        _timer += Time.deltaTime;
        if (_timer >= interval)
        {
            _timer = 0f;
            EmitFootstep(isRun >= 0.5f);
        }
    }

    public void OnFootstep() // call from animation event if not using timer
    {
        bool running = currentSpeed >= runSpeedThreshold;
        EmitFootstep(running);
    }

    void EmitFootstep(bool running)
    {
        if (profile == null) return;

        float baseLoud = running ? profile.runLoudness : profile.walkLoudness;
        float surf = profile.GetSurfaceFactor(currentSurfaceTag);
        float loud = baseLoud * surf;

        // Optional shaping with curve peak
        float shaped = loud * Mathf.Clamp01(profile.footstepCurve.Evaluate(1f));

        _meter.AddBurst(shaped, Mathf.Max(0.01f, profile.baseDuration));
    }
}
