using System.Collections;
using UnityEngine;

public class BossBehaviour : MonoBehaviour
{
    public static event System.Action OnBossDied;

    [Header("References")]
    [SerializeField] private GameObject mainVfxObject;
    [SerializeField] private GameObject vfxObject;

    [Header("Settings")]
    [SerializeField] private float vfxDuration = 4f;       
    
    private CameraShake cameraShake;
    private BattleStarter bossBattleStarter;
    private PlayerInputController playerInput;
    private PlayerMovementController playerMovement;
    private BattleStarter playerBattleStarter;
    private Animator bossAnimator;
    private bool _activated;
    private bool _deathNotified;

    private void Awake()
    {
        bossAnimator = GetComponent<Animator>();
        cameraShake = GetComponent<CameraShake>();
        bossBattleStarter = GetComponent<BattleStarter>();
    }
    
    private void Start()
    {
        mainVfxObject.SetActive(false);
        vfxObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activated) return;
        if (!other.gameObject.CompareTag("Player") &&
            other.GetComponentInParent<PlayerInputController>() == null) return;
        
        playerInput = other.GetComponentInParent<PlayerInputController>();
        playerMovement = other.GetComponentInParent<PlayerMovementController>();
        playerBattleStarter = other.GetComponentInParent<BattleStarter>();
            
        _activated = true;
        ActivateBoss();
    }

    private void ActivateBoss()
    {
        bossAnimator.SetTrigger("Awake");

        // Disable movement 
        playerMovement.CanMove = false;
        playerInput.enabled = false;
        
        StartCoroutine(PlayVFX());
        cameraShake.TriggerShake();
    }

    private IEnumerator PlayVFX()
    {
        // Delay before starting VFX sequence
        yield return new WaitForSeconds(2.2f);
        mainVfxObject.SetActive(true);
        SoundManager.Instance.PlaySfx("Scream", transform);
        
        yield return new WaitForSeconds(0.3f);
        vfxObject.SetActive(true);
        
        yield return new WaitForSeconds(vfxDuration);
        
        mainVfxObject.SetActive(false);
        vfxObject.SetActive(false);

        // Start combat via BattleStarter to reuse the validated/common flow.
        bossBattleStarter.TryStartBattleWithTarget(playerBattleStarter);
        
        playerMovement.CanMove = true;
        playerInput.enabled = true;
    }

    public void NotifyBossDied()
    {
        if (_deathNotified) return;
        _deathNotified = true;
        OnBossDied?.Invoke();
    }
}
