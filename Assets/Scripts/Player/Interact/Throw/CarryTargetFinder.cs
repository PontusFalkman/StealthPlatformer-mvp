// Scripts/Interact/Throw/CarryTargetFinder.cs
using UnityEngine;

[AddComponentMenu("Stealth/Interact/Carry/Target Finder")]
public class CarryTargetFinder : MonoBehaviour
{
    public Transform hand;
    public LayerMask carryMask;
    public float pickupRadius = 1f;

    public bool TryFind(out Carryable carry)
    {
        Vector2 p = hand ? (Vector2)hand.position : (Vector2)transform.position;
        var hit = Physics2D.OverlapCircle(p, pickupRadius, carryMask);
        carry = hit ? hit.GetComponentInParent<Carryable>() : null;
        return carry;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 p = hand ? (Vector2)hand.position : (Vector2)transform.position;
        Gizmos.DrawWireSphere(p, pickupRadius);
    }
#endif
}
