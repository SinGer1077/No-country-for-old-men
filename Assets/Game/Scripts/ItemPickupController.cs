using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;

public class ItemPickupController : MonoBehaviour
{
    private bool readyToPickUp;
    public int itemIndex;
    public bool availableToPickUp;
    public int requireItem = -1;
    public PlayerController2D character;

    public Transform help;

    public UnityEvent onUse;
    

    private void Awake()
    {
        if (character == null)
        {
            character = FindFirstObjectByType<PlayerController2D>();
        }
    }

    private void Start()
    {
       
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision != null && collision.tag == "Player")
        {
            readyToPickUp = true;
        }
    }

    public void OnTriggeExit2D(Collider2D collision)
    {
        if (collision != null && collision.tag == "Player")
        {
            readyToPickUp = false;
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && readyToPickUp)
        {
            if (!availableToPickUp)
            {
                Use();
                return;
            }

            readyToPickUp = false;
            character.PickUpItem(this);
            help.gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    public bool Use()
    {
        if (requireItem != -1)
        {
            if (character.pickedUpItem?.itemIndex != requireItem)
            {
                return false;
            }
        }
        onUse.Invoke();
        Destroy(gameObject);

        return true;
    }
}
