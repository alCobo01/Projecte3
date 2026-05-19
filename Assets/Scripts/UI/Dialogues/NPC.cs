using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class NPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue Sequence")]
    [SerializeField] private NPCDialogueSequence dialogueData; 

    [Header("Settings")]
    [SerializeField] protected float interactionCooldown = 0.5f;

    protected int _currentDialogueIndex; 
    protected float _lastInteractionTime;

    public virtual void Interact()
    {
        if (Time.time < _lastInteractionTime + interactionCooldown) return;
        _lastInteractionTime = Time.time;

        if (DialogueManager.Instance.IsDialogueActive)
        {
            DialogueManager.Instance.NextLine();
        }
        else
        {
            StartDialogue(dialogueData);
        }
    }

    public void StartDialogue(NPCDialogueSequence sequence)
    {
        if (sequence == null) return;
        DialogueManager.Instance.StartDialogue(sequence);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerInputController player)) return;
        if (DialogueManager.Instance.IsDialogueActive) DialogueManager.Instance.EndDialogue();
    }
}
