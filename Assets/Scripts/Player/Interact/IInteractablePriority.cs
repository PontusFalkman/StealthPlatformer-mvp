namespace Stealth.Interact
{
    public interface IInteractablePriority
    {
        int Priority { get; } // higher wins ties
    }
}
