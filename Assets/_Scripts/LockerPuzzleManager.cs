using UnityEngine;

public class LockerPuzzleManager : MonoBehaviour
{
    public LockerAnswerSlot obj1;
    public LockerAnswerSlot obj2;
    public LockerAnswerSlot obj3;

    public GameObject objToActivate;
    public GameObject beans;

    private void Update()
    {
        if (obj1.isMatch && obj2.isMatch && obj3.isMatch)
        {
            objToActivate.SetActive(false);
            beans.SetActive(true);
        }
        else
        {
            return;
        }
    }
}
