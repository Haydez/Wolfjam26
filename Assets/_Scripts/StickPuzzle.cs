using UnityEngine;

public class StickPuzzle : MonoBehaviour
{
    public LockerAnswerSlot obj1;
    public LockerAnswerSlot obj2;

    public GameObject objToActivate;

    private void Update()
    {
        if (obj1.isMatch && obj2.isMatch)
        {
            objToActivate.SetActive(true);
        }
        else
        {
            return;
        }
    }
}
