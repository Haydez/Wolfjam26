
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class BeanstalkCutscene : MonoBehaviour
{
    [SerializeField] private string sceneName = "MainGame";
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Behaviour[] disableDuringCutscene;
    [SerializeField] private bool allowSkip = true;

    /// <summary>Fired when the cutscene finishes or is skipped.</summary>
    public event Action OnCutsceneFinished;

    private bool shouldPlay;
    private bool isPlaying;

    private void Awake()
    {
        var inv = InventoryManager.Instance;

        // Checked in Awake so it runs before TreeSpawner.Start
        // (which may clear the items if "Consume Items" is ticked).
        shouldPlay = SceneManager.GetActiveScene().name == sceneName
                     && inv.HasAllItems()
                     && !inv.BeanstalkCutscenePlayed;
    }

    private void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogWarning("BeanstalkCutscene: assign a Video Player in the Inspector.", this);
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.Stop();

        if (!shouldPlay) return;

        // Draw the video over the whole screen, in front of the scene.
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        if (videoPlayer.targetCamera == null) videoPlayer.targetCamera = Camera.main;
        videoPlayer.targetCameraAlpha = 1f;

        videoPlayer.loopPointReached += HandleVideoFinished;

        SetControlsEnabled(false);
        isPlaying = true;
        InventoryManager.Instance.BeanstalkCutscenePlayed = true;

        videoPlayer.Play();
    }

    private void Update()
    {
        if (isPlaying && allowSkip && SkipPressed())
            EndCutscene();
    }

    private void HandleVideoFinished(VideoPlayer vp) => EndCutscene();

    private void EndCutscene()
    {
        if (!isPlaying) return;
        isPlaying = false;

        videoPlayer.loopPointReached -= HandleVideoFinished;
        videoPlayer.Stop();               // stops drawing the video, revealing the scene + tree

        SetControlsEnabled(true);
        OnCutsceneFinished?.Invoke();
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (disableDuringCutscene == null) return;
        foreach (var b in disableDuringCutscene)
            if (b != null) b.enabled = enabled;
    }

    // Space or Escape skips. Works with both the new Input System and the old Input Manager.
    private static bool SkipPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = UnityEngine.InputSystem.Keyboard.current;
        return kb != null && (kb.spaceKey.wasPressedThisFrame || kb.escapeKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}