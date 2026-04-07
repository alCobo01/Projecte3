using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public Condition attack;
    public Condition chase;
    public Condition combat;
    public Condition run;
    public GameObject target;
    public float AttackDistance;
    public int HP = 5;
    public Node root;
    public Node currentState;

    public int AttackDamage = 1;

    private void Awake()
    {
        attack = new Condition("Attack");
        chase = new Condition("Chase");
        combat = new Condition("Combat");
        run = new Condition("Run");

        AttackDistance = GetComponent<CapsuleCollider>().radius;
        ChangeState();
    }

    private void OnTriggerEnter(Collider collision)
    {
        // Solo reaccionar si el objeto es Player
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            chase.check = true;
            target = collision.gameObject;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            chase.check = false;
            target = null;
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            attack.check = (target.transform.position - transform.position).magnitude <= AttackDistance;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(AttackDamage);
                Debug.Log("Enemigo atacó al jugador!");
            }
        }
    }

    public void OnHurt()
    {
        combat.check = true;
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
