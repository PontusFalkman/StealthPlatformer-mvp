// Scripts/Player/Hiding/PlayerLevelSwitcher.cs
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Collider2D))]
public class PlayerLevelSwitcher : MonoBehaviour
{
    [Header("Physics Layers")]
    public string basePhysicsLayer = "Player";
    public string hidePhysicsLayer = "Player";

    [Header("Input Keys")]
    public KeyCode hideKey = KeyCode.DownArrow;
    public KeyCode returnKey = KeyCode.UpArrow;

    [Header("Render Settings (Normal)")]
    public string normalSortingLayer = "Player";
    public int normalSortingOrder = 0;

    [Header("Render Settings (Hidden → in front)")]
    public string hideSortingLayer = "Wall";
    public int hideSortingOrder = 10;

    [Header("Wall Detection")]
    public LayerMask wallMask;
    public float searchRadius = 1.0f;
    public bool autoReenableOnExit = true;
    public float reenablePadding = 0.1f;

    [Header("Debug")] public bool debug = false;

    SpriteRenderer[] srs;
    SortingGroup[] sgs;
    Collider2D playerCol;

    WallHideTarget currentWall;
    VisibilityLevelTag levelTag;
    bool isHidden;

    void Awake()
    {
        srs = GetComponentsInChildren<SpriteRenderer>(true);
        sgs = GetComponentsInChildren<SortingGroup>(true);
        playerCol = GetComponent<Collider2D>();

        levelTag = GetComponent<VisibilityLevelTag>();
        if (!levelTag) levelTag = gameObject.AddComponent<VisibilityLevelTag>();

        ApplyNormal();
    }

    void Update()
    {
        if (Input.GetKeyDown(hideKey)) TryHideInFront();
        if (Input.GetKeyDown(returnKey)) ReturnToNormal();

        if (autoReenableOnExit && isHidden && currentWall)
        {
            if (!currentWall.ContainsPoint(transform.position, reenablePadding))
            {
                currentWall.SetGateOpen(false);
                currentWall = null;
            }
        }
    }

    void OnDisable()
    {
        if (currentWall) currentWall.SetGateOpen(false);
        currentWall = null;
        isHidden = false;
    }

    void ApplyNormal()
    {
        SetPhysicsLayer(basePhysicsLayer);
        SetRender(normalSortingLayer, normalSortingOrder);
        isHidden = false;

        if (levelTag) levelTag.level = 0;
        if (debug) Debug.Log($"PlayerLevelSwitcher → normal, IsHidden={levelTag.IsHidden}");
    }

    void TryHideInFront()
    {
        var wall = FindNearestWall();
        if (!wall) { if (debug) Debug.Log("PlayerLevelSwitcher: no WallHideTarget found."); return; }

        wall.SetGateOpen(true);
        currentWall = wall;

        SetPhysicsLayer(hidePhysicsLayer);
        SetRender(hideSortingLayer, hideSortingOrder);
        isHidden = true;

        if (levelTag) levelTag.level = 1;
        if (debug) Debug.Log($"PlayerLevelSwitcher → hidden/in front, IsHidden={levelTag.IsHidden}");
    }

    void ReturnToNormal()
    {
        if (currentWall) currentWall.SetGateOpen(false);
        currentWall = null;
        ApplyNormal();
    }

    WallHideTarget FindNearestWall()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, searchRadius, wallMask);
        float best = float.PositiveInfinity;
        WallHideTarget bestWall = null;

        Vector2 p = transform.position;
        foreach (var h in hits)
        {
            if (!h) continue;
            var w = h.GetComponentInParent<WallHideTarget>();
            if (!w) continue;

            Vector2 cp = (Vector2)h.bounds.ClosestPoint(p);
            float d = (cp - p).sqrMagnitude;
            if (d < best) { best = d; bestWall = w; }
        }
        return bestWall;
    }

    void SetPhysicsLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0) return;
        SetLayerRecursive(transform, layer);
    }

    void SetRender(string layerName, int order)
    {
        foreach (var sr in srs) if (sr) { sr.sortingLayerName = layerName; sr.sortingOrder = order; }
        foreach (var sg in sgs) if (sg) { sg.sortingLayerName = layerName; sg.sortingOrder = order; }
    }

    static void SetLayerRecursive(Transform t, int layer)
    {
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++) SetLayerRecursive(t.GetChild(i), layer);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
#endif
}
