using UnityEngine;


public class StairTeleporter : MonoBehaviour
{
    [Tooltip("Empty GameObject marking where the player arrives. Its blue Z arrow is the facing direction.")]
    [SerializeField] Transform destination;
    [Tooltip("Also turn the player to face the destination's forward direction.")]
    [SerializeField] bool matchRotation = true;
    [SerializeField] string playerTag = "Player";

    [Tooltip("Seconds after arriving during which no stair trigger will fire (prevents ping-ponging).")]
    [SerializeField] float cooldown = 0.5f;

    static float readyTime; // shared by every StairTeleporter

    void OnTriggerEnter(Collider other)
    {
        TryUse(other.transform);
    }

    void TryUse(Transform touched)
    {
        if (Time.unscaledTime < readyTime) return;
        if (SceneDoor.AnyTransitionRunning) return;

        Transform player = FindPlayer(touched);
        if (player == null) return;

        if (destination == null)
        {
            Debug.LogWarning($"{name}: no destination assigned on StairTeleporter.", this);
            return;
        }

        if (SplitRevealTransition.Instance == null)
        {
            Debug.LogWarning("StairTeleporter: no SplitRevealTransition in the scene. Teleporting without transition.");
            Teleport(player);
            readyTime = Time.unscaledTime + cooldown;
            return;
        }

        // Block re-triggering until well after the transition finishes.
        readyTime = float.MaxValue;
        SplitRevealTransition.PlayInScene(() => Teleport(player));
        SplitRevealTransition.TransitionFinished += OnFinished;
    }

    void OnFinished()
    {
        SplitRevealTransition.TransitionFinished -= OnFinished;
        readyTime = Time.unscaledTime + cooldown;
    }

    void Teleport(Transform player)
    {
        // A CharacterController overwrites manual position changes unless it's
        // switched off for the move.
        var controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        var body = player.GetComponent<Rigidbody>();
        if (body != null && !body.isKinematic)
        {
            body.linearVelocity = Vector3.zero; 
            body.angularVelocity = Vector3.zero;
        }

        if (matchRotation) player.SetPositionAndRotation(destination.position, destination.rotation);
        else player.position = destination.position;

        if (body != null) body.position = player.position;
        Physics.SyncTransforms();

        if (controller != null) controller.enabled = true;
    }

    // Checks the touched object and its parents, so it still works if the
    // collider is on a child of the player.
    Transform FindPlayer(Transform t)
    {
        for (; t != null; t = t.parent)
        {
            if (t.CompareTag(playerTag)) return t;
        }
        return null;
    }

    // Draws a line to the destination in the Scene view when selected.
    void OnDrawGizmosSelected()
    {
        if (destination == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, destination.position);
        Gizmos.DrawWireSphere(destination.position, 0.3f);
        Gizmos.DrawRay(destination.position, destination.forward);
    }
}