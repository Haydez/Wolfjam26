using UnityEngine;

public class GetDirt : MonoBehaviour
{
    public string correctTag;
    public GameObject objectToActivate;

    private void Update()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag(correctTag))
            {
                objectToActivate.SetActive(true);
                child.gameObject.SetActive(false);
                break;
            }
        }
    }
}
