using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class NPCInteraction : MonoBehaviour
{
    [Header("Interaction Type")]
    [Tooltip("Check if this is a character that should snap to face the player. Uncheck for environment objects.")]
    public bool isCharacter = true;

    [Header("Input Settings")]
    [Tooltip("Reference to your Input Action Asset for interaction.")]
    public InputActionReference interactActionReference;

    [Header("Dialogue Library")]
    public DialogueData firstMeetingDialogue;
    public DialogueData[] genericDialogues;

    [Header("NPC Attributes")]
    public string npcName;
    private bool hasMetPlayer = false;

    private DialogueUI dialogueUI;
    private bool isCurrentlyTalking = false;
    private bool playerInRange;

    void Start()
    {
        dialogueUI = Object.FindAnyObjectByType<DialogueUI>();
    }

    void OnEnable()
    {
        if (interactActionReference != null && interactActionReference.action != null)
        {
            interactActionReference.action.Enable();
            interactActionReference.action.performed += OnInteract;
        }
    }

    void OnDisable()
    {
        if (interactActionReference != null && interactActionReference.action != null)
        {
            interactActionReference.action.performed -= OnInteract;
        }
    }

    void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // If dialogue is currently open, route input to advance text via DialogueUI
        if (dialogueUI != null && dialogueUI.dialogueHappening)
        {
            dialogueUI.TryAdvanceDialogue();
            return;
        }

        // Otherwise, open dialogue if the player is in range
        if (playerInRange && !isCurrentlyTalking)
        {
            StartCoroutine(InteractRoutine());
        }
    }

    IEnumerator InteractRoutine()
    {
        isCurrentlyTalking = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            isCurrentlyTalking = false;
            yield break;
        }

        // If it's a character, snap to face the player in 4 cardinal directions
        if (isCharacter)
        {
            SnapToFacePlayer(playerObj.transform.position);
        }

        DialogueData chosenDialogue = GetApplicableDialogue();

        if (chosenDialogue != null)
        {
            dialogueUI.StartDialogue(chosenDialogue, this);

            while (dialogueUI.dialogueHappening)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning($"[NPCInteraction] No valid dialogue found on {gameObject.name}. Aborting conversation safely.");
        }

        isCurrentlyTalking = false;
    }

    void SnapToFacePlayer(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = playerPosition - transform.position;
        directionToPlayer.y = 0; // Ignore height differences

        if (directionToPlayer == Vector3.zero) return;

        // Determine whether the primary offset is along X or Z axis to snap 90 degrees
        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.z))
        {
            // Snap East or West
            if (directionToPlayer.x > 0)
                transform.rotation = Quaternion.Euler(0, 90, 0);
            else
                transform.rotation = Quaternion.Euler(0, 270, 0);
        }
        else
        {
            // Snap North or South
            if (directionToPlayer.z > 0)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else
                transform.rotation = Quaternion.Euler(0, 180, 0);
        }
    }

    DialogueData GetApplicableDialogue()
    {
        if (!hasMetPlayer && firstMeetingDialogue != null)
        {
            hasMetPlayer = true;
            return firstMeetingDialogue;
        }

        return GetRandomGenericDialogue();
    }

    DialogueData GetRandomGenericDialogue()
    {
        if (genericDialogues == null || genericDialogues.Length == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, genericDialogues.Length);
        return genericDialogues[randomIndex];
    }

    private void OnTriggerEnter(Collider other) { if (other.CompareTag("Player")) playerInRange = true; }
    private void OnTriggerExit(Collider other) { if (other.CompareTag("Player")) playerInRange = false; }
}