using UnityEngine;

public class AsteroidHealth : MonoBehaviour
{
    [SerializeField, Min(0)] private int scoreValue = 50;

    private AsteroidPool pool;
    private Camera gameCamera;
    private int health;
    private float fallSpeed;
    private bool returned;

    public void Initialize(
        AsteroidPool owner, Camera camera, int startingHealth, float speed)
    {
        pool = owner;
        gameCamera = camera;
        health = startingHealth;
        fallSpeed = speed;
        returned = false;
    }

    private void Update()
    {
        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);

        if (gameCamera.WorldToViewportPoint(transform.position).y < -0.1f)
            ReturnToPool();
    }

    public void TakeDamage(int damage)
    {
        if (returned)
            return;

        health -= damage;

        if (health > 0)
            return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
            Debug.Log($"Score: {GameManager.Instance.Score}", this);
        }

        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (returned)
            return;

        returned = true;
        pool.Recycle(this);
    }
}