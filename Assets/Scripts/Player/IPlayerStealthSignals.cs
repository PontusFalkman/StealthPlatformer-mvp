namespace Project.Player.Stealth
{
    public interface IPlayerStealthSignals
    {
        float Visibility { get; }
        float Noise { get; }
        UnityEngine.Vector2 WorldPosition { get; }
    }
}
