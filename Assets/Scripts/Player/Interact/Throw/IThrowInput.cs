// Scripts/Interact/Throw/IThrowInput.cs
public interface IThrowInput
{
    bool ThrowPressed { get; }      // edge
    bool DropPressed { get; }       // edge
    UnityEngine.Vector2 Aim { get; } // -1..1, world-relative or screen stick
}
