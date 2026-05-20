using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A trigger that unlocks a player skill when entered.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SkillUnlockTrigger : MonoBehaviour
{
    public enum SkillType { Dash, DoubleJump, Teleport, CombatSkill, Other }

    [Header("Settings")]
    [SerializeField] private SkillType skillToUnlock = SkillType.Dash;
    
    [Tooltip("Only used if Skill Type is CombatSkill")]
    [SerializeField] private SkillData combatSkillData;

    [SerializeField] private bool destroyOnUnlock = true;
    
    [Header("Events")]
    public UnityEvent OnUnlockEvent;

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
        if (other.CompareTag("Player"))
        {
            UnlockSkill(other.gameObject);
        }
    }

    private void UnlockSkill(GameObject player)
    {
        switch (skillToUnlock)
        {
            case SkillType.Dash:
                if (PlayerStatsManager.Instance != null)
                {
                    PlayerStatsManager.Instance.dashUnlocked = true;
                    Debug.Log("[SkillUnlockTrigger] Dash Unlocked!");
                }
                break;
            
            case SkillType.Teleport:
                if (CheckpointManager.Instance != null)
                {
                    CheckpointManager.Instance.TeleportUnlocked = true;
                    Debug.Log("[SkillUnlockTrigger] Teleportation Unlocked!");
                }
                break;

            case SkillType.CombatSkill:
                if (combatSkillData != null && PlayerStatsManager.Instance != null)
                {
                    PlayerStatsManager.Instance.AddSkill(combatSkillData);
                    Debug.Log($"[SkillUnlockTrigger] Combat Skill '{combatSkillData.skillName}' Unlocked!");
                }
                break;
            
            // Add other skills here if needed
        }

        OnUnlockEvent?.Invoke();

        if (destroyOnUnlock)
        {
            Destroy(gameObject);
        }
    }
}
