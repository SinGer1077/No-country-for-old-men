using UnityEngine;

public class EnemyMovingAlgorithm
{
    public float angularSpeed = 1f;
    public float circleRad = 1f;

    private Vector2 fixedPoint;
    private float currentAngle;

    public EnemyMovingAlgorithm()
    {

    }
    public Vector2 SimpleMovement(float deltaTime)
    {
        currentAngle += angularSpeed * deltaTime;
        Vector2 offset = new Vector2(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle)) * circleRad;
        return fixedPoint + offset;
    }
}
