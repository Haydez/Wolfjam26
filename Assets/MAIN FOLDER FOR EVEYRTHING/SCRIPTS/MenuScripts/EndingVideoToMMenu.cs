
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class EndingVideoToMMenu : MonoBehaviour
{
    [Tooltip("Scene to load when the video ends. Leave blank to stay here.")]
    [SerializeField] private string nextScene = "";

    [SerializeField] private bool loop = false;
    [SerializeField] private bool allowSkip = true;

    private VideoPlayer videoPlayer;
    private bool finished;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
        if (videoPlayer.targetCamera == null)
            videoPlayer.targetCamera = GetComponent<Camera>() != null ? GetComponent<Camera>() : Camera.main;
        videoPlayer.targetCameraAlpha = 1f;

        videoPlayer.isLooping = loop;
        videoPlayer.playOnAwake = false; 
        videoPlayer.loopPointReached += OnVideoFinished;

        videoPlayer.Play();
    }

    private void Update()
    {
        if (allowSkip && !finished && SkipPressed())
            Finish();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (!loop) Finish();
    }

    private void Finish()
    {
        if (finished) return;
        finished = true;

        videoPlayer.loopPointReached -= OnVideoFinished;
        videoPlayer.Stop();

        if (!string.IsNullOrEmpty(nextScene))
            SceneManager.LoadScene(nextScene);
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