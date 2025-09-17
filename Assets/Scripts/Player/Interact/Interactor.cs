// Scripts/Interact/Interactor.cs
using System.Collections.Generic;
using UnityEngine;

namespace Stealth.Interact
{
    [AddComponentMenu("Stealth/Interact/Interactor")]
    public class Interactor : MonoBehaviour
    {
        [Header("Refs")]
        public InteractScanner scanner;
        public MonoBehaviour inputProvider;          // implements IInteractInput
        public ActionNoiseEmitter actionNoise;       // optional

        [Header("Behavior")]
        public float perTargetCooldown = 0.25f;
        public bool cancelOnMoveAway = true;

        [Header("Default Noise")]
        [Range(0f, 1f)] public float defaultMagnitude = NoisePresets.SmallMag;
        public float defaultRadius = NoisePresets.SmallRadius;
        public string defaultSurfaceTag = "default";

        IInteractInput input;
        IInteractable current;
        float holdTimer;
        readonly Dictionary<IInteractable, float> lastUseTime = new();

        Transform tr;

        void Awake()
        {
            tr = transform;
            input = inputProvider as IInteractInput;
            if (!scanner) scanner = GetComponent<InteractScanner>();
            if (scanner) scanner.OnTargetChanged += OnTargetChanged;
        }

        void OnDestroy()
        {
            if (scanner) scanner.OnTargetChanged -= OnTargetChanged;
        }

        void Update()
        {
            if (inputProvider && input == null) input = inputProvider as IInteractInput;

            var prev = current;
            current = scanner ? scanner.Target : null;

            // cancel if target changed or lost
            if (cancelOnMoveAway && prev != null && prev != current)
            {
                TryCancel(prev, InteractionContext.Create(tr));
                holdTimer = 0f;
            }

            if (current == null) { holdTimer = 0f; return; }
            if (IsOnCooldown(current)) return;

            var ctx = InteractionContext.Create(tr);

            // required hold = (hold interact) + (confirm interact)
            float needHold =
                (current is IHoldInteractable h ? Mathf.Max(0.01f, h.HoldSeconds) : 0f) +
                (current is IConfirmInteract c ? Mathf.Max(0.01f, c.ConfirmSeconds) : 0f);

            if (needHold > 0f)
            {
                if (input != null && input.InteractHeld)
                {
                    holdTimer += Time.deltaTime;
                    if (holdTimer >= needHold)
                    {
                        holdTimer = 0f;
                        TryInteract(current, ctx);   // revalidates at press time
                    }
                }
                if (input != null && input.InteractReleased)
                {
                    holdTimer = 0f;
                    TryCancel(current, ctx);
                }
            }
            else
            {
                if (input != null && input.InteractPressed) TryInteract(current, ctx);
                if (input != null && input.InteractReleased) TryCancel(current, ctx);
            }
        }

        void TryInteract(IInteractable it, InteractionContext ctx)
        {
            // moving targets: revalidate CanInteract on press
            if (!SafeCanInteract(it, ctx)) return;

            SafeInteract(it, ctx);
            EmitNoise(it, ctx);
            lastUseTime[it] = Time.time;
        }

        void EmitNoise(IInteractable it, InteractionContext ctx)
        {
            if (!actionNoise) return;

            NoiseEvent e;
            if (it is INoiseOverride ovrd && ovrd.TryGetNoise(ctx, out e))
            {
                // use interactable-provided values
            }
            else
            {
                e = new NoiseEvent(
                    defaultMagnitude,
                    defaultRadius,
                    defaultSurfaceTag,
                    (Vector2)transform.position
                );
            }

            try { actionNoise.OnInteract(e); } catch { }
        }

        void TryCancel(IInteractable it, InteractionContext ctx)
        {
            if (it is IInterruptibleInteractable intr)
            {
                try { intr.Cancel(ctx); } catch { }
            }
        }

        bool IsOnCooldown(IInteractable it)
        {
            if (!lastUseTime.TryGetValue(it, out float t)) return false;
            return Time.time - t < perTargetCooldown;
        }

        static bool SafeCanInteract(IInteractable it, InteractionContext ctx)
        {
            try { return it.CanInteract(ctx); } catch { return false; }
        }

        static void SafeInteract(IInteractable it, InteractionContext ctx)
        {
            try { it.Interact(ctx); } catch { }
        }

        void OnTargetChanged(IInteractable prev, IInteractable next) { holdTimer = 0f; }
    }
}
