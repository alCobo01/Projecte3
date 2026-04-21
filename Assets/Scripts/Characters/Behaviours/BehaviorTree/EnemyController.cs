using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Condition attack;
    public Condition chase;
    public Condition combat;
    public GameObject target;
    public Node root;
    public Node currentState;
    public EnemySO enemyData;
    private CharacterAnimationController _animController;

    private void Awake()
    {
        _animController = GetComponent<CharacterAnimationController>();
        attack = new Condition("Attack");
        chase = new Condition("Chase");
        combat = new Condition("Combat");
        ChangeState();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo reaccionar si el objeto es Player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            chase.check = true;
            target = collision.gameObject;
            if (_animController != null) _animController.SetRunning(true);
            Debug.Log(" Chase = true");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            chase.check = false;
            attack.check = false;
            target = null;
            if (_animController != null) _animController.SetRunning(false);
            Debug.Log("Chase = false");
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            attack.check = (target.transform.position - transform.position).magnitude <= enemyData.attackDistance;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (_animController != null) _animController.TriggerAttack();
            //Aqui ira la funcion para inicar combate :*
            Debug.Log("Enemigo ataca al jugador!");
        }
    }

    public void OnHurt()
    {
        combat.check = true;
        if (_animController != null) _animController.TriggerHurt();
        ChangeState();
    }

    private void Update()
    {
        if (currentState != null)
            currentState.OnUpdate(this);
    }

    public void ChangeState()
    {
        StartCoroutine(WaitToTheEndOfFrame());
    }

    private IEnumerator WaitToTheEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        foreach (var node in root.children)
        {
            if (node.EnterCondition(this))
            {
                if (currentState != null)
                    currentState.OnExit(this);

                currentState = node;
                node.OnStart(this);
                break;
            }
        }
    }
}
