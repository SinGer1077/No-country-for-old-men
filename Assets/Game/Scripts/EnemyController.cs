using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Chase,
    Fire,
    Rest
}

public class EnemyController : MonoBehaviour
{
    [SerializeField] public PlayerController2D character;

    [Header("Movement")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField] private Transform visual;
    [SerializeField] private float rotationSpeed = 360f;

    [SerializeField]
    private Transform[] patrolPoints;
    private int currentPointIndex = 0;

    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float fireCooldown = 0.25f;
    [SerializeField, Min(0f)] private float prepareShotTime = 0.5f;
    [SerializeField, Min(0f)] private float restTime = 0.5f;
    [SerializeField, Min(0f)] private float maxShotDistance = 20f;
    [SerializeField, Min(0f)] private float damage;
    [SerializeField] private LayerMask hitLayers = ~0;

    private Vector2 moveInput;
    private float nextShotTime;

    private EnemyState currentState;
    private float stateTimer;

    // chase
    private float chaseUpdateDelta = 0.0f; 

    private void Awake()
    {
        if (character == null)
        {
            character = FindFirstObjectByType<PlayerController2D>();
        }

        agent.updateRotation = false;
        agent.updateUpAxis = false;

        currentState = EnemyState.Patrol;
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: 
                ReadMovement();
                break;

            case EnemyState.Chase:
                ChaseCharacter(); 
                break;

            case EnemyState.Fire:
                HandleShooting();
                break;

            case EnemyState.Rest:
                HandleRest();
                break;

        }
        UpdateRotation();
    }

    private void ReadMovement()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }

    }

    private void ChaseCharacter()
    {
        if (character == null)
            return;

        if (chaseUpdateDelta <= 0.0f)
        {
            agent.SetDestination(character.transform.position);
            chaseUpdateDelta = 1.0f;
        }

        chaseUpdateDelta -= Time.fixedDeltaTime;

        if (Time.time >= nextShotTime)
        {
            currentState = EnemyState.Fire;
            stateTimer = prepareShotTime;

            agent.ResetPath();
        }
    }

    private void UpdateRotation()
    {
        Vector2 direction = agent.desiredVelocity;

        if (direction.sqrMagnitude < 0.01f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x)
                      * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, angle);

        visual.rotation = Quaternion.RotateTowards(
            visual.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void HandleShooting()
    {
        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer > 0f)
            return;

        Shoot();

        currentState = EnemyState.Rest;
        stateTimer = restTime;
    }


    private void Shoot()
    {
        Vector2 origin = firePoint.position;
        Vector2 direction = firePoint.up;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            direction,
            maxShotDistance,
            hitLayers
        );

        if (hit.collider != null)
        {
            Debug.DrawLine(origin, hit.point, Color.red, 0.2f);

            Debug.Log("Shot hit: " + hit.collider.name);

            if (hit.collider.TryGetComponent<HPController>(out HPController hp))
            {
                hp.TakeDamage(damage);
            }
        }
        else
        {
            Debug.DrawRay(
                origin,
                direction * maxShotDistance,
                Color.yellow,
                0.2f
            );
        }

        nextShotTime = Time.time + fireCooldown;
    }

    private void HandleRest()
    {
        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer > 0f)
            return;

        currentState = EnemyState.Chase;
        chaseUpdateDelta = 0f;
    }

    public void StartChase()
    {
        currentState = EnemyState.Chase;

        chaseUpdateDelta = 0f;
        nextShotTime = Time.time + fireCooldown;
    }

    public void ContinuePatrol()
    {
        currentState = EnemyState.Patrol;

        agent.ResetPath();
    }

}
