using UnityEngine;

public class HPController : MonoBehaviour
{
    [SerializeField]
    private float maxHP;

    private float currentHP;


    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        if (currentHP <= 0)
        {
            Death();
        }
    }
    public void Death()
    {
        Debug.Log("Someones dead");
        Destroy(gameObject);
    }
}
