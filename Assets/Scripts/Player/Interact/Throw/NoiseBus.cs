// Scripts/Perception/NoiseBus.cs
using System;
using Stealth.Interact;

public static class NoiseBus
{
    public static event Action<NoiseEvent> OnNoise;
    public static void Raise(NoiseEvent e) { try { OnNoise?.Invoke(e); } catch { } }
}
