using UnityEngine;

[CreateAssetMenu(
    menuName = "Interactions/Dialogue",
    fileName = "NewDialogueInteraction")]
public class DialogueInteraction : Interaction
{
    [TextArea(3, 6)]
    public string dialogueText;

    public override void Execute(
        Interactable target,
        PlayerController player)
    {
        Debug.Log(dialogueText);
    }
}
