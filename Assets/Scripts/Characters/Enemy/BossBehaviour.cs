using System.Collections;
using UnityEngine;

public class BossBehaviour : MonoBehaviour
{
    [Header("References")]
    private Animator bossAnimator;
    public GameObject mainVfxObject;
    public GameObject vfxObject;
    private CameraShake cameraShake;
    private BattleStarter bossBattleStarter;
    private PlayerInputController playerInput;
    private PlayerMovementController playerMovement;
    private BattleStarter playerBattleStarter;

    [Header("Settings")]
    public float vfxDuration = 4f;       
    
    private bool _activated;

    private void Awake()
    {
        bossAnimator = GetComponent<Animator>();
        cameraShake = GetComponent<CameraShake>();
        bossBattleStarter = GetComponent<BattleStarter>();
    }
    private void Start()
    {
        if (mainVfxObject != null)
            mainVfxObject.SetActive(false);
        if (vfxObject != null)
            vfxObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activated) return;

        if (other.gameObject.CompareTag("Player") || other.gameObject.layer == 3)
        {
            playerInput = other.GetComponent<PlayerInputController>();
            playerMovement = other.GetComponent<PlayerMovementController>();
            playerBattleStarter = other.GetComponent<BattleStarter>();
            _activated = true;
            ActivateBoss();
        }
    }

    private void ActivateBoss()
    {
        bossAnimator.SetTrigger("Awake");

        // Disable movement 
        if (playerMovement != null)
            playerMovement.CanMove = false;

        if (playerInput != null)
            playerInput.enabled = false;

        if (mainVfxObject != null || vfxObject != null)
            StartCoroutine(PlayVFX());

        cameraShake.TriggerShake();
    }

    private IEnumerator PlayVFX()
    {
        // Delay before starting VFX sequence
        yield return new WaitForSeconds(2.2f);

        if (mainVfxObject != null)
            mainVfxObject.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        if (vfxObject != null)
            vfxObject.SetActive(true);

        yield return new WaitForSeconds(vfxDuration);

        if (mainVfxObject != null)
            mainVfxObject.SetActive(false);
        if (vfxObject != null)
            vfxObject.SetActive(false);

        // Start combat
        if (bossBattleStarter != null && playerBattleStarter != null)
        {
            var playerData = playerBattleStarter.BattleParty[0];
            var enemies = bossBattleStarter.BattleParty;
            
            var playerAnim = playerBattleStarter.GetComponentInChildren<CharacterAnimationController>();
            var enemyAnim = bossBattleStarter.GetComponentInChildren<CharacterAnimationController>();
            
            var playerTransform = playerBattleStarter.transform.root;
            var enemyTransform = bossBattleStarter.transform.root;

            BattleTransitionManager.Instance?.PrepareBattle(playerTransform, enemyTransform);
            BattleManager.Instance.StartBattle(playerData, enemies, BattleInitiator.Enemy, playerAnim, enemyAnim, playerTransform, enemyTransform);
        }

        if (playerMovement != null)
            playerMovement.CanMove = true;

        if (playerInput != null)
            playerInput.enabled = true;
    }
}
