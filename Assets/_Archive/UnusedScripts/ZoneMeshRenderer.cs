// Scripts/Stealth/Debug/ZoneMeshRenderer.cs
using UnityEngine;

[ExecuteAlways]
[AddComponentMenu("Stealth/Debug/Zone Mesh Renderer")]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ZoneMeshRenderer : MonoBehaviour
{
    public LightZone lightZone;           // assign one
    public DarknessZone darkZone;

    [Min(16)] public int rays = 96;
    [Min(0f)] public float occlusionInset = 0.05f;
    [Min(0f)] public float feather = 0.12f;
    [Range(0f, 1f)] public float coreAlpha = 0.85f;
    public Color lightColor = new(1f, 1f, 0.2f, 1f);
    public Color darkColor = new(0.1f, 0.1f, 0.1f, 1f);
    public int sortingOrder = 0;

    MeshFilter mf; MeshRenderer mr; Mesh mesh;

    void OnEnable()
    {
        mf = GetComponent<MeshFilter>();
        mr = GetComponent<MeshRenderer>();
        if (!mr.sharedMaterial) mr.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        if (mesh == null) { mesh = new Mesh { name = "ZoneRuntimeMesh" }; mesh.MarkDynamic(); }
        mf.sharedMesh = mesh;
        mr.sortingOrder = sortingOrder;
        Refresh();
    }

    void OnValidate() { if (isActiveAndEnabled) Refresh(); }
    void Update() { if (!Application.isPlaying) Refresh(); }

    public void Refresh()
    {
        bool isLight = lightZone != null;
        if (!isLight && !darkZone) { mr.enabled = false; return; }
        mr.enabled = true;

        Vector2 origin = isLight ? lightZone.GetOrigin() : darkZone.GetOrigin();
        Color baseCol = isLight ? lightColor : darkColor;

        int N = Mathf.Max(16, rays);
        float step = Mathf.PI * 2f / N;

        int vertsOuter = N + 1;
        int vertsFeather = feather > 0f ? N : 0;
        int totalVerts = vertsOuter + vertsFeather;

        var verts = new Vector3[totalVerts];
        var cols = new Color[totalVerts];

        verts[0] = origin;
        cols[0] = new Color(baseCol.r, baseCol.g, baseCol.b, coreAlpha);

        for (int i = 0; i < N; i++)
        {
            float a = i * step;
            float r = isLight ? lightZone.GetOuterRadiusAtAngle(a) : darkZone.GetOuterRadiusAtAngle(a);
            r = Mathf.Max(0f, r - occlusionInset);
            Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));
            verts[1 + i] = origin + dir * r;
            cols[1 + i] = new Color(baseCol.r, baseCol.g, baseCol.b, coreAlpha);
        }

        int triCount = N * 3 + (feather > 0f ? N * 6 : 0);
        var tris = new int[triCount];

        for (int i = 0; i < N; i++)
        {
            int t = i * 3;
            tris[t + 0] = 0;
            tris[t + 1] = 1 + i;
            tris[t + 2] = 1 + ((i + 1) % N);
        }

        if (vertsFeather > 0)
        {
            int baseV = vertsOuter;
            int baseT = N * 3;
            for (int i = 0; i < N; i++)
            {
                float a = i * step;
                float r = isLight ? lightZone.GetOuterRadiusAtAngle(a) : darkZone.GetOuterRadiusAtAngle(a);
                r = Mathf.Max(0f, r - occlusionInset);
                Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));
                verts[baseV + i] = origin + dir * (r + feather);
                cols[baseV + i] = new Color(baseCol.r, baseCol.g, baseCol.b, 0f);

                int iInnerA = 1 + i;
                int iInnerB = 1 + ((i + 1) % N);
                int iOuterA = baseV + i;
                int iOuterB = baseV + ((i + 1) % N);

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
