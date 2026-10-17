using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    private EnemyMovingAlgorithm movingAlgorithm;


    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float fireCooldown = 0.25f;
    [SerializeField, Min(0f)] private float maxShotDistance = 20f;

    private Vector2 moveInput;
    private float nextShotTime;

    private void Awake()
    {
        movingAlgorithm = new EnemyMovingAlgorithm();
    }

    private void FixedUpdate()
    {
        ReadMovement();
    }

    private void ReadMovement()
    {
        //todo realize some algorithm for patroling here
        moveInput = movingAlgorithm.SimpleMovement(Time.time).normalized;
        transform.position += (Vector3)(moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}
