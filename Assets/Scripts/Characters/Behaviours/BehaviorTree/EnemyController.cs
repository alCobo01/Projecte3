using System.Collections;
using System.Linq;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Condition attack, chase, search, battle;
    public GameObject target;
    [HideInInspector] public Vector3 lastKnownPosition;
    public Node root, currentState;
    public EnemySO enemyData;
    public float searchDuration = 4f;
    
    private CharacterAnimationController _animController;
    private float _currentSearchTimer;
    private Coroutine _changeStateCoroutine;

    private void Awake()
    {
        _animController = GetComponent<CharacterAnimationController>();
        attack = new Condition("Attack");
        chase = new Condition("Chase");
        search = new Condition("Search");
        battle = new Condition("Battle");
        ChangeState();
    }

    private void Start()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnBattleEnded += OnBattleEnded;
    }

    private void OnDestroy()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnBattleEnded -= OnBattleEnded;
    }

    private void OnBattleEnded(bool playerWon)
    {
        SetBattleMode(false);
    }

    public void SetBattleMode(bool active)
    {
        battle.check = active;
        ChangeState();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo reaccionar si el objeto es Player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            chase.check = true;
            search.check = false;
            target = collision.gameObject;
            if (_animController != null) _animController.SetRunning(true);
            Debug.Log(" Chase = true");
            ChangeState();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (target) lastKnownPosition = target.transform.position;
        
        chase.check = false;
        attack.check = false;
        search.check = true;
        _currentSearchTimer = searchDuration;
        target = null;
            
        if (_animController) _animController.SetRunning(false);
        Debug.Log("Chase = false, Search = true");
        ChangeState();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && target)
        {
            attack.check = (target.transform.position - transform.position).magnitude <= enemyData.attackDistance;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Player")) return;
        if (_animController) _animController.TriggerAttack();
    }

    public void OnHurt()
    {
        if (_animController) _animController.TriggerHurt();
        SetBattleMode(true);
    }

    private void Update()
    {
        if (search.check)
        {
            _currentSearchTimer -= Time.deltaTime;
            if (_currentSearchTimer <= 0)
            {
                search.check = false;
                ChangeState();
            }
        }

        if (currentState) currentState.OnUpdate(this);
    }

    public void ChangeState()
    {
        if (_changeStateCoroutine != null) StopCoroutine(_changeStateCoroutine);
        _changeStateCoroutine = StartCoroutine(WaitToTheEndOfFrame());
    }

    private IEnumerator WaitToTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();
        foreach (var node in root.children.Where(node => node.EnterCondition(this)))
        {
            if (currentState == node) break;

            if (currentState) currentState.OnExit(this);
            currentState = node;
            node.OnStart(this);
            break;
        }
        _changeStateCoroutine = null;
    }
}
