using UnityEngine;

public class HPController : MonoBehaviour
{
    [SerializeField]
    private float maxHP;

    [SerializeField]
    private ParticleSystem bloodVFX;

    [SerializeField]
    private SpriteRenderer[] spritesToFlash;

    private Color[] originalColors;

    private float currentHP;


    private void Awake()
    {
        currentHP = maxHP;

        originalColors = new Color[spritesToFlash.Length];
        for (int i = 0; i < spritesToFlash.Length; i++)
        {
            originalColors[i] = spritesToFlash[i].color;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        bloodVFX.Play();
        DamageImpact();
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

    private void DamageImpact()
    {
        foreach (var sprite in spritesToFlash)
        {
            sprite.color = Color.white;
        }
        Invoke("ReturnColor", 0.1f);
    }

    private void ReturnColor()
    {
        for (int i = 0; i < spritesToFlash.Length; i++)
        {
            spritesToFlash[i].color = originalColors[i];
        }
    }
}
