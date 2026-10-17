using UnityEngine;

[DisallowMultipleComponent]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;

    [Header("Aiming")]
    [SerializeField] private Transform aimPivot;

    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float fireCooldown = 0.25f;
    [SerializeField, Min(0f)] private float maxShotDistance = 20f;
    [SerializeField] private LayerMask hitLayers = ~0;

    private Camera mainCamera;
    private Vector2 moveInput;
    private float nextShotTime;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (aimPivot == null)
            aimPivot = transform;
    }

    private void Update()
    {
        ReadMovement();
        AimAtMouse();
        HandleShooting();
    }

    private void ReadMovement()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void FixedUpdate()
    {
        // Transform movement is simple for a jam prototype.
        // If you use Rigidbody2D physics, switch this to Rigidbody2D.MovePosition.
        transform.position += (Vector3)(moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void AimAtMouse()
    {
        if (mainCamera == null)
            return;

        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (Vector2)(mouseWorld - aimPivot.position);

        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void HandleShooting()
    {
        if (!Input.GetMouseButtonDown(0) || Time.time < nextShotTime)
            return;

        nextShotTime = Time.time + fireCooldown;

        Vector2 origin = firePoint.position;
        Vector2 direction = firePoint.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxShotDistance, hitLayers);

        if (hit.collider != null)
        {
            Debug.DrawLine(origin, hit.point, Color.red, 0.2f);
            Debug.Log("Shot hit: " + hit.collider.name);
        }
        else
        {
            Debug.DrawRay(origin, direction * maxShotDistance, Color.yellow, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (firePoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(firePoint.position, 0.04f);
        Gizmos.DrawLine(
            firePoint.position,
            firePoint.position + firePoint.right * maxShotDistance
        );
    }
}
