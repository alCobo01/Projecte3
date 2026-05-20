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
        if (anim) anim.SetRunning(true);
        Debug.Log("SEARCH START");
    }

    public override void OnUpdate(EnemyController ec)
    {
        base.OnUpdate(ec);

        var chase = ec.GetComponent<ChaseBehaviour>();
        if (!chase) return;
        
        var distanceToLastPos = Mathf.Abs(ec.lastKnownPosition.x - ec.transform.position.x);
        if (distanceToLastPos > 0.5f)
        {
            // Nos movemos hacia la última posición conocida
            // Usamos la velocidad de caminar para que parezca que busca
            var speed = ec.enemyData.walkingSpeed * 1.2f; 
                
            // Calculamos dirección manualmente para no depender de un Transform
            var directionX = Mathf.Sign(ec.lastKnownPosition.x - ec.transform.position.x);
                
            // Aplicamos movimiento similar al de ChaseBehaviour
            var rb = ec.GetComponent<Rigidbody2D>();
            if (!rb) return;
            
            var edgeDetector = ec.GetComponent<GroundEdgeDetector>();
            if (!edgeDetector || edgeDetector.HasGroundAhead(directionX))
            {
                rb.linearVelocity = new Vector2(directionX * speed, rb.linearVelocity.y);
                ec.transform.rotation = directionX switch
                {
                    // Girar el personaje
                    > 0 => Quaternion.Euler(0, 0, 0),
                    < 0 => Quaternion.Euler(0, 180, 0),
                    _ => ec.transform.rotation
                };

                var anim = ec.GetComponent<CharacterAnimationController>();
                if (anim) anim.SetWalking(1f);
            }
            else
            {
                // Si hay un borde, nos detenemos
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                var anim = ec.GetComponent<CharacterAnimationController>();
                if (anim) anim.SetWalking(0f);
            }
        }
        else
        {
            // Hemos llegado, nos quedamos quietos esperando a que el timer de EnemyController termine
            chase.StopChasing();
            var anim = ec.GetComponent<CharacterAnimationController>();
            if (anim) anim.SetWalking(0f);
        }
    }

    public override void OnExit(EnemyController ec)
    {
        var chase = ec.GetComponent<ChaseBehaviour>();
        if (chase) chase.StopChasing();
        Debug.Log("SEARCH EXIT");
    }
}
