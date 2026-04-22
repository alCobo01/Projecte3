using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTransitionManager : MonoBehaviour
{
    public static BattleTransitionManager Instance { get; private set; }

    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Vector2 playerViewportPoint = new(0.3f, 0.35f);
    [SerializeField] private Vector2 enemyViewportPoint = new(0.7f, 0.35f);
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private BattleCameraManager battleCameraManager;
    [SerializeField] private BattleCameraShake battleCameraShake;
    [SerializeField] private List<SpriteRenderer> transitionSprites = new();

    private CanvasGroup _transitionCanvasGroup;
    private Transform _player, _enemy;
    private Rigidbody2D _playerRb, _enemyRb;
    private bool _playerRbSimulated, _enemyRbSimulated;
    private Vector3 _playerOriginalPos, _enemyOriginalPos;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _transitionCanvasGroup = transitionPanel.GetComponent<CanvasGroup>();

        SetPanelAlpha(0f);
        transitionPanel.SetActive(false);
    }

    public void PrepareBattle(Transform player, Transform enemy)
    {
        _player = player;
        _enemy = enemy;
        _playerOriginalPos = _player.position;
        _enemyOriginalPos = _enemy.position;

        _playerRb = _player.GetComponent<Rigidbody2D>();
        _enemyRb = _enemy.GetComponent<Rigidbody2D>();
    }

    public IEnumerator ExecuteBattleEntry()
    {
        SetPhysicsSimulation(false);
        battleCameraShake.TriggerShake();

        battleCameraManager.PositionBattleCamera(_player, _enemy);
        battleCameraManager.GetBattleTargetPositions(_player, _enemy, playerViewportPoint, enemyViewportPoint, out var playerTarget, out var enemyTarget);

        battleCameraManager.SwitchToBattleCam();

        SetPanelAlpha(0f);
        transitionPanel.SetActive(true);
        yield return new WaitWhile(() => battleCameraManager.IsBlending);

        yield return MoveToPositions(playerTarget, enemyTarget, 0f, 1f);
    }

    public IEnumerator ExecuteBattleExit()
    {
        yield return MoveToPositions(_playerOriginalPos, _enemyOriginalPos, 1f, 0f);
        SetPhysicsSimulation(true);

        transitionPanel.SetActive(false);
        battleCameraManager.SwitchToWorldCam();

        _player = null;
        _enemy = null;
        _playerRb = null;
        _enemyRb = null;
    }

    private IEnumerator MoveToPositions(Vector3 playerTarget, Vector3 enemyTarget, float alphaFrom, float alphaTo)
    {
        var playerStart = _player.position;
        var enemyStart = _enemy.position;
        var elapsed = 0f;

        SetPanelAlpha(alphaFrom);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            var t = Smooth01(elapsed / transitionDuration);

            _player.position = Vector3.Lerp(playerStart, playerTarget, t);
            _enemy.position = Vector3.Lerp(enemyStart, enemyTarget, t);
            SetPanelAlpha(Mathf.Lerp(alphaFrom, alphaTo, t));

            yield return null;
        }

        _player.position = playerTarget;
        _enemy.position = enemyTarget;
        SetPanelAlpha(alphaTo);
    }
    
    private void SetPanelAlpha(float value)
    {
        _transitionCanvasGroup.alpha = value;

        foreach (var sprite in transitionSprites)
        {
            var c = sprite.color;
            c.a = value;
            sprite.color = c;
        }
    }

    private void SetPhysicsSimulation(bool isEnabled)
    {
        //Player rigidbody
        if (!isEnabled)
            _playerRbSimulated = _playerRb.simulated;
        else
            _playerRb.simulated = _playerRbSimulated;

        if (!isEnabled)
        {
            _playerRb.simulated = false;
            _playerRb.linearVelocity = Vector2.zero;
            _playerRb.angularVelocity = 0f;
        }
        
        //Enemies rigidbody
        if (!isEnabled) _enemyRbSimulated = _enemyRb.simulated;
        else _enemyRb.simulated = _enemyRbSimulated;

        if (!isEnabled)
        {
            _enemyRb.simulated = false;
            _enemyRb.linearVelocity = Vector2.zero;
            _enemyRb.angularVelocity = 0f;
        }
        
    }

    private static float Smooth01(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    private static Vector3 ViewportToWorld(Camera cam, Vector2 viewport, float targetZ)
    {
        var distance = Mathf.Abs(targetZ - cam.transform.position.z);
        var world = cam.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, distance));
        world.z = targetZ;
        return world;
    }

    private void OnDrawGizmos()
    {
        var cam = Camera.main;
        const float markerRadius = 0.15f;

        var playerZ = Application.isPlaying && _player != null ? _player.position.z : 0f;
        var enemyZ = Application.isPlaying && _enemy != null ? _enemy.position.z : 0f;

        var playerPoint = ViewportToWorld(cam, playerViewportPoint, playerZ);
        var enemyPoint = ViewportToWorld(cam, enemyViewportPoint, enemyZ);

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(playerPoint, markerRadius);
        Gizmos.DrawLine(playerPoint + Vector3.left * markerRadius, playerPoint + Vector3.right * markerRadius);
        Gizmos.DrawLine(playerPoint + Vector3.up * markerRadius, playerPoint + Vector3.down * markerRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(enemyPoint, markerRadius);
        Gizmos.DrawLine(enemyPoint + Vector3.left * markerRadius, enemyPoint + Vector3.right * markerRadius);
        Gizmos.DrawLine(enemyPoint + Vector3.up * markerRadius, enemyPoint + Vector3.down * markerRadius);
    }
}
