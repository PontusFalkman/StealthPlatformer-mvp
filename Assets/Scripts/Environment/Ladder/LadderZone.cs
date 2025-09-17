using UnityEngine;

[AddComponentMenu("Stealth/Traversal/Ladder Zone")]
[RequireComponent(typeof(Collider2D))]
public class LadderZone : MonoBehaviour
{
    [Tooltip("World up for this ladder. Usually (0,1).")]
    public Vector2 up = Vector2.up;

    [Tooltip("Snap player X to ladder center while climbing.")]
    public bool snapXToCenter = true;

    Collider2D col;
    public Vector2 UpNorm => up.sqrMagnitude < 1e-6f ? Vector2.up : up.normalized;
    public float CenterX => col ? col.bounds.center.x : transform.position.x;

    void Reset() { var c = GetComponent<Collider2D>(); if (c) c.isTrigger = true; }
    void Awake() { col = GetComponent<Collider2D>(); if (col) col.isTrigger = true; }
}
