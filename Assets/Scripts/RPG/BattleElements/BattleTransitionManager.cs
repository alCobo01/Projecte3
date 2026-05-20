using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTransitionManager : MonoBehaviour
{
    public static BattleTransitionManager Instance { get; private set; }
    
    [Header("References")]
    [SerializeField] private GameObject transitionPanel;
    [SerializeField] private BattleCameraManager battleCameraManager;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private List<SpriteRenderer> transitionSprites = new();
    
    [Header("Config values")]
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private float enemyDefeatFlickerDuration = 0.6f;
    [SerializeField] private float enemyFleeFlickerDuration = 1.2f;
    [SerializeField] private float enemyDefeatFlickerInterval = 0.08f;
    [SerializeField] private int regularBattleMusicIndex = 3;
    [SerializeField] private int bossBattleMusicIndex = 4;
    [SerializeField] private Vector2 playerViewportPoint;
    [SerializeField] private Vector2 enemyViewportPoint;
    
    private CanvasGroup _transitionCanvasGroup;
    private Transform _player, _enemy;
    private Rigidbody2D _playerRb, _enemyRb;
    private PlayerAttackController _playerAttackController;
    private Vector3 _playerOriginalPos, _enemyOriginalPos;
    private bool _playerRbSimulated, _enemyRbSimulated, _enemyFlickerDone;
    
    private void Awake()
    {
        if (Instance && Instance != this)
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
        _playerAttackController = _player.GetComponentInChildren<PlayerAttackController>();
    }

    public IEnumerator ExecuteBattleEntry()
    {
        var entryMusicIndex = IsBossBattle() ? bossBattleMusicIndex : regularBattleMusicIndex;
        SoundManager.Instance.PlayMusicByIndex(entryMusicIndex);

        SetPlayerAttackEnabled(false);
        SetPhysicsSimulation(false);
        SetBattleOrientation();
        cameraShake.TriggerShake();

        battleCameraManager.PositionBattleCamera(_player, _enemy);
        battleCameraManager.GetBattleTargetPositions(_player, _enemy, playerViewportPoint, enemyViewportPoint, out var playerTarget, out var enemyTarget);
        battleCameraManager.SwitchToBattleCam();

        SetPanelAlpha(0f);
        transitionPanel.SetActive(true);

        SetSortingLayer(_player.gameObject, "TemporalCharacters");
        SetSortingLayer(_enemy.gameObject, "TemporalCharacters");
        
        yield return new WaitWhile(() => battleCameraManager.IsBlending);
        yield return MoveToPositions(playerTarget, enemyTarget, 0f, 1f);
        SetBattleOrientation();
    }

    private void SetBattleOrientation()
    {
        var playerFlip = _player.GetComponentInChildren<CharacterFlipBehaviour>(true);
        var enemyFlip = _enemy.GetComponentInChildren<CharacterFlipBehaviour>(true);
        playerFlip.ForceOrientation(true);
        enemyFlip.ForceOrientation(IsBossBattle());
    }

    public IEnumerator ExecuteBattleExit(bool playerWon)
    {
        _enemyFlickerDone = true;
        if (playerWon)
        {
            _enemyFlickerDone = false;
            StartCoroutine(FlickerAndHideEnemy(enemyDefeatFlickerDuration, hideAtEnd: true));
        }
        else if (_enemy)
        {
            _enemyFlickerDone = false;
            StartCoroutine(FlickerAndHideEnemy(enemyFleeFlickerDuration, hideAtEnd: false));
            StartCoroutine(DisableEnemyBattleStarterDuringFlee());
            StartCoroutine(DisableEnemyAttacksDuringFlee());
        }

        yield return MoveToPositions(_playerOriginalPos, _enemyOriginalPos, 1f, 0f);
        SetPhysicsSimulation(true);

        SetSortingLayer(_player.gameObject, "Characters");
        SetSortingLayer(_enemy.gameObject, "Characters");
        
        transitionPanel.SetActive(false);
        battleCameraManager.SwitchToWorldCam();
        SoundManager.Instance?.PlayMusicByIndex(1);
        SetPlayerAttackEnabled(true);

        if (playerWon && !_enemyFlickerDone)
            yield return new WaitUntil(() => _enemyFlickerDone);

        if (playerWon) Destroy(_enemy.gameObject);

        _player = null;
        _enemy = null;
        _playerRb = null;
        _enemyRb = null;
        _playerAttackController = null;
    }

    private void SetPlayerAttackEnabled(bool isEnabled) => _playerAttackController.enabled = isEnabled;

    private IEnumerator FlickerAndHideEnemy(float duration, bool hideAtEnd)
    {
        if (_enemy is null) yield break;

        var renderers = _enemy.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0) yield break;

        var elapsed = 0f;
        var visible = true;

        while (elapsed < duration)
        {
            visible = !visible;
            SetRenderersVisible(renderers, visible);
            yield return new WaitForSeconds(enemyDefeatFlickerInterval);
            elapsed += enemyDefeatFlickerInterval;
        }

        SetRenderersVisible(renderers, !hideAtEnd);
        _enemyFlickerDone = true;
    }

    private IEnumerator DisableEnemyBattleStarterDuringFlee()
    {
        var starters = _enemy.GetComponentsInChildren<BattleStarter>(true);
        SetBattleStartersEnabled(starters, false);
        yield return new WaitUntil(() => _enemyFlickerDone);
        SetBattleStartersEnabled(starters, true);
    }

    private IEnumerator DisableEnemyAttacksDuringFlee()
    {
        var attacks = _enemy.GetComponentsInChildren<AttackBehaviour>(true);
        SetAttackBehavioursEnabled(attacks, false);
        yield return new WaitUntil(() => _enemyFlickerDone);
        SetAttackBehavioursEnabled(attacks, true);
    }

    private static void SetRenderersVisible(SpriteRenderer[] renderers, bool isVisible)
    {
        foreach (var renderer in renderers) renderer.enabled = isVisible;
    }

    private static void SetBattleStartersEnabled(BattleStarter[] starters, bool isEnabled)
    {
        foreach (var starter in starters) starter.enabled = isEnabled;
    }

    private static void SetAttackBehavioursEnabled(AttackBehaviour[] attacks, bool isEnabled)
    {
        foreach (var attack in attacks) attack.enabled = isEnabled;
    }
    
    private IEnumerator MoveToPositions(Vector3 playerTarget, Vector3 enemyTarget, float alphaFrom, float alphaTo)
    {
        var playerStart = _player.position;
        var enemyStart = _enemy ? _enemy.position : enemyTarget;
        var elapsed = 0f;

        SetPanelAlpha(alphaFrom);

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            var t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

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
        if (!isEnabled)
        {
            _playerRbSimulated = _playerRb.simulated;
            _playerRb.simulated = false;
            _playerRb.linearVelocity = Vector2.zero;
            _playerRb.angularVelocity = 0f;
            
            _enemyRbSimulated = _enemyRb.simulated;
            _enemyRb.simulated = false;
            _enemyRb.linearVelocity = Vector2.zero;
            _enemyRb.angularVelocity = 0f;
        }
        else
        {
            _playerRb.simulated = _playerRbSimulated;
            _enemyRb.simulated = _enemyRbSimulated;
        }
    }
    
    private static void SetSortingLayer(GameObject go, string layerName)
    {
        var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);
         
        foreach (var sr in renderers)
            sr.sortingLayerName = layerName;
    }

    private static Vector3 ViewportToWorld(Camera cam, Vector2 viewport, float targetZ)
    {
        var distance = Mathf.Abs(targetZ - cam.transform.position.z);
        var world = cam.ViewportToWorldPoint(new Vector3(viewport.x, viewport.y, distance));
        world.z = targetZ;
        return world;
    }

    private bool IsBossBattle()
    {
        if (!_enemy) return false;
        return _enemy.GetComponentInChildren<BossBehaviour>(true);
    }
    
    private void OnDrawGizmosSelected()
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
