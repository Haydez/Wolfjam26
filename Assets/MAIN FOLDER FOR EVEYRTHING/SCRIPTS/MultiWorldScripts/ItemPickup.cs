// ItemPickup.cs
// Put this on the collectible object in each world scene.
// Setup:
//   - Choose the matching Item Type in the Inspector.
//   - Tick "Is Trigger" on the object's Collider.
//   - Tag your player "Player" and give it a Rigidbody or CharacterController.
// For a 2D game, change Collider -> Collider2D and OnTriggerEnter -> OnTriggerEnter2D(Collider2D other).
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemType itemType;

    private void Start()
    {
        if (InventoryManager.Instance.HasItem(itemType))
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (InventoryManager.Instance.AddItem(itemType))
            Destroy(gameObject);
    }
}
