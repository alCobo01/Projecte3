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
        
        var vcams = Object.FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
        CinemachineCamera targetVcam = null;

        foreach (var vcam in vcams)
        {
            if (vcam.name == "ExplorationCamera") 
            {
                targetVcam = vcam;
                break;
            }
        }
        
        if (targetVcam != null)
        {
            targetVcam.Follow = player.transform;
            targetVcam.OnTargetObjectWarped(player.transform, player.transform.position - targetVcam.transform.position);
        }
    }
}