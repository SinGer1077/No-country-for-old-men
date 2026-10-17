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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.tag == "Player")
        //{
        //    RaycastHit2D hit = Physics2D.Raycast(directionPos.position, directionPos.up, 10.0f, hitLayer.value);
        //    if (hit.collider != null && hit.collider.tag == "Player")
        //    {
        //        Debug.Log("Founded!!!");
        //        foundedSprite.enabled = true;
        //        controller.StartChase();
        //        currentChaseTimer = 0.0f;
        //    }
        //}
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        RaycastHit2D[] hits = Physics2D.RaycastAll(
            directionPos.position,
            directionPos.up,
            10f
        );

        Debug.DrawRay(
            directionPos.position,
            directionPos.up * 10f,
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
            }
        }
    }
}
