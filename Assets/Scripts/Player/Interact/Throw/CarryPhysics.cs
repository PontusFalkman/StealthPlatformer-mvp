using UnityEngine;
using UnityEngine.SceneManagement; // add this

[AddComponentMenu("Stealth/Interact/Carry/Physics Toggle")]
public class CarryPhysics : MonoBehaviour
{
    public bool makeKinematicWhileHeld = true;
    public bool disableColliderWhileHeld = true;

    public struct Snapshot
    {
        public Rigidbody2D rb;
        public RigidbodyType2D originalType;
        public Collider2D[] colliders;
        public Bounds bounds;
        public Scene originalScene;
    }

    public Snapshot MakeHeld(Carryable c)
    {
        var rb = c.GetComponent<Rigidbody2D>();
        var snap = new Snapshot
        {
            rb = rb,
            originalType = rb.bodyType,
            colliders = c.GetComponentsInChildren<Collider2D>(true),
            bounds = GetBounds(c.transform),
            originalScene = c.gameObject.scene
        };

        if (makeKinematicWhileHeld)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        if (disableColliderWhileHeld)
            foreach (var co in snap.colliders) if (co) co.enabled = false;

        return snap;
    }

    public void ReleaseHeld(in Snapshot snap)
    {
        if (!snap.rb) return;
        if (disableColliderWhileHeld)
            foreach (var co in snap.colliders) if (co) co.enabled = true;
        if (makeKinematicWhileHeld)
            snap.rb.bodyType = snap.originalType;
    }

    static Bounds GetBounds(Transform root)
    {
        var cols = root.GetComponentsInChildren<Collider2D>(true);
        bool init = false;
        Bounds b = new Bounds(root.position, Vector3.one * 0.1f);
        foreach (var c in cols)
        {
            if (!c) continue;
            if (!init) { b = c.bounds; init = true; }
            else b.Encapsulate(c.bounds);
        }
        return b;
    }
}
