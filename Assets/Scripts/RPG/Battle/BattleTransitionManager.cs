using System.Collections;
using UnityEngine;

public class BattleTransitionManager : MonoBehaviour
{
    public static BattleTransitionManager Instance { get; private set; }

    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float playerBattleOffsetX = -2f;
    [SerializeField] private float enemyBattleOffsetX = 2f;
    [SerializeField] private BattleCameraManager battleCamera;

    private Transform _player;
    private Transform _enemy;
    private Vector3 _playerOriginalPos;
    private Vector3 _enemyOriginalPos;
    private bool _isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void PrepareBattle(Transform player, Transform enemy)
    {
        _player = player;
        _enemy = enemy;
        _playerOriginalPos = player.position;
        _enemyOriginalPos = enemy.position;
    }

    public IEnumerator ExecuteBattleEntry()
    {
        if (_player == null || _enemy == null) yield break;

        _isTransitioning = true;
        battleCamera?.SwitchToBattleCam();

        var playerTarget = new Vector3(_enemy.position.x + playerBattleOffsetX, _enemy.position.y, _enemy.position.z);
        var enemyTarget = new Vector3(_player.position.x + enemyBattleOffsetX, _player.position.y, _player.position.z);

        yield return MoveToPositions(playerTarget, enemyTarget);
    }

    public IEnumerator ExecuteBattleExit()
    {
        if (_player == null || _enemy == null) yield break;

        battleCamera?.SwitchToWorldCam();

        yield return MoveToPositions(_playerOriginalPos, _enemyOriginalPos);

        _player = null;
        _enemy = null;
        _isTransitioning = false;
    }

    private IEnumerator MoveToPositions(Vector3 playerTarget, Vector3 enemyTarget)
    {
        var playerStart = _player.position;
        var enemyStart = _enemy.position;
        var elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            var t = elapsed / transitionDuration;
            t = t * t * (3f - 2f * t);

            if (_player != null) _player.position = Vector3.Lerp(playerStart, playerTarget, t);
            if (_enemy != null) _enemy.position = Vector3.Lerp(enemyStart, enemyTarget, t);

            yield return null;
        }

        if (_player != null) _player.position = playerTarget;
        if (_enemy != null) _enemy.position = enemyTarget;
    }

    public bool IsTransitioning => _isTransitioning;
}
