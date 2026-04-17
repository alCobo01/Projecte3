using UnityEngine;

public class BattleCameraController : MonoBehaviour
{
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Camera battleCamera;

    private void Awake()
    {
        if (battleCamera != null) battleCamera.enabled = false;
    }

    public void SwitchToBattleCam()
    {
        if (worldCamera != null) worldCamera.enabled = false;
        if (battleCamera != null) battleCamera.enabled = true;
    }

    public void SwitchToWorldCam()
    {
        if (battleCamera != null) battleCamera.enabled = false;
        if (worldCamera != null) worldCamera.enabled = true;
    }
}
