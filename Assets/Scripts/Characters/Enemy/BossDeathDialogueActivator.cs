using UnityEngine;

public class BossDeathDialogueActivator : MonoBehaviour
{
    [SerializeField] private GameObject target;

    private void OnEnable() => BossBehaviour.OnBossDied += HandleBossDied;
    private void OnDisable() => BossBehaviour.OnBossDied -= HandleBossDied;

    private void HandleBossDied()
    {
        if (target != null) target.SetActive(true);
    }
}
