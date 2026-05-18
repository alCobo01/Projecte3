using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogueSequence", menuName = "Scriptable Objects/NPC Dialogue Sequence")]
public class NPCDialogueSequence : ScriptableObject
{
    [System.Serializable]
    public class DialogueBlock
    {
        public Sprite speakerPortrait;
        public string[] lines;
        public float typingSpeed = 0.08f;
    }

    public DialogueBlock[] dialogues;
}
