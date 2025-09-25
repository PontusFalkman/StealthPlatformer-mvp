// Scripts/Interact/Throw/CarryPhysics.cs
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Stealth/Interact/Carry Physics")]
public class CarryPhysics : MonoBehaviour
{
    public struct Snapshot
    {
        public Rigidbody2D rb;
        public Transform originalParent;
        public Scene originalScene;
    }

    // Capture references only. Do not change physics state here.
    public Snapshot MakeHeld(Carryable c)
    {
        var rb = c ? c.GetComponent<Rigidbody2D>() : null;
        return new Snapshot
        {
            rb = rb,
            originalParent = c ? c.transform.parent : null,
            originalScene = c ? c.gameObject.scene : default
        };
    }

    // No impulses, no bodyType changes, no velocity writes.
    public void ReleaseHeld(Snapshot s)
    {
        if (!s.rb) return;
        // ensure transform sync before hand-off
        Physics2D.SyncTransforms();
        // leave rb state to Carryable.OnDropped
    }
}
