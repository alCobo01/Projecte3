using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the player's child light based on the current scene.
/// </summary>
public class PlayerLightController : MonoBehaviour
{
    [Tooltip("The GameObject representing the light attached to the player.")]
    [SerializeField] private GameObject playerLight;

    [Tooltip("The name of the scene where the light should be active.")]
    [SerializeField] private string targetSceneName = "FinalLevel";

    private void Awake()
    {
        if (playerLight == null)
        {
            var light2D = GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>(true);
            if (light2D != null)
            {
                playerLight = light2D.gameObject;
            }
        }
        UpdateLightState();
    }

    /// <summary>
    /// Checks the current scene and enables/disables the light.
    /// </summary>
    public void UpdateLightState()
    {
        if (playerLight == null)
        {
            Debug.LogWarning("PlayerLightController: No light GameObject assigned.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        playerLight.SetActive(currentScene == targetSceneName);
    }
}
