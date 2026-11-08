using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    public GameObject objectToActivate;

    public int requiredCoins = 3;

    private void Update()
    {
        int coinCount = 0;

        foreach (Transform child in transform)
        {
            if (child.CompareTag("Coin"))
            {
                coinCount++;
                child.gameObject.SetActive(false);
            }
        }

        if (coinCount >= requiredCoins)
        {
            objectToActivate.SetActive(true);
        }
    }
}
