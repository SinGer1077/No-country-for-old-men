using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Chase,
    Fire,
    Rest,
    Check
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
    [SerializeField] private ParticleSystem prepareShotVFX;
    [SerializeField] private Transform visualShotPoint;

    [Header("Check / Investigation")]
    [SerializeField, Min(0.1f)] private float checkRadius = 5f;
    [SerializeField, Min(0f)] private float checkLookDuration = 3f;
    [SerializeField, Min(0.1f)] private float checkPointSampleDistance = 1.5f;

    [SerializeField]
    private SpriteRenderer questionSprite;

    private Vector3 checkTarget;
    private bool isLookingAround;


    [SerializeField] private LineRenderer shotTrace;
    private Color startColor;
    private Color endColor;
    private Coroutine routine;
    private float traceDuration = 0.2f;

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

        startColor = shotTrace.startColor;
        endColor = shotTrace.endColor;
        shotTrace.positionCount = 2;
        shotTrace.useWorldSpace = true;
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

            case EnemyState.Check:
                HandleCheck();
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

        if (!prepareShotVFX.isPlaying)
        {
            prepareShotVFX.Play();
        }

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
            shotTrace.SetPosition(0, visualShotPoint.position);
            shotTrace.SetPosition(1, hit.point);

        }
        else
        {
            Debug.DrawRay(
                origin,
                direction * maxShotDistance,
                Color.yellow,
                0.2f
            );

            shotTrace.SetPosition(0, visualShotPoint.position);
            shotTrace.SetPosition(1, direction * maxShotDistance);
        }

        if (routine != null)
            StopCoroutine(routine);

        SetAlpha(1f);
        shotTrace.enabled = true;
        shotTrace.startColor = startColor;
        shotTrace.endColor = endColor;

        routine = StartCoroutine(FadeOut());

        nextShotTime = Time.time + fireCooldown;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < traceDuration)
        {
            float t = elapsed / traceDuration;

            SetAlpha(1f - t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0f);
        shotTrace.enabled = false;
        routine = null;
    }

    private void SetAlpha(float alpha)
    {
        Color s = startColor;
        Color e = endColor;

        s.a *= alpha;
        e.a *= alpha;

        shotTrace.startColor = s;
        shotTrace.endColor = e;
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

        isLookingAround = false;
        agent.isStopped = false;

        chaseUpdateDelta = 0f;
        nextShotTime = Time.time + fireCooldown;
    }

    public void ContinuePatrol()
    {
        currentState = EnemyState.Patrol;
        isLookingAround = false;

        agent.isStopped = false;
        agent.ResetPath();

        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        agent.SetDestination(
            patrolPoints[currentPointIndex].position
        );
    }

     public void CheckState(Vector3 pointToCheck)
     {
        if (currentState == EnemyState.Chase ||
            currentState == EnemyState.Fire ||
            currentState == EnemyState.Rest)
        {
            return;
        }

        if (!TryGetRandomNavMeshPoint(
                pointToCheck,
                checkRadius,
                out checkTarget))
        {
            // Если подходящую точку найти не удалось,
            // попробуем дойти непосредственно до шума.
            if (!NavMesh.SamplePosition(
                    pointToCheck,
                    out NavMeshHit hit,
                    checkPointSampleDistance,
                    agent.areaMask))
            {
                return;
            }

            checkTarget = hit.position;
        }

        currentState = EnemyState.Check;
        isLookingAround = false;

        agent.isStopped = false;
        agent.SetDestination(checkTarget);
     }

    private bool TryGetRandomNavMeshPoint(Vector3 center, float radius, out Vector3 result)
    {
        result = Vector3.zero;

        const int attempts = 30;

        for (int i = 0; i < attempts; i++)
        {
            Vector2 offset = Random.insideUnitCircle * radius;

            Vector3 candidate = center + new Vector3(
                offset.x,
                offset.y,
                0f
            );

            if (NavMesh.SamplePosition(
                    candidate,
                    out NavMeshHit hit,
                    checkPointSampleDistance,
                    agent.areaMask))
            {
                result = hit.position;
                return true;
            }
        }

        return false;
    }

    private void HandleCheck()
    {
        questionSprite.enabled = true;
        if (!isLookingAround)
        {
            if (agent.pathPending)
                return;

            if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                ContinuePatrol();
                return;
            }

            if (agent.remainingDistance >
                agent.stoppingDistance + 0.05f)
            {
                return;
            }

            agent.ResetPath();
            agent.isStopped = true;

            isLookingAround = true;
            stateTimer = checkLookDuration;
        }

        visual.Rotate(
            0f,
            0f,
            rotationSpeed * Time.fixedDeltaTime
        );

        stateTimer -= Time.fixedDeltaTime;

        if (stateTimer <= 0f)
        {
            questionSprite.enabled = false;
            ContinuePatrol();
        }
    }

}
