using UnityEngine;
using Project.Player.Stealth;

[RequireComponent(typeof(PlayerVisibilityMeter))]
[RequireComponent(typeof(PlayerNoiseMeter))]
public class PlayerStealthSignals : MonoBehaviour, IPlayerStealthSignals
{
    PlayerVisibilityMeter vis;
    PlayerNoiseMeter noise;

    public float Visibility => vis ? vis.CurrentValue : 0f;
    public float Noise => noise ? noise.CurrentValue : 0f;
    public Vector2 WorldPosition => transform.position;

    void Awake()
    {
        vis = GetComponent<PlayerVisibilityMeter>();
        noise = GetComponent<PlayerNoiseMeter>();
    }
}
