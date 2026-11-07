using UnityEngine;

[CreateAssetMenu(
    menuName = "Interactions/Pickup",
    fileName = "NewPickupInteraction")]
public class PickupInteract : Interaction
{
    public override void Execute(
        Interactable target,
        PlayerController player)
    {
        if (player.isHolding != true)
        {
            Transform holdPoint = player.HoldPoint;

            target.transform.SetParent(holdPoint);
            player.isHolding = true;
            player.heldObj = target.gameObject;

            target.transform.localPosition = Vector3.zero;
            target.transform.localRotation = Quaternion.identity;
            target.transform.localScale = Vector3.one;
        }
    }
}
