using UnityEngine;


public class FlashlightPickup : MonoBehaviour
{
    public static bool HasFlashlight { get; private set; }

    void Awake() => HasFlashlight = false;

    public void Interact()
    {
        HasFlashlight = true;
        gameObject.SetActive(false);
        Debug.Log("[FlashlightPickup] Flashlight picked up.");
    }
}
