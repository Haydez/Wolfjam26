using UnityEngine;

[CreateAssetMenu(
    menuName = "Interactions/Button",
    fileName = "NewButtonInteraction")]
public class ButtonInteraction : Interaction
{
    public override void Execute(
        Interactable target,
        PlayerController player)
    {
        if (target.objLinkedToInteract != null)
        {
            target.objLinkedToInteract.SetActive(!target.objLinkedToInteract.activeSelf);
        }
    }
}
