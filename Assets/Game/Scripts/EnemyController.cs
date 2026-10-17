using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
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
    [SerializeField, Min(0f)] private float maxShotDistance = 20f;
    [SerializeField, Min(0f)] private float damage;

    private Vector2 moveInput;
    private float nextShotTime;

    private void Awake()
    {
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void FixedUpdate()
    {
        ReadMovement();
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
}
