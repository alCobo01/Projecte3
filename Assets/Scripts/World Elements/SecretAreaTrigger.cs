using UnityEngine;

namespace WorldElements
{
    /// <summary>
    /// Controls the visibility of a Tilemap layer. 
    /// Includes a cooldown and state checks to prevent "double triggering" and flickering.
    /// </summary>
    public class SecretAreaTrigger : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Tilemap or GameObject to hide.")]
        [SerializeField] private GameObject secretTilemap;

        [Header("Settings")]
        [Tooltip("Cooldown in seconds to prevent rapid toggling.")]
        [SerializeField] private float toggleCooldown = 0.5f;
        
        private float _lastToggleTime;
        private bool _hasPlayedFirstEnterSfx;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                TryPlayFirstEnterSfx();
                TrySetTilemapState(false);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                // We add a small delay or check to ensure we really want to re-enable it
                TrySetTilemapState(true);
            }
        }

        private void TrySetTilemapState(bool active)
        {
            if (secretTilemap == null) return;

            // Check if enough time has passed since the last change
            if (Time.time < _lastToggleTime + toggleCooldown) return;

            // Only change if the state is actually different
            if (secretTilemap.activeSelf != active)
            {
                secretTilemap.SetActive(active);
                _lastToggleTime = Time.time;
            }
        }

        private void TryPlayFirstEnterSfx()
        {
            if (_hasPlayedFirstEnterSfx) return;

            SoundManager.Instance.PlaySfx("Secret", transform);
            _hasPlayedFirstEnterSfx = true;
        }
    }
}
