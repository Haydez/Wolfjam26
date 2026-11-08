using UnityEngine;

[CreateAssetMenu(
    menuName = "Interactions/Active",
    fileName = "NewActiveInteraction")]

public class ActiveInteract : Interaction
{
    public override void Execute(
        Interactable target,
        PlayerController player)
    {
        target.gameObject.SetActive(false);
    }
}
