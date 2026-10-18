using UnityEngine;

public class NoiseController : MonoBehaviour
{
    public void MakeNoise(Vector3 pos)
    {
        Collider2D[] touchedEnemies = Physics2D.OverlapCircleAll(pos, 10.0f);

        foreach (var enemy in touchedEnemies)
        {
            if (enemy.tag == "Enemy")
            {
                EnemyController controller;
                if (enemy.TryGetComponent<EnemyController>(out controller))
                {
                    controller.CheckState(pos);
                }
            }
        }
    }
}
