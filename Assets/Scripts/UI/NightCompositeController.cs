// Scripts/Stealth/Rendering/NightCompositeController.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
[AddComponentMenu("Stealth/Rendering/Night Composite Controller")]
[RequireComponent(typeof(Image))]
public class NightCompositeController : MonoBehaviour
{
    [Header("Template (assign M_NightCompositeUI)")]
    public Material templateMaterial;

    [Header("Ambient night (script-driven)")]
    [Range(0f, 1f)] public float ambient = 0.30f;

    [Header("Patches (script-driven)")]
    [Range(0f, 1f)] public float maxAlpha = 0.35f;
    [Range(0.5f, 4f)] public float gamma = 1.8f;
    [Min(0f)] public float intensityGain = 1f;
    [Min(1)] public int maxZones = 32;

    Image img; Camera cam; Material mat; Vector4[] buf;

    void OnEnable()
    {
        img = GetComponent<Image>();
        cam = Application.isPlaying ? Camera.main : cam;

        if (!templateMaterial) { Debug.LogError("Assign templateMaterial (UI/NightComposite).", this); return; }
        mat = SafeInstantiate(templateMaterial);
        img.material = mat;
        img.maskable = false;
        var c = img.color; c.a = 1f; img.color = c;

        buf = new Vector4[Mathf.Clamp(maxZones, 1, 32)];
        PushStatics();
    }

    void OnDisable() { SafeDestroy(mat); }
    void OnValidate() { if (mat) PushStatics(); }

    void LateUpdate()
    {
        if (!img || !mat) return;
        if (!cam) cam = Camera.main;
        if (img.material != mat) img.material = mat; // pin

        var zones = DarknessZone.All;
        int n = Mathf.Min(zones.Count, buf.Length);

        for (int i = 0; i < n; i++)
        {
            var z = zones[i]; if (!z) { buf[i] = Vector4.zero; continue; }
            Vector3 wp = z.transform.position;
            Vector3 vp = cam ? cam.WorldToViewportPoint(wp) : new Vector3(0.5f, 0.5f, 1f);
            if (cam && vp.z <= 0f) { buf[i] = Vector4.zero; continue; }

            float outer = z.innerRadius + z.falloffDistance;
            Vector3 vpEdge = cam ? cam.WorldToViewportPoint(wp + z.transform.right * outer) : (vp + new Vector3(0.1f, 0, 0));
            float r = Vector2.Distance((Vector2)vp, (Vector2)vpEdge);
            if (r <= 0f) r = outer * 0.15f;

            float inten = Mathf.Clamp01(z.intensity * intensityGain);
            buf[i] = new Vector4(vp.x, vp.y, r, inten);
        }

        mat.SetInt("_DarkCount", n);
        mat.SetVectorArray("_DarkData", buf);
        PushStatics();
    }

    void PushStatics()
    {
        mat.SetFloat("_Ambient", Mathf.Clamp01(ambient));
        mat.SetFloat("_MaxAlpha", Mathf.Clamp01(maxAlpha));
        mat.SetFloat("_Gamma", Mathf.Clamp(gamma, 0.5f, 4f));
    }

    static Material SafeInstantiate(Material src)
    {
#if UNITY_EDITOR
        return Application.isPlaying ? Object.Instantiate(src) : Object.Instantiate(src);
#else
        return Object.Instantiate(src);
#endif
    }
    static void SafeDestroy(Object o)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) Object.DestroyImmediate(o);
        else Object.Destroy(o);
#else
        Object.Destroy(o);
#endif
    }
}
