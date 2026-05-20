using UnityEngine;

/// <summary>
/// Triggers an NPC dialogue automatically when a player enters the trigger zone.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DialogueAreaTrigger : MonoBehaviour
{
    [Header("Dialogue")]
    [Tooltip("The dialogue sequence to trigger.")]
    [SerializeField] private NPCDialogueSequence dialogueSequence;
    [SerializeField] private string dialogueId;

    [Header("Settings")]
    [Tooltip("If true, the dialogue will only trigger once.")]
    [SerializeField] private bool triggerOnce = true;
    
    [Tooltip("If true, the player won't be able to move during this dialogue.")]
    [SerializeField] private bool freezePlayer = true;

    private bool _hasTriggered = false;

    private void Awake()
    {
        // Ensure the collider is set as a trigger
        if (TryGetComponent(out Collider2D col))
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it should only trigger once
        if (_hasTriggered && triggerOnce) return;

        if (triggerOnce && PlayerStatsManager.Instance != null &&
            PlayerStatsManager.Instance.HasCompletedDialogue(dialogueId))
            return;

        // Detect player
        if (other.CompareTag("Player"))
        {
            if (DialogueManager.Instance != null && dialogueSequence != null)
            {
                // Trigger dialogue directly through the Manager, passing the freeze setting
                DialogueManager.Instance.StartDialogue(dialogueSequence, freezePlayer);
                _hasTriggered = true;
                if (triggerOnce && PlayerStatsManager.Instance != null)
                {
                    if (string.IsNullOrEmpty(dialogueId) && dialogueSequence != null)
                        dialogueId = dialogueSequence.name;
                    PlayerStatsManager.Instance.MarkDialogueCompleted(dialogueId);
                }
            }
            else
            {
                if (DialogueManager.Instance == null) Debug.LogWarning("[DialogueAreaTrigger] DialogueManager Instance not found in scene.");
                if (dialogueSequence == null) Debug.LogWarning($"[DialogueAreaTrigger] No dialogue sequence assigned on {gameObject.name}");
            }
        }
    }

    /// <summary>
    /// Resets the trigger so it can be activated again if triggerOnce is true.
    /// </summary>
    public void ResetTrigger()
    {
        _hasTriggered = false;
    }
}
