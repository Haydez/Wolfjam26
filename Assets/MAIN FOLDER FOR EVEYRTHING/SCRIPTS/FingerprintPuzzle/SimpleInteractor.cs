using UnityEngine;

public class SimpleInteractor : MonoBehaviour
{
    [Tooltip("How close the player must be to interact (world units).")]
    public float range = 2.5f;

    void Update()
    {
        if (!PuzzleInput.InteractPressed()) return;

        Component nearest = null; //flashlight or table
        float best = float.MaxValue;

        foreach (Collider c in Physics.OverlapSphere(transform.position, range))
        {
            Component target = c.GetComponentInParent<FlashlightPickup>();
            if (!target) target = c.GetComponentInParent<FingerprintTable>();
            if (!target) continue;

            float d = Vector3.Distance(transform.position, c.ClosestPoint(transform.position));
            if (d < best) { best = d; nearest = target; }
        }

        if (nearest is FlashlightPickup flashlight) flashlight.Interact();
        else if (nearest is FingerprintTable table) table.Interact();
        else Debug.Log($"[SimpleInteractor] Nothing to interact with within {range}m.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
