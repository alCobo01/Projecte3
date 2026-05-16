using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class NPC : MonoBehaviour, IInteractable
{
    [Header("Dialogue Sequence")]
    [SerializeField] private NPCDialogueSequence dialogueData; 

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Settings")]
    [SerializeField] private float interactionCooldown = 0.5f;

    private int _currentDialogueIndex, _lineIndex; 
    private float _lastInteractionTime;
    private bool _isTyping, _isDialogueActive;

    public void Interact()
    {
        if (Time.time < _lastInteractionTime + interactionCooldown) return;
        _lastInteractionTime = Time.time;

        if (_isDialogueActive)
            NextLine();
        else
            StartDialogue();
    }

    private void StartDialogue()
    {
        if (dialogueData == null || dialogueData.dialogues.Length == 0)
        {
            Debug.LogWarning($"[NPC] {gameObject.name} no tiene diálogos asignados.");
            return;
        }

        _lineIndex = 0;
        _isDialogueActive = true;

        dialoguePanel.SetActive(true);
        StartCoroutine(TypeLine());
    }

    private void NextLine()
    {
        var block = dialogueData.dialogues[Mathf.Min(_currentDialogueIndex, dialogueData.dialogues.Length - 1)];

        if (_isTyping)
        {
            StopAllCoroutines();
            dialogueText.SetText(block.lines[_lineIndex]);
            _isTyping = false;
        }
        else
        {
            _lineIndex++;

            if (_lineIndex < block.lines.Length)
                StartCoroutine(TypeLine());
            else
                EndDialogue(true);
        }
    }

    private IEnumerator TypeLine()
    {
        var block = dialogueData.dialogues[Mathf.Min(_currentDialogueIndex, dialogueData.dialogues.Length - 1)];

        _isTyping = true;
        dialogueText.SetText("");

        foreach (var letter in block.lines[_lineIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(block.typingSpeed);
        }

        _isTyping = false;
    }

    private void EndDialogue(bool completed)
    {
        StopAllCoroutines();
        _isDialogueActive = false;

        dialogueText.SetText("");
        dialoguePanel.SetActive(false);

        // Avanzar al siguiente diálogo solo si se ha completado y no estamos en el último
        if (completed && _currentDialogueIndex < dialogueData.dialogues.Length - 1)
            _currentDialogueIndex++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerInputController player)) return;
        if (_isDialogueActive) EndDialogue(false);
    }

}
