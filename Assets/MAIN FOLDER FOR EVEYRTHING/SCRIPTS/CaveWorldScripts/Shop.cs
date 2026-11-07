using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShopItem : MonoBehaviour
{
    [SerializeField] private ItemType itemForSale = ItemType.MineralWater;
    [SerializeField] private int price = 3;
    [SerializeField] private GameObject promptObject; // optional Press E to buy

    private bool playerInRange;

    private void Start()
    {
        if (promptObject != null) promptObject.SetActive(false);
    }

    private void Update()
    {
        if (!playerInRange || !InteractPressed()) return;

        var inv = InventoryManager.Instance;

        if (inv.HasItem(itemForSale))
        {
            Debug.Log($"You already have {itemForSale}.");
        }
        else if (inv.Coins < price)
        {
            Debug.Log($"Not enough coins. Need {price}, have {inv.Coins}.");
        }
        else if (inv.SpendCoins(price) && !inv.AddItem(itemForSale))
        {
            inv.AddCoins(price);
            Debug.Log("Inventory full.");
        }
        else
        {
            Debug.Log($"Bought {itemForSale} for ${price}.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (promptObject != null) promptObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (promptObject != null) promptObject.SetActive(false);
    }
    
    private static bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        return kb != null && kb.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}