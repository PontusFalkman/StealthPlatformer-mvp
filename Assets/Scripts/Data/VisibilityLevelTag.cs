// Scripts/Stealth/VisibilityLevelTag.cs
using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class VisibilityLevelTag : MonoBehaviour
{
    public static readonly List<VisibilityLevelTag> All = new();

    [Tooltip("Gameplay level for this object")]
    public int level = 0;

    public bool IsHidden => level > 0;

    void OnEnable()
    {
        if (!All.Contains(this)) All.Add(this);
    }

    void OnDisable()
    {
        All.Remove(this);
    }

    // convenience accessor
    public Vector2 Position => transform.position;
}
