public interface IStanceInput
{
    // True only on the frame when crouch was toggled
    bool CrouchTogglePressed { get; }

    // True while run is being held (not yet used, but ready)
    bool RunHeld { get; }
}
