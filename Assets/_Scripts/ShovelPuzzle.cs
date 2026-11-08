using UnityEngine;

public class ShovelPuzzle : MonoBehaviour
{
    public ShovelSlot obj1;
    public ShovelSlot obj2;

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
