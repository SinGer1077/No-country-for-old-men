
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D rb;

    [Header("Aiming")]
    [SerializeField] private Transform aimPivot;

    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0f)] private float fireCooldown = 0.25f;
    [SerializeField, Min(0f)] private float maxShotDistance = 20f;
    [SerializeField] private LayerMask hitLayers = ~0;
    [SerializeField, Min(0f)] private float damage;

    [SerializeField] private SpriteRenderer Impact;
    [SerializeField] private ParticleSystem ImpactVFX;
    [SerializeField] private AudioSource ImpactSound;
    [SerializeField] private ParticleSystem Trace;
    [SerializeField] private ParticleSystem hitVfx;

    [Header("Reload")]
    [SerializeField, Min(1)] private int magazineSize = 6;
    [SerializeField, Min(0.1f)] private float reloadDuration = 2f;
    [SerializeField] private Image reloadProgressImage;
    [SerializeField] private BulletController bulletController;
    [SerializeField] private TextMeshProUGUI reloadHelpText;

    private int ammoInMagazine;
    private bool isReloading;
    private float reloadTimer;

    // Public properties for UI.
    public int AmmoInMagazine => ammoInMagazine;
    public int MagazineSize => magazineSize;
    public bool IsReloading => isReloading;

    // Reload progress from 0 to 1.
    public float ReloadProgress =>
        isReloading
            ? Mathf.Clamp01(1f - reloadTimer / reloadDuration)
            : 0f;

    // Seconds remaining until reload completes.
    public float ReloadTimeRemaining =>
        isReloading ? Mathf.Max(0f, reloadTimer) : 0f;

    private Camera mainCamera;
    private Vector2 moveInput;
    private float nextShotTime;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (aimPivot == null)
            aimPivot = transform;

        ammoInMagazine = magazineSize;
    }

    private void Update()
    {
        ReadMovement();
        AimAtMouse();
        UpdateReload();
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
        if (rb != null)
            rb.linearVelocity = moveInput * moveSpeed;
    }

    private void AimAtMouse()
    {
        if (mainCamera == null || aimPivot == null)
            return;

        Vector3 mouseWorld =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction =
            (Vector2)(mouseWorld - aimPivot.position);

        if (direction.sqrMagnitude < 0.0001f)
            return;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        aimPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdateReload()
    {
        // Start reloading when R is pressed.
        if (Input.GetKeyDown(KeyCode.R) &&
            !isReloading &&
            ammoInMagazine < magazineSize)
        {
            isReloading = true;
            reloadTimer = reloadDuration;

            reloadProgressImage.gameObject.SetActive(true);
            reloadProgressImage.fillAmount = 1f;

            reloadHelpText.gameObject.SetActive(false);
        }

        if (!isReloading && ammoInMagazine == 0)
        {
            reloadHelpText.gameObject.SetActive(true);
        }

        if (!isReloading)
            return;

        reloadTimer -= Time.deltaTime;
        reloadProgressImage.fillAmount = reloadTimer / reloadDuration;

        if (reloadTimer <= 0f)
        {
            ammoInMagazine = magazineSize;
            reloadTimer = 0f;
            isReloading = false;

            reloadProgressImage.gameObject.SetActive(false);

            bulletController.Reload();
        }
    }

    private void HandleShooting()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        // Cannot shoot while reloading or without ammo.
        if (isReloading || ammoInMagazine <= 0)
            return;

        if (Time.time < nextShotTime)
            return;

        nextShotTime = Time.time + fireCooldown;
        ammoInMagazine--;

        Vector2 origin = firePoint.position;
        Vector2 direction = firePoint.right;

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
            else if (hitVfx != null)
            {
                Instantiate(hitVfx, hit.point, Quaternion.identity);
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

        if (Impact != null)
            Impact.enabled = true;

        if (ImpactVFX != null)
            ImpactVFX.Play();

        if (ImpactSound != null)
            ImpactSound.Play();

        if (Trace != null)
            Trace.Play();

        CancelInvoke(nameof(DisableImpact));
        Invoke(nameof(DisableImpact), 0.1f);

        bulletController.WasteBullet();
    }

    private void DisableImpact()
    {
        if (Impact != null)
            Impact.enabled = false;
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

