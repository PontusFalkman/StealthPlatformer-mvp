// Scripts/UI/VisibilityBar.cs
using UnityEngine;
using UnityEngine.UI;

public class VisibilityBar : MonoBehaviour
{
    public PlayerVisibilityMeter source;
    public Image fillImage;
    public Slider slider;

    void OnEnable()
    {
        if (source != null)
            source.OnChanged += HandleChanged;

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

        HandleChanged(source.CurrentValue);
    }

    void HandleChanged(float v)
    {
        if (fillImage != null) fillImage.fillAmount = Mathf.Clamp01(v);
        if (slider != null) slider.value = Mathf.Clamp01(v);
    }
}
