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

        PlayerController playerController = playerObj.GetComponent<PlayerController>();

        // 1. Freeze player movement and clear input
        if (playerController != null)
        {
            playerController.SetControl(false);
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

            // Wait until the dialogue panel closes / dialogue stops happening
            while (dialogueUI.dialogueHappening)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning($"[NPCInteraction] No valid dialogue found on {gameObject.name}. Aborting conversation safely.");
        }

        // 2. Unfreeze player movement safely when conversation ends
        if (playerController != null)
        {
            playerController.SetControl(true);
        }

        isCurrentlyTalking = false;
    }

    void SnapToFacePlayer(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = playerPosition - transform.position;
        directionToPlayer.y = 0; // Ignore height differences

        if (directionToPlayer == Vector3.zero) return;

        if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.z))
        {
            if (directionToPlayer.x > 0)
                transform.rotation = Quaternion.Euler(0, 90, 0);
            else
                transform.rotation = Quaternion.Euler(0, 270, 0);
        }
        else
        {
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