using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Interaction interaction;
    public GameObject objLinkedToInteract;

    public Interaction Interaction => interaction;

    public void Interact(PlayerController player)
    {
        if (interaction == null)
            return;

        interaction.Execute(this, player);
    }
}
