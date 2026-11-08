using UnityEngine;

public class DestroyOnCraft : MonoBehaviour
{
    public GameObject objectToDelete1;
    public GameObject objectToDelete2;

    private void OnEnable()
    {
        if (objectToDelete1 != null)
            Destroy(objectToDelete1);

        if (objectToDelete2 != null)
            Destroy(objectToDelete2);
    }
}
