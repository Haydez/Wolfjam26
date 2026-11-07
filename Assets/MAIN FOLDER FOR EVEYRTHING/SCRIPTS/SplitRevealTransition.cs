using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// the current frame splits down the middle and
/// the two halves slide apart to reveal the next scene behind them


/// Sequence:
///   Freeze : the current frame is captured and split into two halves.
///   Load   : the next scene loads behind the closed halves.
///   Crack  : the halves part slightly, so light from the new scene shows through the seam.
///   Open   : the halves swing out to the sides while the new scene pushes in slightly.
/// </summary>
public class SplitRevealTransition : MonoBehaviour
{
    public enum SplitDirection { Vertical, Horizontal }

    public static SplitRevealTransition Instance { get; private set; }
    public static bool IsTransitioning { get; private set; }

    public static event Action TransitionStarted;
    public static event Action TransitionFinished;

    [Header("Split")]
    [Tooltip("Vertical = left/right doors. Horizontal = top/bottom doors.")]
    [SerializeField] SplitDirection direction = SplitDirection.Vertical;

    [Header("Timing (seconds, ignores Time.timeScale)")]
    [Tooltip("Short beat before anything moves, so the freeze reads as intentional.")]
    [SerializeField] float startDelay = 0.1f;
    [SerializeField] float crackDuration = 0.2f;
    [SerializeField] float crackHold = 0.15f;
    [SerializeField] float openDuration = 0.7f;

    [Header("Door feel")]
    [Tooltip("How far the doors part during the crack, as a fraction of each half's width.")]
    [SerializeField, Range(0f, 0.3f)] float crackAmount = 0.04f;
    [Tooltip("Doors shrink slightly as they swing away, like real doors opening toward the camera.")]
    [SerializeField, Range(0f, 0.3f)] float doorShrink = 0.08f;
    [Tooltip("Darkens the doors as they open, as if they're turning away from the light.")]
    [SerializeField, Range(0f, 1f)] float doorDarken = 0.35f;
    [Tooltip("Width of the shadow on each door's inner edge, in pixels. 0 = none.")]
    [SerializeField, Min(0f)] float edgeShadowWidth = 24f;
    [SerializeField] Color edgeShadowColor = new Color(0f, 0f, 0f, 0.6f);

    [Header("New scene push-in")]
    [Tooltip("New scene starts this much larger and settles to normal size. 0 = off.")]
    [SerializeField, Range(0f, 0.3f)] float pushInAmount = 0.08f;

    [Header("Audio (optional)")]
    [SerializeField] AudioClip sound;
    [SerializeField, Range(0f, 1f)] float soundVolume = 1f;

    Canvas canvas;
    RectTransform root, behindRect, doorA, doorB;
    RawImage behindImage, doorAImage, doorBImage;
    AudioSource audioSource;

    // ------------------------------------------------------------------ public API

    public static void Play(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("SplitRevealTransition: no instance in the scene. Loading without transition.");
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
        if (transform.parent == null) DontDestroyOnLoad(gameObject);
        BuildOverlay();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.ignoreListenerPause = true;
    }

    void BuildOverlay()
    {
        var canvasGO = new GameObject("SplitRevealCanvas", typeof(RectTransform));
        canvasGO.transform.SetParent(transform, false);

        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32000;
        canvasGO.AddComponent<CanvasScaler>(); // constant pixel size
        canvasGO.AddComponent<GraphicRaycaster>();
        root = (RectTransform)canvasGO.transform;

        // view of the new scene
        behindRect = CreateChild("NewScene", root);
        Stretch(behindRect);
        behindImage = behindRect.gameObject.AddComponent<RawImage>();
        behindImage.raycastTarget = false;

        doorA = CreateDoor("DoorA", out doorAImage);
        doorB = CreateDoor("DoorB", out doorBImage);

        canvas.enabled = false;
    }

    RectTransform CreateDoor(string name, out RawImage image)
    {
        var rt = CreateChild(name, root);
        image = rt.gameObject.AddComponent<RawImage>();
        image.raycastTarget = true; // blocks clicks while the doors are closed

        if (edgeShadowWidth > 0f)
        {
            var shadow = CreateChild("EdgeShadow", rt);
            var img = shadow.gameObject.AddComponent<Image>();
            img.color = edgeShadowColor;
            img.raycastTarget = false;
        }
        return rt;
    }

    // lays out both doors to exactly cover their half of the screen
    void LayoutDoors()
    {
        bool vertical = direction == SplitDirection.Vertical;

        // Door A = left (or bottom), Door B = right (or top).
        SetupDoor(doorA, doorAImage,
            vertical ? new Vector2(0f, 0f) : new Vector2(0f, 0f),
            vertical ? new Vector2(0.5f, 1f) : new Vector2(1f, 0.5f),
            vertical ? new Rect(0f, 0f, 0.5f, 1f) : new Rect(0f, 0f, 1f, 0.5f),
            vertical ? new Vector2(1f, 0.5f) : new Vector2(0.5f, 1f));

        SetupDoor(doorB, doorBImage,
            vertical ? new Vector2(0.5f, 0f) : new Vector2(0f, 0.5f),
            vertical ? new Vector2(1f, 1f) : new Vector2(1f, 1f),
            vertical ? new Rect(0.5f, 0f, 0.5f, 1f) : new Rect(0f, 0.5f, 1f, 0.5f),
            vertical ? new Vector2(0f, 0.5f) : new Vector2(0.5f, 0f));
    }

    void SetupDoor(RectTransform door, RawImage image, Vector2 aMin, Vector2 aMax, Rect uv, Vector2 innerEdge)
    {
        door.anchorMin = aMin;
        door.anchorMax = aMax;
        door.offsetMin = door.offsetMax = Vector2.zero;
        //pivot on the outer edge so the shrink looks like a door swinging on its hinge
        door.pivot = Vector2.one - innerEdge;
        door.anchoredPosition = Vector2.zero;
        door.localScale = Vector3.one;
        image.uvRect = uv;
        image.color = Color.white;

        // inner shadow strip for depth for doors
        if (door.childCount > 0)
        {
            var shadow = (RectTransform)door.GetChild(0);
            bool vertical = direction == SplitDirection.Vertical;
            shadow.anchorMin = innerEdge;
            shadow.anchorMax = innerEdge;
            if (vertical) { shadow.anchorMin = new Vector2(innerEdge.x, 0f); shadow.anchorMax = new Vector2(innerEdge.x, 1f); }
            else          { shadow.anchorMin = new Vector2(0f, innerEdge.y); shadow.anchorMax = new Vector2(1f, innerEdge.y); }
            shadow.pivot = innerEdge;
            shadow.sizeDelta = vertical ? new Vector2(edgeShadowWidth, 0f) : new Vector2(0f, edgeShadowWidth);
            shadow.anchoredPosition = Vector2.zero;
            shadow.gameObject.SetActive(false); // only shows once the doors part
        }
    }

    // ------------------------------------------------------------------ the transition

    IEnumerator Run(string sceneName)
    {
        IsTransitioning = true;
        TransitionStarted?.Invoke();

        // freeze the current frame.
        yield return new WaitForEndOfFrame();
        Texture2D oldShot = ScreenCapture.CaptureScreenshotAsTexture();

        doorAImage.texture = oldShot;
        doorBImage.texture = oldShot;
        LayoutDoors();
        behindImage.enabled = false;
        canvas.enabled = true;

        // load the next scene behind the closed doors
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName);
        load.allowSceneActivation = false;

        for (float t = 0f; t < startDelay || load.progress < 0.9f; t += Time.unscaledDeltaTime)
            yield return null;

        load.allowSceneActivation = true;
        while (!load.isDone) yield return null;
        yield return null; // let the new scene run Start() and place its camera

        // live render of new scene
        Camera cam = Camera.main;
        RenderTexture live = null;
        if (pushInAmount > 0f && cam != null)
        {
            live = new RenderTexture(Screen.width, Screen.height, 24);
            RenderCamera(cam, live);
            behindImage.texture = live;
            behindImage.enabled = true;
            behindRect.localScale = Vector3.one * (1f + pushInAmount);
        }

        if (sound != null) audioSource.PlayOneShot(sound, soundVolume);

        float halfSize = direction == SplitDirection.Vertical
            ? root.rect.width * 0.5f
            : root.rect.height * 0.5f;

        SetShadows(edgeShadowWidth > 0f);

        // doors part a little and pause
        for (float t = 0f; t < crackDuration; t += Time.unscaledDeltaTime)
        {
            float e = EaseOutCubic(t / crackDuration);
            PlaceDoors(halfSize * crackAmount * e, 0f);
            if (live != null) RenderCamera(cam, live);
            yield return null;
        }
        PlaceDoors(halfSize * crackAmount, 0f);

        for (float t = 0f; t < crackHold; t += Time.unscaledDeltaTime)
        {
            if (live != null) RenderCamera(cam, live);
            yield return null;
        }

        // doors swing fully out while the new scene settles
        float start = halfSize * crackAmount;
        float end = halfSize * (1f + doorShrink) + edgeShadowWidth + 2f; // fully off-screen
        for (float t = 0f; t < openDuration; t += Time.unscaledDeltaTime)
        {
            float p = t / openDuration;
            float e = EaseInOutCubic(p);
            PlaceDoors(Mathf.Lerp(start, end, e), e);

            if (live != null)
            {
                RenderCamera(cam, live);
                behindRect.localScale = Vector3.one * (1f + pushInAmount * (1f - EaseOutCubic(p)));
            }
            yield return null;
        }

        // back to the live scene
        canvas.enabled = false;
        doorAImage.texture = doorBImage.texture = null;
        behindImage.texture = null;
        behindRect.localScale = Vector3.one;
        Destroy(oldShot);
        if (live != null) { live.Release(); Destroy(live); }

        IsTransitioning = false;
        TransitionFinished?.Invoke();
    }

    // offset = how far each door has moved away from the seam 
    // openness = drives the shrink and darkening
    void PlaceDoors(float offset, float openness)
    {
        bool vertical = direction == SplitDirection.Vertical;
        Vector2 dir = vertical ? Vector2.right : Vector2.up;

        doorA.anchoredPosition = -dir * offset;
        doorB.anchoredPosition =  dir * offset;

        // shrink only across the split axis' perpendicular, like a door turning on its hinge
        float s = 1f - doorShrink * openness;
        Vector3 scale = vertical ? new Vector3(s, 1f - doorShrink * 0.5f * openness, 1f)
                                 : new Vector3(1f - doorShrink * 0.5f * openness, s, 1f);
        doorA.localScale = doorB.localScale = scale;

        float shade = 1f - doorDarken * openness;
        doorAImage.color = doorBImage.color = new Color(shade, shade, shade, 1f);
    }

    void SetShadows(bool on)
    {
        if (doorA.childCount > 0) doorA.GetChild(0).gameObject.SetActive(on);
        if (doorB.childCount > 0) doorB.GetChild(0).gameObject.SetActive(on);
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

    static float EaseOutCubic(float x) { x = Mathf.Clamp01(x); float i = 1f - x; return 1f - i * i * i; }
    static float EaseInOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }
}