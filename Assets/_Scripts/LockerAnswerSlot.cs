using UnityEngine;

public class LockerAnswerSlot : MonoBehaviour
{
    [SerializeField] private string targetTag;
    public bool isMatch = false;

    private void Update()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag(targetTag))
            {
                isMatch = true;
                break;
            }
        }
    }
}
