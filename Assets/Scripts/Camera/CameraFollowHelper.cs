using UnityEngine;
using Unity.Cinemachine;

public static class CameraFollowHelper
{
    public static void AssignPlayerToCamera(GameObject player, MonoBehaviour owner)
    {
        owner.StartCoroutine(AssignPlayerDelayed(player));
    }

    private static System.Collections.IEnumerator AssignPlayerDelayed(GameObject player)
    {
        yield return new WaitForEndOfFrame();
        
        // Buscamos todas las cámaras de tipo CinemachineCamera
        var vcams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        CinemachineCamera targetVcam = null;

        foreach (var vcam in vcams)
        {
            // Buscamos específicamente la ExplorationCamera
            if (vcam.name == "ExplorationCamera") 
            {
                targetVcam = vcam;
                break;
            }
        }
        
        if (targetVcam != null)
        {
            Debug.Log($"CameraFollowHelper: Asignando Follow a {player.name} en {targetVcam.name}.");
            targetVcam.Follow = player.transform;
        }
        else
        {
            Debug.LogWarning("CameraFollowHelper: NO se encontró la 'ExplorationCamera'.");
            
            // Debugging extra
            var allCameras = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var cam in allCameras) {
                if (cam.GetType().Name.Contains("Cinemachine")) {
                    Debug.Log($"CameraFollowHelper: Encontré algo relacionado con Cinemachine: {cam.name} de tipo {cam.GetType().Name}");
                }
            }
        }
    }
}