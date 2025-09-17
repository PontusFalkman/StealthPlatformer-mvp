using UnityEngine;
using Project.Player.Stealth;
using Project.Enemies.Shared.Perception;

namespace Project.Enemies.StaticGuard
{
    public class StaticGuard : MonoBehaviour
    {
        [Header("Refs")]
        public Transform player;
        public LayerMask losMask = ~0;                 // obstacles only
        public Transform facing;                       // child pointing right
        [SerializeField] SpriteRenderer facingSprite;  // optional

        [Header("Env")]
        [Range(0f, 1f)] public float ambientDarkness = 0.2f;

        [Header("Tuning")]
        public PerceptionTuning tuning = PerceptionTuning.Normal;
        public float calmRate = 1f;        // seconds of cooldown per second out of FOV
        public float forgetTime = 2f;      // time out of FOV to drop SPOTTED

        [Header("State (read-only)")]
        public bool alerted;
        public bool spotted;
        public float alertTimer;
        public float lastRate;
        public float lastProb;

        float spottedForgetTimer;
        IPlayerStealthSignals signals;

        void Awake()
        {
            if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
            signals = player ? player.GetComponent<IPlayerStealthSignals>() : null;
        }

        void Update()
        {
            if (player == null) return;

            Vector2 guardPos = transform.position;
            Vector2 playerPos = (Vector2)player.position;
            Vector2 toPlayer = playerPos - guardPos;
            float dist = toPlayer.magnitude;

            bool los = !Physics2D.Linecast(guardPos, playerPos, losMask);
            Vector2 forward = facing ? (Vector2)facing.right : (Vector2)transform.right;
            bool inFov = los && PerceptionMath.InFOV(forward, toPlayer, tuning.fovDeg);

            // Raw environment visibility at the target position
            float Vraw = VisibilityField.SampleAt(playerPos, ambientDarkness);

            // Optional noise from player signals (if available)
            float N = signals != null ? Mathf.Clamp01(signals.Noise) : 0f;

            // Only count visual visibility while in cone and with LOS
            float V = inFov ? Vraw : 0f;

            float visFall = PerceptionMath.VisFalloff(dist, tuning.visRange);
            float sndFall = PerceptionMath.SndFalloff(dist, tuning.sndRef);

            // Handle SPOTTED memory: clear after forgetTime out of FOV
            if (spotted)
            {
                bool seeing = inFov && los && V > 0f;
                if (seeing) spottedForgetTimer = forgetTime;
                else
                {
                    spottedForgetTimer -= Time.deltaTime;
                    if (spottedForgetTimer <= 0f)
                    {
                        spotted = false;
                        alerted = false;
                        alertTimer = 0f;
                        OnAlertExit();
                    }
                }
                return;
            }

            // Instant-spot rules
            bool visionInstant = los && inFov && dist <= tuning.hardProx && Vraw >= 1f;
            bool noiseInstant = los && dist <= tuning.hardProx && N >= 1f;
            if (visionInstant || noiseInstant) { Spot(); return; }

            // Probabilistic spotting only in cone
            float rate = 0f, p = 0f;
            if (inFov)
            {
                rate = tuning.kv * V * visFall + tuning.kn * N * sndFall;
                p = PerceptionMath.TickProb(rate, Time.deltaTime);

                if (Random.value < p)
                {
                    if (!alerted) { alerted = true; alertTimer = 0f; OnAlertEnter(); }
                    else { Spot(); return; }
                }
            }
            lastRate = rate; lastProb = p;

            // Alert confirm + cooldown
            if (alerted)
            {
                bool seeing = inFov && los && V > 0f;
                if (seeing)
                {
                    alertTimer += Time.deltaTime;
                    if (alertTimer >= tuning.confirmTime) { Spot(); return; }
                }
                else
                {
                    alertTimer = Mathf.Max(0f, alertTimer - calmRate * Time.deltaTime);
                    if (alertTimer <= 0f) { alerted = false; OnAlertExit(); }
                }
            }
        }

        void Spot()
        {
            spotted = true;
            spottedForgetTimer = forgetTime;
            OnSpotted();
        }

        void OnAlertEnter()
        {
            if (facingSprite) facingSprite.color = Color.yellow;
            Debug.Log($"{name}: ALERTED (player noticed, timer started)");
        }

        void OnAlertExit()
        {
            if (facingSprite) facingSprite.color = Color.white;
            Debug.Log($"{name}: CALMED DOWN");
        }

        void OnSpotted()
        {
            if (facingSprite) facingSprite.color = Color.red;
            Debug.Log($"{name}: SPOTTED PLAYER");
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, tuning.visRange);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, tuning.hardProx);

            Vector3 origin = transform.position;
            Vector2 forward = facing ? (Vector2)facing.right : (Vector2)transform.right;
            float half = tuning.fovDeg * 0.5f * Mathf.Deg2Rad;
            Vector2 left = new Vector2(Mathf.Cos(half), Mathf.Sin(half));
            Vector2 right = new Vector2(Mathf.Cos(half), -Mathf.Sin(half));
            var rot = Quaternion.FromToRotation(Vector2.right, forward);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + (Vector3)(rot * (left * tuning.visRange)));
            Gizmos.DrawLine(origin, origin + (Vector3)(rot * (right * tuning.visRange)));
        }
#endif
    }
}
