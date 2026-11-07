using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Splash")]
    [SerializeField] GameObject splash;
    [SerializeField] CanvasGroup logo;
    [SerializeField] float fadeIn = 1.5f, hold = 2f, fadeOut = 1.5f;

    [Header("Menu")]
    [SerializeField] CanvasGroup menu;
    [SerializeField] GameObject optionsPanel, creditsPanel;
    [SerializeField] float menuFadeIn = 0.8f;

    [Header("Start Game")]
    [SerializeField] string gameSceneName = "MainGame";
    [SerializeField] float fadeToGame = 1f;

    static bool splashShown;   // so returning to the menu skips the logo
    bool starting;             // stops Start being pressed twice

    IEnumerator Start()
    {
        menu.gameObject.SetActive(false);

        if (!splashShown)
        {
            splash.SetActive(true);
            logo.alpha = 0;
            yield return Fade(logo, 0, 1, fadeIn, true);
            yield return Wait(hold);
            yield return Fade(logo, 1, 0, fadeOut, true);
            yield return new WaitForSeconds(0.3f);
            splashShown = true;
        }

        splash.SetActive(false);
        menu.gameObject.SetActive(true);
        menu.alpha = 0;
        menu.interactable = false;
        yield return Fade(menu, 0, 1, menuFadeIn, false);
        menu.interactable = true;
    }

    // ---- button functions ----

    public void StartGame()
    {
        if (starting) return;
        starting = true;
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        if (!Application.CanStreamedLevelBeLoaded(gameSceneName))
        {
            Debug.LogError($"Scene '{gameSceneName}' isn't in the Build Profile scene list, or the name doesn't match.");
            starting = false;
            yield break;
        }

        menu.interactable = false;
        ClosePanels();

        // load the game in the background while the menu fades to black
        AsyncOperation load = SceneManager.LoadSceneAsync(gameSceneName);
        load.allowSceneActivation = false;

        yield return Fade(menu, menu.alpha, 0, fadeToGame, false);

        while (load.progress < 0.9f)   // 0.9 = loaded and waiting to switch
            yield return null;

        load.allowSceneActivation = true;
    }

    public void OpenOptions()  => optionsPanel.SetActive(true);
    public void OpenCredits()  => creditsPanel.SetActive(true);
    public void ClosePanels()  { optionsPanel.SetActive(false); creditsPanel.SetActive(false); }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;   // so Quit works in the editor too
#endif
    }

    // ---- helpers ----

    IEnumerator Fade(CanvasGroup g, float from, float to, float time, bool skippable)
    {
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            if (skippable && SkipPressed()) break;
            g.alpha = Mathf.SmoothStep(from, to, t / time);
            yield return null;
        }
        g.alpha = to;
    }

    IEnumerator Wait(float time)
    {
        for (float t = 0; t < time && !SkipPressed(); t += Time.deltaTime)
            yield return null;
    }

    bool SkipPressed()
    {
        return (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            || (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame);
    }
}