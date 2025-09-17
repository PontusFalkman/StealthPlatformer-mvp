using UnityEngine;

[AddComponentMenu("Stealth/Dev/Light PingPong")]
public class LightPingPong : MonoBehaviour
{
    public Vector2 axis = Vector2.right; // movement direction
    public float distance = 5f;          // total travel
    public float periodSeconds = 3f;     // full back-and-forth
    public bool circular = false;        // circle instead of line

    Vector3 startPos;

    void Awake() { startPos = transform.position; }

    void Update()
    {
        if (periodSeconds <= 0f) return;
        float t = (Time.time * Mathf.PI * 2f) / periodSeconds;

        if (circular)
        {
            float r = distance * 0.5f;
            transform.position = startPos + new Vector3(Mathf.Cos(t) * r, Mathf.Sin(t) * r, 0f);
        }
        else
        {
            float k = Mathf.Sin(t); // -1..1
            Vector2 dir = axis.sqrMagnitude > 1e-6f ? axis.normalized : Vector2.right;
            transform.position = startPos + (Vector3)(dir * (distance * 0.5f * k));
        }
    }
}
