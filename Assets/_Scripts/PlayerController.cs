using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed;
    private Vector2 move;
    public CharacterController controller;
    private float gravity = -9.8f;
    private Vector3 playerVelocity;
    public float interactDistance = 3f;
    private Interactable currentTarget;
    [SerializeField] private Transform holdPoint;
    public Transform HoldPoint => holdPoint;
    public bool isHolding;
    public GameObject heldObj;

    [Header("Animation")]
    public Animator animator; // Reference to your Animator component

    private bool canMove = true;
    private Vector2 lastMoveDirection = Vector2.down; // Remembers the last faced direction

    void Start()
    {
        // Automatically find the Animator on this object or its children if unassigned
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();

            if (animator == null)
            {
                Debug.LogWarning("[PlayerController] No Animator component found on the player or any child objects!", this);
            }
        }
    }

    // Explicitly set player movement control on or off
    public void SetControl(bool state)
    {
        canMove = state;
        if (!canMove)
        {
            move = Vector2.zero; // Reset input so player stops immediately
            if (animator != null) animator.speed = 0f; // Pause animation when frozen
        }
    }

    // wasd to move
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        move = context.ReadValue<Vector2>();
    }

    // space or e to interact
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        currentTarget = null;

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            currentTarget = hit.collider.GetComponentInParent<Interactable>();
        }

        if (currentTarget != null)
        {
            currentTarget.Interact(this);
        }
    }

    private void Update()
    {
        playerVelocity.y += gravity * Time.deltaTime;
        if (controller.isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        movePlayer();
    }

    public void movePlayer()
    {
        Vector3 movement = Vector3.zero;

        if (canMove)
        {
            movement = new Vector3(move.x, 0f, move.y);

            if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
                movement.z = 0f;
            else
                movement.x = 0f;

            if (movement != Vector3.zero)
            {
                // Unpause animator and set correct directional walking state
                if (animator != null) animator.speed = 1f;

                // Store the last active movement direction
                lastMoveDirection = new Vector2(movement.x, movement.z).normalized;
                UpdateAnimatorDirection(lastMoveDirection);
            }
            else
            {
                // Pause animator so it holds the current frame of the last played direction
                if (animator != null) animator.speed = 0f;
            }
        }
        else
        {
            if (animator != null) animator.speed = 0f;
        }

        Vector3 velocity = movement * speed;
        velocity.y = playerVelocity.y;

        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateAnimatorDirection(Vector2 direction)
    {
        if (animator == null) return;

        ResetAnimationBools();

        // Determine dominant direction and set its corresponding bool
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
                animator.SetBool("Right", true);
            else
                animator.SetBool("Left", true);
        }
        else
        {
            if (direction.y > 0)
                animator.SetBool("Up", true);
            else
                animator.SetBool("Down", true);
        }
    }

    private void ResetAnimationBools()
    {
        if (animator == null) return;

        animator.SetBool("Up", false);
        animator.SetBool("Down", false);
        animator.SetBool("Left", false);
        animator.SetBool("Right", false);
    }
}