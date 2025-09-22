// Scripts/Input/IInteractInput.cs
namespace Stealth.Inputs
{
    public interface IInteractInput
    {
        bool InteractPressed { get; }   // edge this frame
        bool InteractHeld { get; }      // held
        bool InteractReleased { get; }  // edge on release
    }
}
