namespace Stealth.Interact
{
    // Minimal input for interactions.
    public interface IInteractInput
    {
        bool InteractPressed { get; }   // rising edge this frame
        bool InteractHeld { get; }      // while held
        bool InteractReleased { get; }  // falling edge this frame
    }
}
