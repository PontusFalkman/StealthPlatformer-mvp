namespace Stealth.Interact
{
    /// Implement on an interactable to specify noise for this action.
    public interface INoiseOverride
    {
        bool TryGetNoise(InteractionContext ctx, out NoiseEvent e);
    }
}
