using UnityEngine;

public class GoldPuzzleManager : MonoBehaviour
{
    public LockerAnswerSlot obj1;

    public GameObject gold;

    private void Update()
    {
        if (obj1.isMatch)
        {
            gold.SetActive(true);
        }
        else
        {
            return;
        }
    }
}
