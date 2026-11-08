using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    public GameObject objectToActivate;

    public int requiredCoins = 3;
    [SerializeField] private int coinCount = 0;

    private void Start()
    {
        coinCount = 0;
    }

    private void Update()
    {
        if((transform.childCount > coinCount) && transform.GetChild(coinCount).CompareTag("Coin"))
        {
            transform.GetChild(coinCount).gameObject.SetActive(false);
            coinCount++;
        }


        if (transform.childCount == 3 && !objectToActivate.activeSelf)
        {
            objectToActivate.SetActive(true);
        }
    }
}
