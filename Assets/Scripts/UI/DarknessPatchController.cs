using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("Stealth/Rendering/Darkness Patch Controller")]
[RequireComponent(typeof(Image))]
public class DarknessPatchController : MonoBehaviour
{
    [Range(0f, 1f)] public float maxAlpha = 0.25f;
    [Min(1)] public int maxZones = 32;

    Camera cam;
    Image img;
    Material runtimeMat;
    Vector4[] darkData;

    void Awake()
    {
        img = GetComponent<Image>();
        cam = Camera.main;
        runtimeMat = Instantiate(img.material);
        img.material = runtimeMat;
        darkData = new Vector4[Mathf.Clamp(maxZones, 1, 32)];
    }

    void OnDestroy() { if (Application.isPlaying && runtimeMat) Destroy(runtimeMat); }

    void LateUpdate()
    {
        if (!cam || !runtimeMat) return;

        List<DarknessZone> zones = DarknessZone.All;
        int count = Mathf.Min(zones.Count, darkData.Length);

        for (int i = 0; i < count; i++)
        {
            var z = zones[i]; if (!z) continue;
            Vector3 wp = z.transform.position;
            Vector3 vp = cam.WorldToViewportPoint(wp);
            if (vp.z <= 0f) { darkData[i] = Vector4.zero; continue; }

            float outer = z.innerRadius + z.falloffDistance;
            Vector3 wpEdge = wp + z.transform.right * outer;
            Vector3 vpEdge = cam.WorldToViewportPoint(wpEdge);
            float vpRadius = Vector2.Distance((Vector2)vp, (Vector2)vpEdge);
            if (vpRadius <= 0f) vpRadius = outer * 0.15f;

            darkData[i] = new Vector4(vp.x, vp.y, vpRadius, z.intensity);
        }

        runtimeMat.SetFloat("_MaxAlpha", maxAlpha);
        runtimeMat.SetInt("_DarkCount", count);
        runtimeMat.SetVectorArray("_DarkData", darkData);
    }
}
