// Scripts/Interact/Throw/Carryable.cs
using UnityEngine;
using Stealth.Interact;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
[AddComponentMenu("Stealth/Interact/Carryable")]
public class Carryable : MonoBehaviour, IInteractable, INoiseOverride
{
    [Header("Prompts")]
    public string pickPrompt = "Pick up";
    public string dropPrompt = "Drop";
    public Transform focus;

    [Header("Noise")]
    public string surfaceTag = "stone";
    public float impactNoiseMag = NoisePresets.SmallMag;
    public float impactNoiseRadius = NoisePresets.SmallRadius;

    [Header("Physics/Throw")]
    [Range(0f, 1f)] public float bounciness = 0.2f;
    [Range(0f, 1f)] public float friction = 0.6f;

    [Header("Drop Safety")]
    [Tooltip("Solids the item must not spawn intersecting with. Exclude Player and Pickups.")]
    [SerializeField] LayerMask solidMask;
    [SerializeField] float dropClearance = 0.12f;
    [SerializeField] float dropSkin = 0.01f;

    [Header("Drop Re-entry")]
    public bool safeReentry = false;   // false = no kinematic hop

    protected Rigidbody2D rb;
    protected Collider2D col;
    protected bool isCarried;

    RigidbodyType2D originalBodyType;
    float originalGravity;
    int originalLayer;
    PhysicsMaterial2D cachedMat;

    Collider2D[] carrierCols;
    Transform carrierRoot;

    Vector3 originalLocalScale;
    Vector3 originalWorldScale;
    const float MinAxisScale = 0.05f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        originalBodyType = rb.bodyType;
        originalGravity = rb.gravityScale;
        originalLayer = gameObject.layer;

        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (rb.linearDamping < 0.1f) rb.linearDamping = 0.1f;

        originalLocalScale = transform.localScale;
        originalWorldScale = transform.lossyScale;
    }

    // IInteractable
    public bool CanInteract(InteractionContext ctx) => !isCarried;
    public void Interact(InteractionContext ctx) { }
    public string GetPrompt(InteractionContext ctx) => isCarried ? dropPrompt : pickPrompt;
    public Vector2 GetFocusPoint() => focus ? (Vector2)focus.position : (Vector2)transform.position;
    public void OnFocus(InteractionContext ctx) { }
    public void OnBlur(InteractionContext ctx) { }

    // INoiseOverride
    public bool TryGetNoise(InteractionContext ctx, out NoiseEvent e)
    {
        e = new NoiseEvent(NoisePresets.SmallMag, NoisePresets.SmallRadius, surfaceTag, (Vector2)transform.position);
        return true;
    }

    // Called by CarrierController
    public virtual void OnPicked(Transform hand)
    {
        isCarried = true;

        carrierRoot = hand ? hand.root : null;
        carrierCols = carrierRoot ? carrierRoot.GetComponentsInChildren<Collider2D>(true) : null;
        if (carrierCols != null && col)
            foreach (var cc in carrierCols) if (cc) Physics2D.IgnoreCollision(col, cc, true);

        gameObject.layer = originalLayer;

        originalBodyType = rb.bodyType;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = originalGravity;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.simulated = true;

        if (col) { col.enabled = false; col.isTrigger = false; }

        if (hand)
        {
            Vector3 targetLocal = WorldToLocalScale(originalWorldScale, hand);
            targetLocal = ClampScale(targetLocal);
            transform.SetParent(hand, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = targetLocal;
        }
        else
        {
            transform.localScale = ClampScale(originalLocalScale);
        }
    }

    public virtual void OnDropped(bool asThrow, Vector2 velocity)
    {
        transform.SetParent(null, true);
        transform.localScale = ClampScale(originalLocalScale);
        isCarried = false;

        if (!safeReentry) { ImmediateReentry(velocity); return; }
        StartCoroutine(SafeMaterializeAfterDrop(asThrow, velocity));
    }

    // Fast path: skip kinematic phase. No lateral impulse added here.
    void ImmediateReentry(Vector2 v)
    {
        // restore collisions with former carrier
        if (carrierCols != null && col)
            foreach (var cc in carrierCols) if (cc) Physics2D.IgnoreCollision(col, cc, false);
        carrierCols = null; carrierRoot = null;

        gameObject.layer = originalLayer;
        if (col) { col.isTrigger = false; col.enabled = true; }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.None;
        rb.gravityScale = originalGravity;
        rb.simulated = true;

        if (cachedMat == null)
            cachedMat = new PhysicsMaterial2D { bounciness = bounciness, friction = friction };
        foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.sharedMaterial = cachedMat;

        rb.linearVelocity = v;   // drop uses small downward bias
        rb.angularVelocity = 0f;
        rb.WakeUp();
    }

    IEnumerator SafeMaterializeAfterDrop(bool wasThrown, Vector2 v)
    {
        // Phase 1: kinematic, collider off
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = originalGravity;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.simulated = true;
        if (col) { col.enabled = false; col.isTrigger = false; }
        gameObject.layer = originalLayer;
        yield return new WaitForFixedUpdate();
        Physics2D.SyncTransforms();

        // Phase 2: move out of overlaps
        Vector2 desired = rb.position;
        if (carrierRoot)
        {
            Vector2 fromCarrier = (rb.position - (Vector2)carrierRoot.position);
            if (fromCarrier.sqrMagnitude < 1e-6f) fromCarrier = Vector2.right;
            desired = (Vector2)carrierRoot.position + fromCarrier.normalized * dropClearance;
        }
        rb.position = ResolveDepenetration(desired);

        // refresh triggers
        if (col)
        {
            col.enabled = false; yield return null; col.enabled = true;
        }

        yield return new WaitForFixedUpdate();
        Physics2D.SyncTransforms();

        // Phase 3: dynamic again
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.None;

        if (cachedMat == null)
            cachedMat = new PhysicsMaterial2D { bounciness = bounciness, friction = friction };
        foreach (var c in GetComponentsInChildren<Collider2D>(true)) c.sharedMaterial = cachedMat;

        rb.linearVelocity = v;   // always use caller velocity
        rb.angularVelocity = 0f;
        rb.WakeUp();

        // restore carrier collisions
        if (carrierCols != null && col)
            foreach (var cc in carrierCols) if (cc) Physics2D.IgnoreCollision(col, cc, false);
        carrierCols = null; carrierRoot = null;

        gameObject.SetActive(true);
        if (col) col.enabled = true;
        rb.simulated = true;
    }

    Vector2 ResolveDepenetration(Vector2 start)
    {
        rb.position = start;
        if (!col) return start;

        var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = solidMask };
        var results = new Collider2D[16];

        for (int iter = 0; iter < 6; iter++)
        {
            int count = col.Overlap(filter, results);
            if (count == 0) break;

            Vector2 totalPush = Vector2.zero;
            for (int i = 0; i < count; i++)
            {
                var other = results[i]; if (!other) continue;
                ColliderDistance2D d = col.Distance(other);
                if (d.isOverlapped) totalPush += d.normal * (d.distance + dropSkin);
            }

            if (totalPush.sqrMagnitude < 1e-6f) break;
            rb.position = rb.position + totalPush;
            Physics2D.SyncTransforms();
        }
        return rb.position;
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        float impact = c.relativeVelocity.magnitude;
        if (impact <= 1f) return;

        float mag = impactNoiseMag * Mathf.InverseLerp(1f, 10f, impact);
        var pt = c.GetContact(0).point;
        NoiseBus.Raise(new NoiseEvent(mag, impactNoiseRadius, surfaceTag, pt));
    }

    // scale helpers
    static Vector3 WorldToLocalScale(Vector3 worldScale, Transform newParent)
    {
        Vector3 p = newParent ? newParent.lossyScale : Vector3.one;
        return new Vector3(
            p.x != 0f ? worldScale.x / p.x : worldScale.x,
            p.y != 0f ? worldScale.y / p.y : worldScale.y,
            p.z != 0f ? worldScale.z / p.z : worldScale.z
        );
    }

    static Vector3 ClampScale(Vector3 s)
    {
        return new Vector3(
            Mathf.Max(MinAxisScale, Mathf.Abs(s.x)) * Mathf.Sign(s.x == 0 ? 1 : s.x),
            Mathf.Max(MinAxisScale, Mathf.Abs(s.y)) * Mathf.Sign(s.y == 0 ? 1 : s.y),
            Mathf.Max(MinAxisScale, Mathf.Abs(s.z)) * Mathf.Sign(s.z == 0 ? 1 : s.z)
        );
    }
}
