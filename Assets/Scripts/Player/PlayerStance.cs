using UnityEngine;

[AddComponentMenu("Stealth/Player/Player Stance")]
public class PlayerStance : MonoBehaviour, IStanceProvider
{
    [Header("Input")]
    public MonoBehaviour stanceInputProvider; // IStanceInput
    IStanceInput input;

    [Header("Defaults")]
    public bool startCrouched = false;

    [Header("Crouch Multipliers")]
    [Min(0f)] public float crouchSpeedMult = 0.55f;
    [Min(0f)] public float crouchNoiseMult = 0.40f;
    [Min(0f)] public float crouchVisibilityMult = 0.60f;

    [Header("Run Multipliers")]
    public bool useRunInput = true;
    [Min(0f)] public float runSpeedMult = 1.5f;   // 6 -> 9
    [Min(0f)] public float runNoiseMult = 1.30f;
    [Min(0f)] public float runVisibilityMult = 1.00f;

    // State
    public bool IsCrouched { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsProne => false; // not used yet

    public StanceKind Current
        => IsCrouched ? StanceKind.Crouch
        : IsRunning ? StanceKind.Run
        : StanceKind.Stand;

    void Awake()
    {
        input = stanceInputProvider as IStanceInput;
        if (input == null) input = GetComponent<IStanceInput>();
        IsCrouched = startCrouched;
        IsRunning = false;
    }

    void Update()
    {
        if (input == null) return;

        // Toggle crouch
        if (input.CrouchTogglePressed) IsCrouched = !IsCrouched;

        // Run while held (disabled if crouched)
        IsRunning = useRunInput && input.RunHeld && !IsCrouched;
    }

    // Multipliers
    public float SpeedMult => IsCrouched ? crouchSpeedMult : (IsRunning ? runSpeedMult : 1f);
    public float NoiseMult => IsCrouched ? crouchNoiseMult : (IsRunning ? runNoiseMult : 1f);
    public float VisibilityMult => IsCrouched ? crouchVisibilityMult : (IsRunning ? runVisibilityMult : 1f);
}
