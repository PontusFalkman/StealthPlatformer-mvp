// Scripts/Interact/Throw/TorchCarryable.cs
using UnityEngine;

[AddComponentMenu("Stealth/Interact/Torch Carryable")]
public class TorchCarryable : Carryable
{
    [Header("Torch")]
    public LightZone carriedLight;
    public LightZone groundLightPrefab;
    public bool startLit = true;
    bool lit;

    void Start() { SetLit(startLit); }

    public void SetLit(bool on)
    {
        lit = on;
        if (carriedLight) carriedLight.gameObject.SetActive(lit && isCarried);
    }

    public override void OnPicked(Transform hand)
    {
        base.OnPicked(hand);
        if (carriedLight) carriedLight.gameObject.SetActive(lit);
    }

    public override void OnDropped(bool asThrow, Vector2 velocity)
    {
        base.OnDropped(asThrow, velocity);
        if (carriedLight) carriedLight.gameObject.SetActive(false);
        if (lit && groundLightPrefab)
        {
            var inst = Instantiate(groundLightPrefab, transform.position, Quaternion.identity);
            Destroy(inst.gameObject, 15f);
        }
    }
}
