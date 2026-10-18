using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

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

    [SerializeField]
    private UnityEvent onTakeDamage;

    [SerializeField]
    private UnityEvent onDeath;


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
        onTakeDamage.Invoke();
        if (currentHP <= 0)
        {
            onDeath.Invoke();
            Death();
        }
    }
    public void Death()
    {
        Debug.Log("Someones dead");
        DeathImpact();
        Invoke("LateDestroy", 3.0f);
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

    private void DeathImpact()
    {
        for (int i = 0; i < spritesToFlash.Length; i++)
        {
            spritesToFlash[i].transform.DORotate(new Vector3(0, 0, GetRandom(0, 180)), 1.5f);
            spritesToFlash[i].transform.DOMove(transform.position + new Vector3(GetRandom(-5, 5), GetRandom(-5, 5), 0), 1.5f);
        }
    }

    private float GetRandom(float min, float max)
    {
        return Random.Range(min, max);
    }

    private void LateDestroy()
    {
        Destroy(gameObject);
    }
}
