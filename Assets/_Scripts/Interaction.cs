using UnityEngine;

public abstract class Interaction : ScriptableObject
{
    public string interactionName = "Interact";

    public abstract void Execute(Interactable target, PlayerController player);
}
