// Scripts/Data/NoiseProfile.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stealth/Noise Profile")]
public class NoiseProfile : ScriptableObject
{
    [Header("Footstep shaping")]
    public AnimationCurve footstepCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Base loudness per action")]
    public float walkLoudness = 0.35f;
    public float runLoudness = 0.7f;
    public float jumpLoudness = 0.5f;
    public float landLoudness = 0.8f;

    [Header("Timing")]
    public float baseDuration = 0.15f;

    [Header("Surface map")]
    public List<SurfaceMultiplier> surfaceMap = new List<SurfaceMultiplier>
    {
        new SurfaceMultiplier{ tag = "Default", factor = 1.0f }
    };

    [Serializable]
    public struct SurfaceMultiplier
    {
        public string tag;
        public float factor;
    }

    public float GetSurfaceFactor(string tag)
    {
        for (int i = 0; i < surfaceMap.Count; i++)
        {
            if (surfaceMap[i].tag == tag) return Mathf.Max(0f, surfaceMap[i].factor);
        }
        return 1f;
    }
}
