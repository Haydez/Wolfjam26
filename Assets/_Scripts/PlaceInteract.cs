using UnityEngine;

[CreateAssetMenu(
    menuName = "Interactions/Place",
    fileName = "NewPlaceInteraction")]
public class PlaceInteract : Interaction
{
    public override void Execute(
        Interactable target,
        PlayerController player)
    {
        if (player.isHolding && player.heldObj != null)
        {
            GameObject heldObject = player.heldObj;

            heldObject.transform.SetParent(target.transform);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.identity;
            heldObject.transform.localScale = Vector3.one;

            player.isHolding = false;
            player.heldObj = null;
        }
    }
}
