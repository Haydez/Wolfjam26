
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

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip newMusic;
    [Tooltip("Off: music switches when the cutscene starts. On: it switches when the cutscene ends.")]
    [SerializeField] private bool swapMusicAfterVideo = false;
    
    public event Action OnCutsceneFinished;

    private bool shouldPlay;
    private bool isPlaying;

    private void Awake()
    {
        var inv = InventoryManager.Instance;
        
        shouldPlay = SceneManager.GetActiveScene().name == sceneName
                     && inv.HasAllItems()
                     && !inv.BeanstalkCutscenePlayed;
    }

    private void Start()
    {
        if (InventoryManager.Instance.BeanstalkCutscenePlayed)
        {
            SwapMusic();
            return;
        }

        if (videoPlayer == null)
        {
            Debug.LogWarning("BeanstalkCutscene: assign a Video Player in the Inspector.", this);
            return;
        }

        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        videoPlayer.Stop();

        if (!shouldPlay) return;
        
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        if (videoPlayer.targetCamera == null) videoPlayer.targetCamera = Camera.main;
        videoPlayer.targetCameraAlpha = 1f;

        videoPlayer.loopPointReached += HandleVideoFinished;

        SetControlsEnabled(false);
        isPlaying = true;
        InventoryManager.Instance.BeanstalkCutscenePlayed = true;

        if (!swapMusicAfterVideo) SwapMusic();

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
        videoPlayer.Stop();               

        if (swapMusicAfterVideo) SwapMusic();

        SetControlsEnabled(true);
        OnCutsceneFinished?.Invoke();
    }

    private void SwapMusic()
    {
        if (musicSource == null || newMusic == null) return;
        if (musicSource.clip == newMusic && musicSource.isPlaying) return;

        musicSource.Stop();
        musicSource.clip = newMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (disableDuringCutscene == null) return;
        foreach (var b in disableDuringCutscene)
            if (b != null) b.enabled = enabled;
    }
    
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