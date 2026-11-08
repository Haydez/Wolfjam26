using UnityEngine;

public class DestroyOnCraft : MonoBehaviour
{
    public GameObject objectToDelete1;
    public GameObject objectToDelete2;

    private void OnEnable()
    {
        if (objectToDelete1 != null)
            objectToDelete1.SetActive(false);

        if (objectToDelete2 != null)
            objectToDelete2.SetActive(false);
    }
}
