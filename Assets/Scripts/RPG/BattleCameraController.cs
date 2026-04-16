using Unity.Cinemachine;
using UnityEngine;

public class BattleCameraManager : MonoBehaviour
{
    public static BattleCameraManager Instance { get; private set; }
    
    [SerializeField] private CinemachineCamera battleCamera;
    [SerializeField] private GameObject player;

    private PlayerMovementController _movementController;
    
    private void Awake()
    {
        if (Instance is null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    public void Start()
    {
        _movementController = player.GetComponent<PlayerMovementController>();
    }
    
    public void TogglePlayerMovement(bool value)
    {
        _movementController.CanMove = value;
    }
}