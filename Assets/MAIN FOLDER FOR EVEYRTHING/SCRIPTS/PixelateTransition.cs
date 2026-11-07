using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PixelateTransition : MonoBehaviour
{
    public static PixelateTransition Instance { get; private set; }
    public static bool IsTransitioning { get; private set; }

    public static event Action TransitionStarted;
    public static event Action TransitionFinished;

    [Header("Pixelation")]
    [Tooltip("Size of one block, in screen pixels, at the peak.")]
    [SerializeField, Min(2)] int maxBlockSize = 48;
    [Tooltip("Number of distinct chunky steps. 0 = smooth.")]
    [SerializeField, Min(0)] int steps = 8;

    [Header("Timing (seconds, ignores Time.timeScale)")]
    [SerializeField] float pixelateOutDuration = 0.6f;
    [SerializeField] float holdDuration = 0.15f;
    [SerializeField] float pixelateInDuration = 0.6f;

    [Header("Optional color dip at the peak")]
    [SerializeField] Color dipColor = Color.black;
    [Tooltip("0 = no dip. 1 = fully covers the image at the peak.")]
    [SerializeField, Range(0f, 1f)] float dipAmount = 0f;

    [Header("New scene")]
    [Tooltip("Render the new scene live while it de-pixelates (things keep moving). Off = frozen first frame.")]
    [SerializeField] bool liveEntry = true;

    [Header("Audio (optional)")]
    [SerializeField] AudioClip sound;
    [SerializeField, Range(0f, 1f)] float soundVolume = 1f;

    Canvas canvas;
    RawImage display;
    Image dip;
    RenderTexture lowRes;
    AudioSource audioSource;

    // ------------------------------------------------------------------ public API

    public static void Play(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("PixelateTransition: no instance in the scene. Loading without transition.");
            SceneManager.LoadScene(sceneName);
            return;
        }
        if (IsTransitioning) return;
        Instance.StartCoroutine(Instance.Run(sceneName));
    }

    // ------------------------------------------------------------------ setup

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildOverlay();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true;
    }

    void BuildOverlay()
    {
        var canvasGO = new GameObject("PixelateCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>(); // swallows clicks during the transition

        var displayRect = CreateChild("PixelatedFrame", canvasGO.transform);
        Stretch(displayRect);
        display = displayRect.gameObject.AddComponent<RawImage>();
        display.color = Color.white;
        display.raycastTarget = true;

        var dipRect = CreateChild("Dip", canvasGO.transform);
        Stretch(dipRect);
        dip = dipRect.gameObject.AddComponent<Image>();
        dip.raycastTarget = false;
        SetDip(0f);

        canvas.enabled = false;
    }

    // ------------------------------------------------------------------ the transition

    IEnumerator Run(string sceneName)
    {
        IsTransitioning = true;
        TransitionStarted?.Invoke();

        // freeze
        yield return new WaitForEndOfFrame();
        Texture2D oldShot = ScreenCapture.CaptureScreenshotAsTexture();

        ShowPixelated(oldShot, 1f);
        SetDip(0f);
        canvas.enabled = true;

        if (sound != null) audioSource.PlayOneShot(sound, soundVolume);

        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        load.allowSceneActivation = false;

        //  pixelate
        for (float t = 0f; t < pixelateOutDuration; t += Time.unscaledDeltaTime)
        {
            float p = t / pixelateOutDuration;
            ShowPixelated(oldShot, BlockSize(p));
            SetDip(dipAmount * p);
            yield return null;
        }
        ShowPixelated(oldShot, maxBlockSize);
        SetDip(dipAmount);

        // hold
        for (float t = 0f; t < holdDuration || load.progress < 0.9f; t += Time.unscaledDeltaTime)
            yield return null;

        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;
        yield return null; // let the new scene run Start() and place its camera

        Destroy(oldShot);

        // new scene pixelate
        var full = new RenderTexture(Screen.width, Screen.height, 24);
        Camera cam = Camera.main;
        if (cam == null) Debug.LogWarning("PixelateTransition: no MainCamera in the new scene.");
        RenderCamera(cam, full);

        for (float t = 0f; t < pixelateInDuration; t += Time.unscaledDeltaTime)
        {
            float p = t / pixelateInDuration;
            if (liveEntry) RenderCamera(cam, full);
            ShowPixelated(full, BlockSize(1f - p));
            SetDip(dipAmount * (1f - p));
            yield return null;
        }

        // new scene
        canvas.enabled = false;
        display.texture = null;
        if (lowRes != null) { RenderTexture.ReleaseTemporary(lowRes); lowRes = null; }
        full.Release();
        Destroy(full);

        IsTransitioning = false;
        TransitionFinished?.Invoke();
    }

    // ------------------------------------------------------------------ pixelation
    
    float BlockSize(float progress)
    {
        progress = Mathf.Clamp01(progress);
        if (steps > 0) progress = Mathf.Round(progress * steps) / steps;
        return Mathf.Pow(maxBlockSize, progress);
    }

    void ShowPixelated(Texture source, float blockSize)
    {
        if (source == null) return;
        int w = Mathf.Max(1, Mathf.RoundToInt(Screen.width / blockSize));
        int h = Mathf.Max(1, Mathf.RoundToInt(Screen.height / blockSize));

        if (lowRes == null || lowRes.width != w || lowRes.height != h)
        {
            if (lowRes != null) RenderTexture.ReleaseTemporary(lowRes);
            lowRes = RenderTexture.GetTemporary(w, h, 0);
            lowRes.filterMode = FilterMode.Point;
        }

        Graphics.Blit(source, lowRes);
        display.texture = lowRes;
    }

    static void RenderCamera(Camera cam, RenderTexture target)
    {
        if (cam == null) return;
#if UNITY_2023_2_OR_NEWER
        var request = new RenderPipeline.StandardRequest { destination = target };
        if (RenderPipeline.SupportsRenderRequest(cam, request))
        {
            RenderPipeline.SubmitRenderRequest(cam, request);
            return;
        }
#endif
        RenderTexture prev = cam.targetTexture;
        cam.targetTexture = target;
        cam.Render();
        cam.targetTexture = prev;
    }

    // ------------------------------------------------------------------ helpers

    void SetDip(float alpha)
    {
        var c = dipColor;
        c.a = alpha;
        dip.color = c;
        dip.enabled = alpha > 0f;
    }

    static RectTransform CreateChild(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}