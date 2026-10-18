using UnityEngine;
using UnityEngine.UI;

public class BulletController : MonoBehaviour
{
    [SerializeField]
    private Image[] bullets;

    private int currentBullet = 0;

    public void WasteBullet()
    {
        bullets[currentBullet].enabled = false;
        currentBullet += 1;
    }
    
    public void Reload()
    {
        foreach (var bullet in bullets)
        {
            bullet.enabled = true;
        }

        currentBullet = 0;
    }
}
