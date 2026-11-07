using UnityEngine;
using System.Collections.Generic;

public enum TextEffect { None, Wiggle, Slam, Small }
public enum NPCExpression { Idle, Happy, Angry, Sad, Shock, Fear, }

[System.Serializable]
public class DialogueLine
{
    public string characterName;
    public Sprite portrait;
    [TextArea(3, 10)] public string text;
    public AudioClip typingSound;
    public TextEffect effect;
    public float typingSpeed = 0.05f;

    [Header("NPC Visuals")]
    public NPCExpression expression;
    public string emoteTrigger;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Conversation")]
public class DialogueData : ScriptableObject
{
    public AudioClip themeTrack;
    public List<DialogueLine> lines;
}