using UnityEngine;
using UnityEngine.UI;
using Stealth.Inputs;

namespace Stealth.Interact
{
    [AddComponentMenu("Stealth/Interact/Interact Prompt Canvas UI")]
    public class InteractPromptCanvasUI : MonoBehaviour
    {
        [Header("Refs")]
        public InteractScanner scanner;        // from player
        public Interactor interactor;          // from player
        public Camera worldCamera;             // main or your gameplay cam
        public Canvas canvas;                  // your existing UI Canvas (Screen Space - Camera or Overlay)
        [Header("UI Elements")]
        public RectTransform root;             // panel to move near target
        public Text promptText;                // or hook your TMP_Text via a tiny wrapper
        public Image holdFill;                 // optional radial/linear fill (Image.type Filled)
        [Header("Layout")]
        public Vector2 screenOffset = new(0f, -24f);

        // local timer for hold progress approximation
        float holdTimer;
        IInteractable lastTarget;

        void Reset()
        {
            canvas = GetComponentInParent<Canvas>();
            if (!worldCamera && Camera.main) worldCamera = Camera.main;
        }

        void Awake()
        {
            if (!scanner) scanner = GetComponentInParent<InteractScanner>();
            if (!interactor) interactor = GetComponentInParent<Interactor>();
            if (!canvas) canvas = GetComponentInParent<Canvas>();
            if (!worldCamera) worldCamera = Camera.main;
            if (scanner) scanner.OnTargetChanged += OnTargetChanged;
            SetActive(false);
        }

        void OnDestroy()
        {
            if (scanner) scanner.OnTargetChanged -= OnTargetChanged;
        }

        void Update()
        {
            var target = scanner ? scanner.Target : null;
            if (target == null) { SetActive(false); holdTimer = 0f; lastTarget = null; return; }

            // Position UI over target focus point
            Vector2 world = target.GetFocusPoint();
            Vector2 screen = worldCamera ? (Vector2)worldCamera.WorldToScreenPoint(world) : (Vector2)Camera.main.WorldToScreenPoint(world);
            screen += screenOffset;

            Vector2 uiPos;
            if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, screen, canvas.worldCamera, out uiPos);
            }
            else
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform, screen, null, out uiPos);
            }
            if (root) root.anchoredPosition = uiPos;

            // Prompt text
            string label = SafeGetPrompt(target);
            if (promptText && promptText.text != label) promptText.text = label;

            // Hold meter approximation using input state
            if (holdFill)
            {
                float fill = 0f;
                if (target is IHoldInteractable hold && interactor && interactor.enabled)
                {
                    bool held = (interactor as MonoBehaviour) != null && (interactorEnabled() && interactHeld());
                    if (target != lastTarget) holdTimer = 0f;
                    if (held) holdTimer += Time.deltaTime;
                    else if (interactReleased()) holdTimer = 0f;

                    float need = Mathf.Max(0.01f, hold.HoldSeconds);
                    fill = Mathf.Clamp01(holdTimer / need);
                }
                else
                {
                    holdTimer = 0f;
                    fill = 0f;
                }
                holdFill.fillAmount = fill;
                holdFill.enabled = (target is IHoldInteractable);
            }

            lastTarget = target;
            SetActive(true);
        }

        void OnTargetChanged(IInteractable prev, IInteractable next)
        {
            holdTimer = 0f;
            if (holdFill) holdFill.fillAmount = 0f;
        }

        string SafeGetPrompt(IInteractable it)
        {
            try { return it.GetPrompt(default) ?? "Use"; } catch { return "Use"; }
        }

        void SetActive(bool on)
        {
            if (root && root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
        }

        // Small inline helpers to avoid tight coupling to your input class
        bool interactorEnabled() => interactor && interactor.isActiveAndEnabled;
        bool interactHeld()
        {
            var src = interactor.inputProvider as IInteractInput;
            return src != null && src.InteractHeld;
        }
        bool interactReleased()
        {
            var src = interactor.inputProvider as IInteractInput;
            return src != null && src.InteractReleased;
        }
    }
}
