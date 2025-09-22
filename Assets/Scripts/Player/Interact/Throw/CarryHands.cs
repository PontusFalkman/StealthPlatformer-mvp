// Scripts/Interact/Throw/CarryHands.cs
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Stealth/Interact/Carry/Hands")]
public class CarryHands : MonoBehaviour
{
    public Transform hand;

    public void Attach(Transform item)
    {
        if (!item || !hand) return;
        var handScene = hand.gameObject.scene;
        if (item.gameObject.scene != handScene)
            SceneManager.MoveGameObjectToScene(item.gameObject, handScene);
        item.SetParent(hand, false);
        item.localPosition = Vector3.zero;
        item.localRotation = Quaternion.identity;
    }

    public void Detach(Transform item, Scene toScene)
    {
        if (!item) return;
        item.SetParent(null, true);
        if (item.gameObject.scene != toScene)
            SceneManager.MoveGameObjectToScene(item.gameObject, toScene);
    }
}
