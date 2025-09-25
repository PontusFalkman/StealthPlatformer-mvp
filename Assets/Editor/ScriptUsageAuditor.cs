// Assets/Editor/ScriptUsageAuditor.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ScriptUsageAuditor : EditorWindow
{
    [MenuItem("Tools/Project Audit/Script Usage Auditor")]
    public static void Open() => GetWindow<ScriptUsageAuditor>("Script Usage Auditor");

    const string ScriptsRoot = "Assets/Scripts";
    const string ReportPath = "Assets/_Audit/ScriptUsageReport.txt";
    const string ArchivePathDefault = "Assets/_Archive/UnusedScripts";

    Vector2 scroll;
    string archivePath = ArchivePathDefault;
    string search = "";

    readonly List<string> warnings = new();
    readonly List<MonoScript> used = new();
    readonly List<MonoScript> unused = new();

    // selection state for unused
    readonly Dictionary<string, bool> sel = new();

    bool showUsed = false;
    bool showUnused = true;
    bool showWarnings = true;
    bool selectAll = false;

    void OnGUI()
    {
        EditorGUILayout.LabelField($"Scripts root: {ScriptsRoot}");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Scan")) RunScan();
        GUILayout.FlexibleSpace();
        search = EditorGUILayout.TextField(GUIContent.none, search, "ToolbarSeachTextField", GUILayout.Width(220));
        EditorGUILayout.EndHorizontal();

        if (used.Count + unused.Count == 0) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Total scripts: {used.Count + unused.Count}   Used: {used.Count}   Unused: {unused.Count}   Warnings: {warnings.Count}");

        scroll = EditorGUILayout.BeginScrollView(scroll);

        showWarnings = EditorGUILayout.Foldout(showWarnings, $"Warnings ({warnings.Count})");
        if (showWarnings)
            foreach (var w in warnings) EditorGUILayout.HelpBox(w, MessageType.Warning);

        showUnused = EditorGUILayout.Foldout(showUnused, $"Unused scripts ({unused.Count})");
        if (showUnused)
        {
            EditorGUILayout.BeginHorizontal();
            bool newSelectAll = GUILayout.Toggle(selectAll, " Select All (filtered)", GUILayout.Width(180));
            if (newSelectAll != selectAll)
            {
                selectAll = newSelectAll;
                foreach (var ms in Filter(unused)) sel[PathOf(ms)] = selectAll;
            }
            if (GUILayout.Button("Invert Selection", GUILayout.Width(140)))
                foreach (var ms in Filter(unused)) sel[PathOf(ms)] = !IsSelected(ms);
            EditorGUILayout.EndHorizontal();

            foreach (var ms in Filter(unused))
                DrawScriptRow(ms, selectable: true);
        }

        showUsed = EditorGUILayout.Foldout(showUsed, $"Used scripts ({used.Count})");
        if (showUsed)
            foreach (var ms in Filter(used))
                DrawScriptRow(ms, selectable: false);

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space();
        if (GUILayout.Button("Write Report")) WriteReport();

        archivePath = EditorGUILayout.TextField("Archive Folder", archivePath);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Archive Selected")) ArchiveSelected();
        if (GUILayout.Button("Reveal Archive Folder")) EditorUtility.RevealInFinder(archivePath);
        EditorGUILayout.EndHorizontal();
    }

    IEnumerable<MonoScript> Filter(IEnumerable<MonoScript> list)
    {
        if (string.IsNullOrWhiteSpace(search)) return list;
        var s = search.Trim().ToLowerInvariant();
        return list.Where(ms =>
        {
            var p = AssetDatabase.GetAssetPath(ms).ToLowerInvariant();
            return ms.name.ToLowerInvariant().Contains(s) || p.Contains(s);
        });
    }

    void DrawScriptRow(MonoScript ms, bool selectable)
    {
        var path = PathOf(ms);
        EditorGUILayout.BeginHorizontal();

        if (selectable)
        {
            bool cur = IsSelected(ms);
            bool now = GUILayout.Toggle(cur, GUIContent.none, GUILayout.Width(18));
            if (now != cur) sel[path] = now;
        }
        else GUILayout.Space(22);

        if (GUILayout.Button("Ping", GUILayout.Width(48)))
            EditorGUIUtility.PingObject(ms);

        EditorGUILayout.ObjectField(ms, typeof(MonoScript), false);

        if (GUILayout.Button("Open", GUILayout.Width(60)))
            AssetDatabase.OpenAsset(ms);

        EditorGUILayout.EndHorizontal();
    }

    bool IsSelected(MonoScript ms)
    {
        var p = PathOf(ms);
        return sel.TryGetValue(p, out var v) && v;
    }

    string PathOf(MonoScript ms) => AssetDatabase.GetAssetPath(ms).Replace("\\", "/");

    void RunScan()
    {
        warnings.Clear(); used.Clear(); unused.Clear(); sel.Clear();

        try
        {
            EditorUtility.DisplayProgressBar("Script Audit", "Collecting scripts…", 0f);

            // 1) MonoScripts under Assets/Scripts (exclude any Editor folder)
            var allMonoScripts = AssetDatabase.FindAssets("t:MonoScript", new[] { ScriptsRoot })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => IsUnderScriptsRoot(p) && !IsEditorPath(p))
                .Select(p => AssetDatabase.LoadAssetAtPath<MonoScript>(p))
                .Where(ms => ms != null)
                .ToList();

            var scriptTypes = new Dictionary<MonoScript, Type>();
            foreach (var ms in allMonoScripts)
            {
                var t = ms.GetClass();
                if (t == null) { warnings.Add($"No class or compile error: {AssetDatabase.GetAssetPath(ms)}"); continue; }
                if (!typeof(MonoBehaviour).IsAssignableFrom(t)) { warnings.Add($"Not MonoBehaviour (ignored): {t.FullName}"); continue; }
                if (t.IsAbstract) warnings.Add($"Abstract MonoBehaviour (never attached): {t.FullName}");
                scriptTypes[ms] = t;
            }

            // 2) Scan all prefabs and scenes under Assets for attachments of those scripts
            var usedSet = new HashSet<MonoScript>();

            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });
            for (int i = 0; i < prefabGuids.Length; i++)
            {
                if (i % 25 == 0) EditorUtility.DisplayProgressBar("Script Audit", $"Scanning prefabs {i}/{prefabGuids.Length}", i / (float)Mathf.Max(1, prefabGuids.Length));
                var path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
                if (IsEditorPath(path)) continue;
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (!go) continue;
                CollectUsedFromGO(go, usedSet);
            }

            var sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets" });
            for (int i = 0; i < sceneGuids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(sceneGuids[i]);
                if (IsEditorPath(path)) continue;
                EditorUtility.DisplayProgressBar("Script Audit", $"Scanning scene: {Path.GetFileName(path)}", i / (float)Mathf.Max(1, sceneGuids.Length));
                var opened = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                foreach (var root in opened.GetRootGameObjects())
                    CollectUsedFromGO(root, usedSet);
                EditorSceneManager.CloseScene(opened, true);
            }

            foreach (var kv in scriptTypes)
            {
                if (usedSet.Contains(kv.Key)) used.Add(kv.Key);
                else { unused.Add(kv.Key); sel[PathOf(kv.Key)] = false; }
            }

            used.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            unused.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

            Debug.Log($"Script audit complete. Used: {used.Count}, Unused: {unused.Count}, Warnings: {warnings.Count}");
        }
        finally { EditorUtility.ClearProgressBar(); }
    }

    static bool IsUnderScriptsRoot(string assetPath)
    {
        var p = assetPath.Replace('\\', '/');
        return p.StartsWith(ScriptsRoot + "/", StringComparison.OrdinalIgnoreCase) || string.Equals(p, ScriptsRoot, StringComparison.OrdinalIgnoreCase);
    }

    static bool IsEditorPath(string assetPath)
    {
        var p = assetPath.Replace('\\', '/');
        return !p.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) || p.Contains("/Editor/");
    }

    static void CollectUsedFromGO(GameObject go, HashSet<MonoScript> usedSet)
    {
        var mbs = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in mbs)
        {
            if (!mb) continue;
            var ms = MonoScript.FromMonoBehaviour(mb);
            if (!ms) continue;
            var path = AssetDatabase.GetAssetPath(ms);
            if (IsUnderScriptsRoot(path)) usedSet.Add(ms);
        }
    }

    void WriteReport()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath)!);
        using var sw = new StreamWriter(ReportPath, false);
        sw.WriteLine("== Script Usage Report ==");
        sw.WriteLine($"Generated: {DateTime.Now}");
        sw.WriteLine($"Scripts root: {ScriptsRoot}");
        sw.WriteLine();
        sw.WriteLine($"Warnings ({warnings.Count})");
        foreach (var w in warnings) sw.WriteLine(" - " + w);
        sw.WriteLine();
        sw.WriteLine($"Unused scripts ({unused.Count})");
        foreach (var ms in unused) sw.WriteLine(" - " + AssetDatabase.GetAssetPath(ms));
        sw.WriteLine();
        sw.WriteLine($"Used scripts ({used.Count})");
        foreach (var ms in used) sw.WriteLine(" - " + AssetDatabase.GetAssetPath(ms));
        AssetDatabase.ImportAsset(ReportPath);
        Debug.Log($"Wrote report: {ReportPath}");
    }

    void ArchiveSelected()
    {
        var toMove = unused.Where(IsSelected).ToList();
        if (toMove.Count == 0) { Debug.Log("Nothing selected."); return; }

        Directory.CreateDirectory(archivePath);
        foreach (var ms in toMove)
        {
            var src = AssetDatabase.GetAssetPath(ms);
            var dst = Path.Combine(archivePath, Path.GetFileName(src)).Replace("\\", "/");
            var result = AssetDatabase.MoveAsset(src, dst);
            if (!string.IsNullOrEmpty(result))
                Debug.LogWarning($"Failed to move {src}: {result}");
            else
                Debug.Log($"Archived {src}");
        }
        AssetDatabase.Refresh();
    }
}
