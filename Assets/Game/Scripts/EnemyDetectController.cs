using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.UI.Image;

public class EnemyDetectController : MonoBehaviour
{
    [SerializeField] private Transform directionPos;
    [SerializeField] private Collider2D detector;
    [SerializeField] private SpriteRenderer foundedSprite;
    [SerializeField] private LayerMask hitLayer = ~0;

    [SerializeField] private float timeToChase;
    [SerializeField] private EnemyController controller;

    private float currentChaseTimer;
    private bool playerDetected;

    private void Awake()
    {
        currentChaseTimer = 0.0f;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        Vector2 origin = directionPos.position;
        Vector2 target = controller.character.transform.position;
        Vector2 direction = target - origin;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            origin,
            direction,
            10f
        );

        Debug.DrawRay(
            origin,
            direction * 10f,
            Color.red
        );

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == detector ) continue;

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                if (!playerDetected)
                {
                    Debug.Log("Founded!!!");
                    foundedSprite.enabled = true;
                    controller.StartChase();
                    playerDetected = true;
                }

                currentChaseTimer = 0f;
                break;
            }
            else
            {
                break;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            currentChaseTimer = timeToChase;
        }
    }

    private void FixedUpdate()
    {
        if (currentChaseTimer > 0.0f)
        {
            currentChaseTimer -= Time.deltaTime;
            if (currentChaseTimer <= 0.0f)
            {
                controller.ContinuePatrol();
                foundedSprite.enabled = false;
                playerDetected = false;
            }
        }
    }

    public void StartChase()
    {
        foundedSprite.enabled = true;
        controller.StartChase();
        playerDetected = true;
        currentChaseTimer = 0f;
    }
}
