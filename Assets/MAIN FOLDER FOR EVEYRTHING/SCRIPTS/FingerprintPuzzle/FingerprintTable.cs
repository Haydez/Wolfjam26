using UnityEngine;


public class FingerprintTable : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public CameraFollow cameraFollow;
    [Tooltip("Empty object above the table, rotated to look down at the clue.")]
    public Transform inspectView;
    [Tooltip("The 'clue' plane, using a FingerprintReveal material.")]
    public Renderer clueRenderer;
    [Tooltip("Optional spot light that follows the mouse for the light circle.")]
    public Light flashlightBeam;
    [Tooltip("Player movement scripts to switch off while inspecting.")]
    public Behaviour[] disableWhileInspecting;

    [Header("Flashlight")]
    public float revealRadius = 0.08f;
    public float beamHeight = 0.5f;
    public string noFlashlightMessage = "It's too dark to make anything out.";

    bool inspecting;
    Material clueMat;
    CursorLockMode prevLock;
    bool prevVisible;

    static readonly int RevealPosID = Shader.PropertyToID("_RevealPos");
    static readonly int RevealRadiusID = Shader.PropertyToID("_RevealRadius");
    static readonly Vector4 Hidden = new Vector4(0f, -9999f, 0f, 0f);

    void Start()
    {
        if (!cameraFollow) cameraFollow = FindFirstObjectByType<CameraFollow>();
        if (!playerCamera) playerCamera = Camera.main;
        clueMat = clueRenderer.material;
        clueMat.SetVector(RevealPosID, Hidden);
        if (flashlightBeam) flashlightBeam.enabled = false;
    }

    public void Interact()
    {
        if (inspecting) return;
        if (!FlashlightPickup.HasFlashlight)
        {
            Debug.Log(noFlashlightMessage);
            return;
        }
        Enter();
    }

    void Enter()
    {
        inspecting = true;
        foreach (var b in disableWhileInspecting) if (b) b.enabled = false;
        cameraFollow.focusView = inspectView;

        prevLock = Cursor.lockState;
        prevVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (flashlightBeam) flashlightBeam.enabled = true;
    }

    void Exit()
    {
        inspecting = false;
        cameraFollow.focusView = null;
        foreach (var b in disableWhileInspecting) if (b) b.enabled = true;

        Cursor.lockState = prevLock;
        Cursor.visible = prevVisible;
        clueMat.SetVector(RevealPosID, Hidden);
        if (flashlightBeam) flashlightBeam.enabled = false;
    }

    void Update()
    {
        if (!inspecting) return;
        if (PuzzleInput.ExitPressed() || PuzzleInput.MovePressed()) { Exit(); return; }

        // project the mouse onto the clue's surface
        Ray ray = playerCamera.ScreenPointToRay(PuzzleInput.MousePosition());
        Transform clue = clueRenderer.transform;
        Plane surface = new Plane(clue.up, clue.position);

        if (surface.Raycast(ray, out float dist))
        {
            Vector3 point = ray.GetPoint(dist);
            clueMat.SetVector(RevealPosID, point);
            clueMat.SetFloat(RevealRadiusID, revealRadius);

            if (flashlightBeam)
            {
                flashlightBeam.transform.position = point + surface.normal * beamHeight;
                flashlightBeam.transform.rotation = Quaternion.LookRotation(-surface.normal);
            }
        }
    }
}

public static class PuzzleInput
{
    public static Vector2 MousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        return UnityEngine.InputSystem.Mouse.current != null
            ? UnityEngine.InputSystem.Mouse.current.position.ReadValue()
            : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    public static bool ExitPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        var mouse = UnityEngine.InputSystem.Mouse.current;
        return (kb != null && kb.escapeKey.wasPressedThisFrame)
            || (mouse != null && mouse.rightButton.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1);
#endif
    }

    public static bool MovePressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        return kb != null && (kb.wKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame
            || kb.sKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame
            || kb.upArrowKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame
            || kb.leftArrowKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A)
            || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D)
            || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow)
            || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);
#endif
    }

    public static bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        return kb != null && kb.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
