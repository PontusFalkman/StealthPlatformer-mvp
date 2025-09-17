// Scripts/Stealth/Debug/ZoneSpriteVisualizer.cs
using UnityEngine;

[ExecuteAlways]
[AddComponentMenu("Stealth/Debug/Zone Sprite Visualizer")]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ZoneSpriteVisualizer : MonoBehaviour
{
    [Header("Source (assign exactly one)")]
    public LightZone lightZone;
    public DarknessZone darkZone;

    [Header("Appearance")]
    [Range(0f, 1f)] public float coreAlpha = 0.85f;
    public Color lightColor = new(1f, 1f, 0.2f, 1f);
    public Color darkColor = new(0.1f, 0.1f, 0.1f, 1f);
    public int sortingOrder = 0;

    [Header("Mesh shape")]
    [Min(16)] public int rays = 96;           // angular samples
    [Min(0f)] public float occlusionInset = 0.05f; // keep inside blockers
    [Min(0f)] public float feather = 0.12f;   // soft rim width (0 = hard edge)

    MeshFilter mf; MeshRenderer mr; Mesh mesh;

    void OnEnable() { Ensure(); Refresh(); }
    void OnValidate() { Ensure(); Refresh(); }
    void Update() { if (!Application.isPlaying) Refresh(); }

    void Ensure()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        if (!mr.sharedMaterial) mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        if (mesh == null) { mesh = new Mesh { name = "ZoneFillMesh" }; mesh.MarkDynamic(); }
        mf.sharedMesh = mesh;
        mr.sortingOrder = sortingOrder;

        // Disable any legacy SpriteRenderer on the same GameObject.
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;
    }

    public void Refresh()
    {
        if (!lightZone && !darkZone) { mr.enabled = false; return; }
        mr.enabled = true;
        BuildFilledMesh();
    }

    void BuildFilledMesh()
    {
        bool isLight = lightZone != null;

        // world origin from the zone
        Vector3 wOrigin = isLight ? (Vector3)lightZone.GetOrigin()
                                  : (Vector3)darkZone.GetOrigin();
        // convert to this object's local space
        Vector3 lOrigin = transform.InverseTransformPoint(wOrigin);

        Color baseCol = isLight ? lightColor : darkColor;
        int N = Mathf.Max(16, rays);
        float step = Mathf.PI * 2f / N;

        int vertsOuter = N + 1;
        int vertsFeather = feather > 0f ? N : 0;
        int totalVerts = vertsOuter + vertsFeather;

        var verts = new Vector3[totalVerts];
        var cols = new Color[totalVerts];
        var tris = new int[N * 3 + (feather > 0f ? N * 6 : 0)];

        // center
        verts[0] = lOrigin;
        cols[0] = new Color(baseCol.r, baseCol.g, baseCol.b, coreAlpha);

        // ring
        for (int i = 0; i < N; i++)
        {
            float a = i * step;
            float r = isLight ? lightZone.GetOuterRadiusAtAngle(a)
                              : darkZone.GetOuterRadiusAtAngle(a);
            r = Mathf.Max(0f, r - occlusionInset);

            Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));
            Vector3 wPos = wOrigin + (Vector3)(dir * r);      // world point
            verts[1 + i] = transform.InverseTransformPoint(wPos); // local point
            cols[1 + i] = new Color(baseCol.r, baseCol.g, baseCol.b, coreAlpha);

            int t = i * 3;
            tris[t + 0] = 0;
            tris[t + 1] = 1 + i;
            tris[t + 2] = 1 + ((i + 1) % N);
        }

        // feather ring
        if (vertsFeather > 0)
        {
            int baseV = vertsOuter;
            int baseT = N * 3;
            for (int i = 0; i < N; i++)
            {
                float a = i * step;
                float r = isLight ? lightZone.GetOuterRadiusAtAngle(a)
                                  : darkZone.GetOuterRadiusAtAngle(a);
                r = Mathf.Max(0f, r - occlusionInset);

                Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));
                Vector3 wOuter = wOrigin + (Vector3)(dir * (r + feather));
                int iOuterA = baseV + i;
                int iOuterB = baseV + ((i + 1) % N);
                verts[iOuterA] = transform.InverseTransformPoint(wOuter);
                cols[iOuterA] = new Color(baseCol.r, baseCol.g, baseCol.b, 0f);

                int iInnerA = 1 + i;
                int iInnerB = 1 + ((i + 1) % N);
                int t = baseT + i * 6;
                tris[t + 0] = iInnerA; tris[t + 1] = iInnerB; tris[t + 2] = iOuterA;
                tris[t + 3] = iOuterA; tris[t + 4] = iInnerB; tris[t + 5] = iOuterB;
            }
        }

        mesh.Clear();
        mesh.SetVertices(verts);
        mesh.SetColors(cols);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();
    }

}
