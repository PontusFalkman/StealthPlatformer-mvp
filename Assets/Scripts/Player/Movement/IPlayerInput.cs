// Scripts/Player/IPlayerInput.cs
public interface IPlayerInput
{
    float MoveX { get; }     // -1..1
    bool JumpPressed { get; } // edge this frame
    bool JumpHeld { get; }    // held
    void Consume();           // clear one-shot edges if needed
}
