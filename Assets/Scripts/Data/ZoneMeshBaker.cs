// Editor/ZoneMeshBaker.cs
using UnityEditor;
using UnityEngine;

public static class ZoneMeshBaker
{
    [MenuItem("Stealth/Bake Selected Zone To Mesh Asset")]
    public static void Bake()
    {
        var go = Selection.activeGameObject;
        if (!go) { Debug.LogWarning("Select a GameObject with LightZone or DarknessZone."); return; }

        var lz = go.GetComponent<LightZone>();
        var dz = go.GetComponent<DarknessZone>();
        if (!lz && !dz) { Debug.LogWarning("No LightZone/DarknessZone on selection."); return; }

        // Parameters
        int rays = 96;
        float inset = 0.05f;

        bool isLight = lz != null;
        Vector2 origin = isLight ? lz.GetOrigin() : dz.GetOrigin();

        int N = Mathf.Max(16, rays);
        float step = Mathf.PI * 2f / N;

        var mesh = new Mesh { name = go.name + "_ZoneMesh" };
        var verts = new Vector3[N + 1];
        var tris = new int[N * 3];

        verts[0] = origin;
        for (int i = 0; i < N; i++)
        {
            float a = i * step;
            float r = isLight ? lz.GetOuterRadiusAtAngle(a) : dz.GetOuterRadiusAtAngle(a);
            r = Mathf.Max(0f, r - inset);
            Vector2 dir = new(Mathf.Cos(a), Mathf.Sin(a));
            verts[1 + i] = origin + dir * r;

            int t = i * 3;
            tris[t + 0] = 0;
            tris[t + 1] = 1 + i;
            tris[t + 2] = 1 + ((i + 1) % N);
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateBounds();

        var path = EditorUtility.SaveFilePanelInProject("Save Zone Mesh", mesh.name, "asset", "Choose a file name");
        if (string.IsNullOrEmpty(path)) return;
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        Debug.Log("Baked mesh saved: " + path);
    }
}
