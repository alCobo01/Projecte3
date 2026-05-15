using UnityEngine;

public class BossBehaviour : MonoBehaviour
{

    [Header("References")]
    private Animator bossAnimator;
    public GameObject mainVfxObject;
    public GameObject vfxObject;
    private CameraShake cameraShake;

    [Header("Settings")]
    public float vfxDuration = 4f;       


    private bool activated = false;

    private void Awake()
    {
        bossAnimator = GetComponent<Animator>();
        cameraShake=GetComponent<CameraShake>();
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
        if (activated) return;

        if (other.gameObject.layer == 3)
        {
            activated = true;
            ActivateBoss();
        }
    }

    private void ActivateBoss()
    {
        bossAnimator.SetTrigger("Awake");

        if (mainVfxObject != null)
            StartCoroutine(PlayVFX());

        cameraShake.TriggerShake();
    }

    private System.Collections.IEnumerator PlayVFX()
    {
        yield return new WaitForSeconds(2);

        vfxObject.SetActive(true);
        yield return new WaitForSeconds(vfxDuration);
        vfxObject.SetActive(false);
    }
}
