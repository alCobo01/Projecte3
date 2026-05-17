using UnityEngine;
using Unity.Cinemachine;

public static class CameraFollowHelper
{
    public static void AssignPlayerToCamera(GameObject player)
    {
        CinemachineCamera vcam = Object.FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = player.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró ninguna CinemachineCamera en la escena.");
        }
    }
}