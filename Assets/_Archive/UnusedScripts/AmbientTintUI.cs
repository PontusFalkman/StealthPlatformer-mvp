using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Stealth/Rendering/Ambient Tint UI")]
[RequireComponent(typeof(Image))]
public class AmbientTintUI : MonoBehaviour
{
    public VisibilitySettings settings;   // same asset used by PlayerVisibilityMeter
    [Range(0f, 2f)] public float alphaScale = 1.0f; // visual vs gameplay decoupling
    public float lerpSpeed = 8f;          // UI smoothing

    Image img;
    float current;

    void Awake() { img = GetComponent<Image>(); }

    void LateUpdate()
    {
        float target = 0.0f;
        if (settings) target = Mathf.Clamp01(settings.ambientDarkness) * Mathf.Max(0f, alphaScale);

        current = Mathf.Lerp(current, target, 1f - Mathf.Exp(-lerpSpeed * Time.unscaledDeltaTime));

        var c = img.color;
        c.a = current;
        img.color = c;
    }
}
