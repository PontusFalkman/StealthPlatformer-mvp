namespace Stealth.Interact
{
    /// Optional hint for next-step labeling.
    public interface IMultiStepInteractable
    {
        string NextPrompt { get; }
    }
}
