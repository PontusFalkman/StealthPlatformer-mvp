public enum StanceKind { Stand, Crouch, Run, Prone }

public interface IStanceProvider
{
    // Current stance
    StanceKind Current { get; }

    // Multipliers for the active stance (1 = no change)
    float SpeedMult { get; }
    float NoiseMult { get; }
    float VisibilityMult { get; }

    // Convenience flags
    bool IsCrouched { get; }
    bool IsRunning { get; }
    bool IsProne { get; } // optional, false if unused
}
