using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BatSpinTransition : MonoBehaviour
{
    public class SceneDoor : MonoBehaviour
    {
        [SerializeField] string nextScene;

        public void Interact()
        {
            BatSpinTransition.Play(nextScene);
        }
    }
    public static BatSpinTransition Instance { get; private set; }
    public static bool IsTransitioning { get; private set; }

    public static event Action TransitionStarted;
    public static event Action TransitionFinished;

    [Header("Look")]
    [SerializeField] Sprite emblem;
    [SerializeField] Vector2 emblemSize = new Vector2(700f, 700f);
    [SerializeField] Color backdropColor = new Color(0.08f, 0.10f, 0.45f, 1f);

    [Header("Timing (seconds, ignores Time.timeScale)")]
    [SerializeField] float spinOutDuration = 0.55f;
    [SerializeField] float emblemHold = 0.35f;
    [SerializeField] float spinInDuration = 0.55f;

    [Header("Motion")]
    [Tooltip("Whole turns so panels always end upright.")]
    [SerializeField, Min(1)] int spinTurns = 2;
    [Tooltip("How far the emblem 'comes toward the camera' before settling back.")]
    [SerializeField] float emblemPeakScale = 1.3f;
    [Tooltip("When during the spin-out (0-1) the emblem starts appearing.")]
    [SerializeField, Range(0f, 0.9f)] float emblemStartsAt = 0.25f;

    [Header("Audio (optional)")]
    [SerializeField] AudioClip sting;
    [SerializeField, Range(0f, 1f)] float stingVolume = 1f;

    Canvas canvas;
    RectTransform sceneRect, emblemRect;
    RawImage sceneImage;
    Image backdrop;
    AudioSource audioSource;

    // ------------------------------------------------------------------ public API

    public static void Play(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("BatSpinTransition: no instance in the scene. Loading without transition.");
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
        var canvasGO = new GameObject("BatSpinCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000; // above everything

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>(); // swallows clicks during the transition

        var bgRect = CreateChild("Backdrop", canvasGO.transform);
        Stretch(bgRect);
        backdrop = bgRect.gameObject.AddComponent<Image>();
        backdrop.color = backdropColor;
        backdrop.raycastTarget = true;

        sceneRect = CreateChild("ScenePanel", canvasGO.transform);
        Stretch(sceneRect);
        sceneImage = sceneRect.gameObject.AddComponent<RawImage>();
        sceneImage.raycastTarget = false;

        emblemRect = CreateChild("Emblem", canvasGO.transform);
        emblemRect.anchorMin = emblemRect.anchorMax = new Vector2(0.5f, 0.5f);
        emblemRect.sizeDelta = emblemSize;
        var emblemImage = emblemRect.gameObject.AddComponent<Image>();
        emblemImage.sprite = emblem;
        emblemImage.preserveAspect = true;
        emblemImage.raycastTarget = false;
        emblemImage.enabled = emblem != null;

        canvas.enabled = false;
    }

    // ----------------------------------------------------------------- the transition

    IEnumerator Run(string sceneName)
    {
        IsTransitioning = true;
        TransitionStarted?.Invoke();

        // freeze the current frame
        yield return new WaitForEndOfFrame();
        Texture2D oldShot = ScreenCapture.CaptureScreenshotAsTexture();

        sceneImage.texture = oldShot;
        SetPanel(sceneRect, 1f, 0f);
        SetPanel(emblemRect, 0f, 0f);
        backdrop.color = backdropColor;
        canvas.enabled = true;

        if (sting != null) audioSource.PlayOneShot(sting, stingVolume);

        // s tart loading underneath the overlay right away
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        load.allowSceneActivation = false;

        float fullSpin = 360f * spinTurns;

        // spin out, old scene shrinks away, emblem spins up toward the camera
        for (float t = 0f; t < spinOutDuration; t += Time.unscaledDeltaTime)
        {
            float p = t / spinOutDuration;
            float e = EaseInCubic(p);
            SetPanel(sceneRect, 1f - e, fullSpin * e);

            float ep = Mathf.Clamp01((p - emblemStartsAt) / (1f - emblemStartsAt));
            float ee = EaseOutCubic(ep);
            SetPanel(emblemRect, emblemPeakScale * ee, -fullSpin * (1f - ee));
            yield return null;
        }
        SetPanel(sceneRect, 0f, 0f);
        SetPanel(emblemRect, emblemPeakScale, 0f);
        Destroy(oldShot);

        //  Holds emblem, settles back into place
        float settle = Mathf.Max(0.01f, emblemHold);
        for (float t = 0f; t < settle; t += Time.unscaledDeltaTime)
        {
            float q = EaseOutBack(t / settle);
            SetPanel(emblemRect, Mathf.LerpUnclamped(emblemPeakScale, 1f, q), 0f);
            yield return null;
        }
        SetPanel(emblemRect, 1f, 0f);

        while (load.progress < 0.9f) yield return null;
        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;

        yield return null;

        //  new scene's camera
        RenderTexture newShot = RenderNewScene();
        sceneImage.texture = newShot;

        //  spin-in
        for (float t = 0f; t < spinInDuration; t += Time.unscaledDeltaTime)
        {
            float p = t / spinInDuration;
            float e = EaseOutCubic(p);
            SetPanel(sceneRect, e, -fullSpin * (1f - e));

            float ep = Mathf.Clamp01(p / 0.6f);
            float ee = EaseInCubic(ep);
            SetPanel(emblemRect, 1f - ee, fullSpin * ee);
            yield return null;
        }

        // back to the live scene
        canvas.enabled = false;
        sceneImage.texture = null;
        if (newShot != null) { newShot.Release(); Destroy(newShot); }

        IsTransitioning = false;
        TransitionFinished?.Invoke();
    }

    RenderTexture RenderNewScene()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("BatSpinTransition: no MainCamera in the new scene; spinning in a blank panel.");
            return null;
        }

        var rt = new RenderTexture(Screen.width, Screen.height, 24);
        
        var request = new RenderPipeline.StandardRequest { destination = rt };
        
        if (RenderPipeline.SupportsRenderRequest(cam, request))
        {
            RenderPipeline.SubmitRenderRequest(cam, request);
            return rt;
        }
        
        RenderTexture prev = cam.targetTexture;
        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = prev;
        return rt;
    }

    // ------------------------------------------------------------------ helpers

    static void SetPanel(RectTransform rt, float scale, float zDegrees)
    {
        rt.localScale = new Vector3(scale, scale, 1f);
        rt.localEulerAngles = new Vector3(0f, 0f, zDegrees);
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
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    static float EaseInCubic(float x) { x = Mathf.Clamp01(x); return x * x * x; }
    static float EaseOutCubic(float x) { x = Mathf.Clamp01(x); float i = 1f - x; return 1f - i * i * i; }
    static float EaseOutBack(float x)
    {
        x = Mathf.Clamp01(x);
        const float c1 = 1.70158f, c3 = c1 + 1f;
        float m = x - 1f;
        return 1f + c3 * m * m * m + c1 * m * m;
    }
}