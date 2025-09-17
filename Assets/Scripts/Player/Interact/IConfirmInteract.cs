namespace Stealth.Interact
{
    // Require a short confirmation hold before Interact().
    public interface IConfirmInteract
    {
        float ConfirmSeconds { get; } // e.g., 0.8f for risky actions
    }
}
