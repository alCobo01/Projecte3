using Unity.Cinemachine;
using UnityEngine;

public class BattleCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineBrain worldCamera;
    [SerializeField] private CinemachineCamera battleCamera;

    private bool _isBattleCamActive;
    
    public void SwitchToBattleCam()
    {
        if (_isBattleCamActive) return;
        _isBattleCamActive = true;
        
        if (worldCamera != null) worldCamera.enabled = false;
        if (battleCamera != null) battleCamera.gameObject.SetActive(true);
    }

    public void SwitchToWorldCam()
    {
        if (!_isBattleCamActive) return;
        _isBattleCamActive = false;
        
        if (battleCamera != null) battleCamera.gameObject.SetActive(false);
        if (worldCamera != null) worldCamera.enabled = true;
    }
}