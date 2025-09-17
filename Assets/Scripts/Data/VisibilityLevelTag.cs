// Scripts/Stealth/VisibilityLevelTag.cs
using UnityEngine;

[AddComponentMenu("Stealth/Visibility Level Tag")]
public class VisibilityLevelTag : MonoBehaviour
{
    [Tooltip("Which gameplay layer this object lives on, e.g. 0, 1, or 2.")]
    public int level = 1;
}
