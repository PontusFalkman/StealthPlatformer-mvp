// Scripts/UI/NoiseBar.cs
using UnityEngine;
using UnityEngine.UI;

public class NoiseBar : MonoBehaviour
{
    public PlayerNoiseMeter source;
    public Image fillImage;   // or null if using Slider
    public Slider slider;     // optional

    void OnEnable()
    {
        if (source != null)
            source.OnChanged += HandleChanged;

        // initialize
        HandleChanged(source != null ? source.CurrentValue : 0f);
    }

    void OnDisable()
    {
        if (source != null)
            source.OnChanged -= HandleChanged;
    }

    void Update()
    {
        if (source == null) return;
        if (fillImage == null && slider == null) return;

        // Polling fallback for cases without events
        HandleChanged(source.CurrentValue);
    }

    void HandleChanged(float v)
    {
        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(v);
        if (slider != null) slider.value = Mathf.Clamp01(v);
    }
}
