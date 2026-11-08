using UnityEngine;

public class StickPuzzle : MonoBehaviour
{
    public LockerAnswerSlot obj1;
    public LockerAnswerSlot obj2;

    public GameObject objToActivate;
    private bool isActive = false;

    private void Update()
    {
        if (obj1.isMatch && obj2.isMatch && !isActive)
        {
            objToActivate.SetActive(true);
            isActive = true;
        }

    }
}
