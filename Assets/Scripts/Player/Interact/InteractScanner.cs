using System;
using System.Collections.Generic;
using UnityEngine;

namespace Stealth.Interact
{
    [AddComponentMenu("Stealth/Interact/Interact Scanner")]
    public class InteractScanner : MonoBehaviour
    {
        [Header("Scan")]
        public float radius = 1.6f;
        public LayerMask interactableMask = ~0;
        public bool requireLOS = false;
        public LayerMask losBlockers = 0;
        [Min(0f)] public float losInset = 0.05f;

        [Header("Scoring")]
        [Range(0f, 1f)] public float facingWeight = 0.6f;
        [Range(0f, 1f)] public float distanceWeight = 0.4f;
        public float coneDegrees = 60f;
        public float coneBonus = 0.25f;
        public float switchHysteresis = 0.1f;

        [Header("Debug (read-only)")]
        public string debugTargetName;
        public float debugTargetScore;
        public Vector2 debugLosFrom;
        public Vector2 debugLosTo;
        public bool debugLosClear;

        public IInteractable Target { get; private set; }
        public event Action<IInteractable, IInteractable> OnTargetChanged;

        static readonly Collider2D[] hits = new Collider2D[16];
        readonly Dictionary<Collider2D, IInteractable> cache = new();
        readonly HashSet<IInteractable> dedupe = new();

        Transform tr;

        void Awake() { tr = transform; }

        [Obsolete]
        void Update()
        {
            float bestScore;
            var best = FindBestCandidate(out bestScore);

            // hysteresis
            if (Target != null && best != null && !ReferenceEquals(best, Target))
            {
                float curScore = Score(Target);
                if (curScore + switchHysteresis >= bestScore)
                {
                    best = Target;
                    bestScore = curScore;
                }
            }

            if (!ReferenceEquals(best, Target))
            {
                var prev = Target;
                Target = best;
                OnTargetChanged?.Invoke(prev, Target);
            }

            debugTargetName = Target is Component c ? c.gameObject.name : "<none>";
            debugTargetScore = (Target != null) ? Score(Target) : 0f;
        }

        [Obsolete]
        IInteractable FindBestCandidate(out float bestScore)
        {
            bestScore = float.NegativeInfinity;
            IInteractable best = null;

            int count = Physics2D.OverlapCircleNonAlloc(tr.position, radius, hits, interactableMask);
            if (count <= 0) return null;

            dedupe.Clear();
            for (int i = 0; i < count; i++)
            {
                var col = hits[i]; if (!col) continue;

                if (!cache.TryGetValue(col, out var it) || it == null)
                    cache[col] = it = col.GetComponentInParent<IInteractable>();
                if (it == null) continue;
                if (!dedupe.Add(it)) continue;

                var ctx = InteractionContext.Create(tr);
                if (!SafeCan(it, ctx)) continue;

                if (requireLOS && !HasLOS(tr.position, it)) continue;

                float s = Score(it);
                if (s > bestScore) { bestScore = s; best = it; }
            }
            return best;
        }

        float Score(IInteractable it)
        {
            Vector2 origin = tr.position;
            Vector2 forward = tr.right;

            Vector2 focus = it.GetFocusPoint();
            Vector2 to = focus - origin;
            float dist = Mathf.Max(0.0001f, to.magnitude);

            float dirScore = Vector2.Dot(forward.normalized, to / dist);
            float dstScore = 1f - Mathf.Clamp01(dist / Mathf.Max(0.0001f, radius));
            float score = facingWeight * dirScore + distanceWeight * dstScore;

            float ang = Vector2.Angle(forward, to);
            if (ang <= coneDegrees * 0.5f) score += coneBonus;

            if (it is IInteractablePriority pr) score += 0.001f * pr.Priority;
            return score;
        }

        bool HasLOS(Vector2 origin, IInteractable it)
        {
            Vector2 target = it.GetFocusPoint();
            Vector2 dir = target - origin;
            float dist = dir.magnitude;
            if (dist < 0.001f)
            {
                debugLosFrom = origin; debugLosTo = target; debugLosClear = true; return true;
            }

            Vector2 n = dir / dist;
            origin += n * losInset;
            target -= n * losInset;
            dist = (target - origin).magnitude;

            var hit = Physics2D.Raycast(origin, n, dist, losBlockers);
            bool clear;
            if (!hit.collider) clear = true;
            else
            {
                var itComp = it as Component;
                var itRoot = itComp ? itComp.transform.root : null;
                var hitRoot = hit.collider.attachedRigidbody ? hit.collider.attachedRigidbody.transform.root : hit.collider.transform.root;
                clear = (itRoot && hitRoot == itRoot);
            }

            debugLosFrom = origin; debugLosTo = target; debugLosClear = clear;
            return clear;
        }

        static bool SafeCan(IInteractable it, InteractionContext ctx)
        {
            try { return it.CanInteract(ctx); } catch { return false; }
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // scan radius
            Gizmos.color = new Color(0f, 1f, 1f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, radius);

            // current target highlight
            if (Target is Component t)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, t.transform.position);
                Gizmos.DrawWireSphere(t.transform.position, 0.1f);
            }

            // LOS ray
            if (Application.isPlaying)
            {
                Gizmos.color = debugLosClear ? Color.green : Color.red;
                Gizmos.DrawLine(debugLosFrom, debugLosTo);
            }
        }
#endif
    }
}
