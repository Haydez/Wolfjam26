using UnityEngine;

public class GoldPuzzleManager : MonoBehaviour
{
    public LockerAnswerSlot obj1;

    public GameObject coin;
    private bool isActive = false;

    private void Update()
    {
        if (obj1.isMatch && !isActive)
        {
            coin.SetActive(true);
            isActive = true;
        }
    }
}
