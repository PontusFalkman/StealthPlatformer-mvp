// Assets/Scripts/World/WallHideTarget.cs
using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("Stealth/World/Wall Hide Target")]
public class WallHideTarget : MonoBehaviour
{
    [Tooltip("If empty, all Collider2D on this object and children are used.")]
    public Collider2D[] colliders;

    [Tooltip("Optional renderers used to compute bounds when colliders are disabled.")]
    public SpriteRenderer[] renderers;

    void Reset()
    {
        colliders = GetComponentsInChildren<Collider2D>(includeInactive: true);
        renderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
    }

    public void SetGateOpen(bool open)
    {
        if (colliders == null || colliders.Length == 0)
            colliders = GetComponentsInChildren<Collider2D>(true);
        foreach (var c in colliders) if (c) c.enabled = !open;
    }

    // Works even if colliders are disabled.
    public bool ContainsPoint(Vector2 p, float padding)
    {
        Bounds b = GetWorldBounds();
        b.Expand(padding * 2f);
        return b.Contains(new Vector3(p.x, p.y, b.center.z));
    }

    Bounds GetWorldBounds()
    {
        Bounds? acc = null;

        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (!r) continue;
            acc = acc.HasValue ? Enc(acc.Value, r.bounds) : r.bounds;
        }

        if (!acc.HasValue && colliders != null)
        {
            foreach (var c in colliders)
            {
                if (!c) continue;
                acc = acc.HasValue ? Enc(acc.Value, c.bounds) : c.bounds;
            }
        }

        return acc ?? new Bounds(transform.position, Vector3.one * 0.01f);

        static Bounds Enc(Bounds a, Bounds b) { a.Encapsulate(b.min); a.Encapsulate(b.max); return a; }
    }
}
