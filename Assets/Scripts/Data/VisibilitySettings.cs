// Scripts/Data/VisibilitySettings.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Stealth/Visibility Settings")]
public class VisibilitySettings : ScriptableObject
{
    [Range(0f, 1f)] public float ambientDarkness = 0.2f;
    [Range(0f, 1f)] public float lightBlend = 1.0f;
    public float sampleRadius = 0.25f;
    public float smoothing = 10f;
}
