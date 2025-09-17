using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Stealth/Rendering/Player Sprite Darkener")]
public class PlayerSpriteDarkener : MonoBehaviour
{
    [Range(0f, 1f)] public float maxDim = 0.6f;   // 0.6 = up to 60% darker
    [Range(0.1f, 4f)] public float gamma = 1.5f;  // curve response
    public bool includeAmbient = false;          // off = dark zones only

    SpriteRenderer[] sprites;
    Color[] baseColors;

    void Awake()
    {
        sprites = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) baseColors[i] = sprites[i].color;
    }

    void LateUpdate()
    {
        // strongest local darkness
        float darkLocal = 0f;
        var zones = DarknessZone.All;
        Vector2 p = transform.position;
        for (int i = 0; i < zones.Count; i++)
        {
            var z = zones[i]; if (!z) continue;
            darkLocal = Mathf.Max(darkLocal, Mathf.Clamp01(z.DarknessAt(p)));
        }

        // optional ambient factor
        if (includeAmbient)
        {
            float ambientDark = 0.2f; // match your VisibilitySettings if needed
            darkLocal = Mathf.Clamp01(1f - (1f - ambientDark) * (1f - darkLocal));
        }

        // shaping and clamp
        float k = Mathf.Pow(Mathf.Clamp01(darkLocal), gamma) * Mathf.Clamp01(maxDim);

        // multiply sprite colors
        for (int i = 0; i < sprites.Length; i++)
        {
            var c0 = baseColors[i];
            float m = 1f - k; // 1 = unchanged, 0.4 = 60% darker
            sprites[i].color = new Color(c0.r * m, c0.g * m, c0.b * m, c0.a);
        }
    }

    void OnDisable()
    {
        // restore base color
        if (sprites == null) return;
        for (int i = 0; i < sprites.Length; i++)
            if (sprites[i]) sprites[i].color = baseColors[i];
    }
}
