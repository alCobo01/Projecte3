using Unity.Cinemachine;
using UnityEngine;

public class BattleCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineCamera battleCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private Vector3 battleCameraOffset = new(0f, 2f, -15f);
    [SerializeField] private float blendDuration = 1f;

    private bool _isBattleCamActive;

    public bool IsBlending => cinemachineBrain.IsBlending;

    public void SwitchToBattleCam()
    {
        if (_isBattleCamActive) return;
        _isBattleCamActive = true;
        cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, blendDuration);
        battleCamera.gameObject.SetActive(true);
        battleCamera.Priority = 20;
    }

    public void SwitchToWorldCam()
    {
        if (!_isBattleCamActive) return;
        _isBattleCamActive = false;
        battleCamera.Priority = 0;
        battleCamera.gameObject.SetActive(false);
    }

    public void PositionBattleCamera(Transform player, Transform enemy)
    {
        var center = (player.position + enemy.position) * 0.5f;
        battleCamera.transform.position = center + battleCameraOffset;
    }

    public void GetBattleTargetPositions(Transform player, Transform enemy, Vector2 playerViewport, 
        Vector2 enemyViewport, out Vector3 playerTarget, out Vector3 enemyTarget)
    {
        var cam = Camera.main;
        var center = (player.position + enemy.position) * 0.5f;
        var battleCamPos = center + battleCameraOffset;

        playerTarget = ViewportToWorldAtPosition(cam, battleCamPos, playerViewport, player.position.z);
        enemyTarget = ViewportToWorldAtPosition(cam, battleCamPos, enemyViewport, enemy.position.z);
    }

    private static Vector3 ViewportToWorldAtPosition(Camera cam, Vector3 camPos, Vector2 viewport, float targetZ)
    {
        var distance = Mathf.Abs(targetZ - camPos.z);
        var ray = cam.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
        var origin = ray.origin + (camPos - cam.transform.position);
        var world = origin + ray.direction * distance;
        world.z = targetZ;
        return world;
    }
}