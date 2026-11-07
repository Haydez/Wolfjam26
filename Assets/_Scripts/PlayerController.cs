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

    private bool canMove = true;

    // Explicitly set player movement control on or off
    public void SetControl(bool state)
    {
        canMove = state;
        if (!canMove)
        {
            move = Vector2.zero; // Reset input so player stops immediately
        }
    }

    // wasd to move
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        move = context.ReadValue<Vector2>();
    }

    // space to interact
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
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movement), 0.15f);
        }

        Vector3 velocity = movement * speed;
        velocity.y = playerVelocity.y;

        controller.Move(velocity * Time.deltaTime);
    }
}