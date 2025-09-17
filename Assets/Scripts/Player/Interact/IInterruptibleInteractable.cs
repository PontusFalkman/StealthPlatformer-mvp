namespace Stealth.Interact
{
    /// Let the interactor cancel in-progress actions.
    public interface IInterruptibleInteractable
    {
        void Cancel(InteractionContext ctx);
    }
}
