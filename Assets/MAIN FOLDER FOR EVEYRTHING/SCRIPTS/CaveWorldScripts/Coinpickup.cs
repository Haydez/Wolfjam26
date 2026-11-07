
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class CoinPickup : MonoBehaviour
{
    [SerializeField] private int value = 1;
    [SerializeField] private string coinId = "";

    private void Awake()
    {
        if (string.IsNullOrEmpty(coinId))
        {
            Vector3 p = transform.position;
            coinId = $"{SceneManager.GetActiveScene().name}_{p.x:F2}_{p.y:F2}_{p.z:F2}";
        }
    }

    private void Start()
    {
        // already picked up on a previous visit? yeet it
        if (InventoryManager.Instance.IsCoinCollected(coinId))
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        InventoryManager.Instance.CollectCoin(coinId, value);
        Destroy(gameObject);
    }
}