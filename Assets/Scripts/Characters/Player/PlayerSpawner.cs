using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private void Start()
    {
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
            CameraFollowHelper.AssignPlayerToCamera(player, this);
        }
    }
}
