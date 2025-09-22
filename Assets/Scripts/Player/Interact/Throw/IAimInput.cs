// Scripts/Input/IAimInput.cs
using UnityEngine;

namespace Stealth.Inputs
{
    public interface IAimInput
    {
        Vector2 Aim { get; } // normalized aim or raw stick/mouse vector
    }
}
