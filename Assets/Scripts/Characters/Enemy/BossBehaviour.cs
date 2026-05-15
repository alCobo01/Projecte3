using UnityEngine;

public class BossBehaviour : MonoBehaviour
{

    [Header("References")]
    public Animator bossAnimator;
    public GameObject vfxObject;         

    [Header("Settings")]
    public float vfxDuration = 2f;       


    private bool activated = false;

    private void Awake()
    {
        bossAnimator = GetComponent<Animator>();
    }
    private void Start()
    {
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
        if (vfxObject != null)
            StartCoroutine(PlayVFX());
    }

    private System.Collections.IEnumerator PlayVFX()
    {
        vfxObject.SetActive(true);
        yield return new WaitForSeconds(vfxDuration);
        vfxObject.SetActive(false);
    }
}
