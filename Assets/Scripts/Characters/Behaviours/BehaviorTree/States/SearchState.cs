using UnityEngine;

[CreateAssetMenu(fileName = "SearchState", menuName = "Scriptable Objects/SearchState")]
public class SearchState : Node
{
    public override bool EnterCondition(EnemyController ec)
    {
        return ec.search.check;
    }

    public override bool ExitCondition(EnemyController ec)
    {
        return !ec.search.check || ec.chase.check || ec.attack.check;
    }

    public override void OnStart(EnemyController ec)
    {
        var anim = ec.GetComponent<CharacterAnimationController>();
        if (anim != null) anim.SetRunning(true);
        Debug.Log("SEARCH START");
    }

    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);

        ChaseBehaviour chase = ec.GetComponent<ChaseBehaviour>();
        if (chase != null)
        {
            float distanceToLastPos = Mathf.Abs(ec.lastKnownPosition.x - ec.transform.position.x);
            
            if (distanceToLastPos > 0.5f)
            {
                // Nos movemos hacia la última posición conocida
                // Usamos la velocidad de caminar para que parezca que busca
                float speed = ec.enemyData.walkingSpeed * 1.2f; 
                
                // Calculamos dirección manualmente para no depender de un Transform
                float directionX = Mathf.Sign(ec.lastKnownPosition.x - ec.transform.position.x);
                
                // Aplicamos movimiento similar al de ChaseBehaviour
                Rigidbody2D rb = ec.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = new Vector2(directionX * speed, rb.linearVelocity.y);
                    
                    // Girar el personaje
                    if (directionX > 0) ec.transform.rotation = Quaternion.Euler(0, 0, 0);
                    else if (directionX < 0) ec.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                
                var anim = ec.GetComponent<CharacterAnimationController>();
                if (anim != null) anim.SetWalking(1f);
            }
            else
            {
                // Hemos llegado, nos quedamos quietos esperando a que el timer de EnemyController termine
                chase.StopChasing();
                var anim = ec.GetComponent<CharacterAnimationController>();
                if (anim != null) anim.SetWalking(0f);
            }
        }
    }

    public override void OnExit(EnemyController ec)
    {
        ChaseBehaviour chase = ec.GetComponent<ChaseBehaviour>();
        if (chase != null) chase.StopChasing();
        Debug.Log("SEARCH EXIT");
    }
}
