using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI textDisplay;
    public TextMeshProUGUI nameDisplay;
    public Image portraitDisplay;
    public AudioSource audioSource;
    public AudioSource musicSource;

    [Header("Settings")]
    [Range(0.1f, 0.5f)] public float pitchVariation = 0.2f;
    public bool dialogueHappening;

    private PlayerController playerMovementScript;
    private DialogueData currentDialogue;
    private NPCInteraction activeNPC;
    private int lineIndex;
    private bool isTyping;
    private bool cancelTyping;
    private DialogueLine currentLineData;
    private float basePitch = 1.0f;
    private bool canInput = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerMovementScript = player.GetComponent<PlayerController>();
        if (audioSource != null) basePitch = audioSource.pitch;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void DisableControlForInteraction()
    {
        if (playerMovementScript != null)
        {
            //playerMovementScript.ToggleControl();
        }
    }

    public void StartDialogue(DialogueData data, NPCInteraction npc)
    {
        // Absolute safety flush before starting a new chat
        StopAllCoroutines();

        activeNPC = npc;
        currentDialogue = data;
        lineIndex = 0;
        currentLineData = null;
        dialogueHappening = true;
        canInput = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        if (musicSource != null && data.themeTrack != null)
        {
            musicSource.clip = data.themeTrack;
            musicSource.Play();
        }

        StartCoroutine(PlayLine());
    }

    // Called directly by NPCInteraction when the same interact button is pressed during conversation
    public void TryAdvanceDialogue()
    {
        if (!dialogueHappening || !canInput) return;

        if (isTyping)
        {
            cancelTyping = true;
        }
        else
        {
            NextLine();
        }
    }

    void NextLine()
    {
        lineIndex++;
        if (lineIndex < currentDialogue.lines.Count)
        {
            StopAllCoroutines();
            StartCoroutine(PlayLine());
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        StopAllCoroutines();
        dialogueHappening = false;
        canInput = false;

        

        activeNPC = null;
        currentLineData = null;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (musicSource != null) musicSource.Stop();

        if (playerMovementScript != null)
        {
            // playerMovementScript.ToggleControl();
        }
    }

    IEnumerator PlayLine()
    {
        canInput = false;
        isTyping = true;
        cancelTyping = false;
        currentLineData = currentDialogue.lines[lineIndex];

    

        nameDisplay.text = currentLineData.characterName;
        textDisplay.text = currentLineData.text;
        textDisplay.maxVisibleCharacters = 0;

        if (portraitDisplay != null)
        {
            portraitDisplay.gameObject.SetActive(currentLineData.portrait != null);
            portraitDisplay.sprite = currentLineData.portrait;
        }

        yield return new WaitForEndOfFrame();
        textDisplay.ForceMeshUpdate();

        int totalChars = textDisplay.textInfo.characterCount;
        if (totalChars == 0) totalChars = currentLineData.text.Length;

        // Brief buffer to prevent the initial interaction button press from instantly skipping text
        yield return new WaitForSecondsRealtime(0.15f);
        canInput = true;

        for (int i = 0; i <= totalChars; i++)
        {
            if (cancelTyping)
            {
                textDisplay.maxVisibleCharacters = totalChars;
                break;
            }

            textDisplay.maxVisibleCharacters = i;

            if (i < totalChars && !char.IsWhiteSpace(currentLineData.text[i]))
            {
                if (currentLineData.typingSound != null && audioSource != null)
                {
                    audioSource.pitch = basePitch + Random.Range(-pitchVariation, pitchVariation);
                    audioSource.PlayOneShot(currentLineData.typingSound);
                }
            }

            yield return new WaitForSeconds(currentLineData.typingSpeed);
        }

        isTyping = false;
       
    }

    void LateUpdate()
    {
        if (currentLineData == null || textDisplay == null || !dialogueHappening || currentLineData.effect == TextEffect.None) return;

        textDisplay.ForceMeshUpdate();
        var textInfo = textDisplay.textInfo;
        if (textInfo == null || textInfo.characterCount == 0) return;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIndex = charInfo.materialReferenceIndex;
            if (textInfo.meshInfo == null || matIndex >= textInfo.meshInfo.Length) continue;

            var verts = textInfo.meshInfo[matIndex].vertices;
            if (verts == null || charInfo.vertexIndex >= verts.Length) continue;

            Vector3 charCenter = (verts[charInfo.vertexIndex + 0] + verts[charInfo.vertexIndex + 2]) / 2f;

            for (int j = 0; j < 4; j++)
            {
                if ((charInfo.vertexIndex + j) < verts.Length)
                    verts[charInfo.vertexIndex + j] = ApplyEffect(verts[charInfo.vertexIndex + j], charCenter, i, currentLineData.effect);
            }
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            if (meshInfo.mesh == null) continue;
            meshInfo.mesh.vertices = meshInfo.vertices;
            textDisplay.UpdateGeometry(meshInfo.mesh, i);
        }
    }

    Vector3 ApplyEffect(Vector3 pos, Vector3 center, int charIndex, TextEffect effect)
    {
        switch (effect)
        {
            case TextEffect.Wiggle: return pos + new Vector3(0, Mathf.Sin(Time.time * 10f + charIndex * 0.5f) * 5f, 0);
            case TextEffect.Slam: return pos + ((pos - center) * 0.5f) + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
            case TextEffect.Small: return pos - ((pos - center) * 0.4f);
            default: return pos;
        }
    }
}