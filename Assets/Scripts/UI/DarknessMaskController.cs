using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("Stealth/Rendering/Darkness Mask Controller")]
[RequireComponent(typeof(Image))]
public class DarknessMaskController : MonoBehaviour
{
    [Range(0f, 1f)] public float ambientDarkness = 0.10f; // 10% ≈ 26/255
    [Min(1)] public int maxLights = 32;

    Camera cam;
    Image img;
    Material runtimeMat;
    Vector4[] lightData;

    void Awake()
    {
        img = GetComponent<Image>();
        cam = Camera.main;
        runtimeMat = Instantiate(img.material);
        img.material = runtimeMat;
        lightData = new Vector4[Mathf.Clamp(maxLights, 1, 32)];
    }

    void OnDestroy() { if (Application.isPlaying && runtimeMat) Destroy(runtimeMat); }

    void LateUpdate()
    {
        if (!cam || !runtimeMat) return;

        List<LightZone> lights = LightZone.All;
        int count = Mathf.Min(lights.Count, lightData.Length);

        for (int i = 0; i < count; i++)
        {
            var z = lights[i]; if (!z) { lightData[i] = Vector4.zero; continue; }

            // world → viewport center
            Vector3 wp = z.transform.position;
            Vector3 vp = cam.WorldToViewportPoint(wp);
            if (vp.z <= 0f) { lightData[i] = Vector4.zero; continue; }

            // use OUTER radius = inner + falloff
            float outer = Mathf.Max(0.0001f, z.innerRadius + z.falloffDistance);

            // estimate viewport radius by sampling a point at outer along local X
            Vector3 wpEdge = wp + z.transform.right * outer;
            Vector3 vpEdge = cam.WorldToViewportPoint(wpEdge);
            float vpRadius = Vector2.Distance((Vector2)vp, (Vector2)vpEdge);
            if (vpRadius <= 0f)
            {
                // conservative fallback for orthographic cams with zero baseline
                vpRadius = outer * 0.15f;
            }

            float intensity = Mathf.Clamp01(z.intensity);
            lightData[i] = new Vector4(vp.x, vp.y, vpRadius, intensity);
        }

        runtimeMat.SetFloat("_AmbientDarkness", Mathf.Clamp01(ambientDarkness));
        runtimeMat.SetInt("_LightCount", count);
        runtimeMat.SetVectorArray("_LightData", lightData);
    }
}
