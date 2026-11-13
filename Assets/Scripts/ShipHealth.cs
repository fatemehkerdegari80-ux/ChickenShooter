using System.Collections;
using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    [SerializeField] private EnemyEggPool eggPool;
    [SerializeField, Min(1)] private int lives = 3;
    [SerializeField, Min(0f)] private float invulnerabilitySeconds = 1f;

    private ShipShooting shooting;
    private SpriteRenderer shipSprite;
    private float nextDamageTime;

    private void Awake()
    {
        shooting = GetComponent<ShipShooting>();
        shipSprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            lives = GameManager.Instance.Lives;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool hitByEgg = eggPool != null &&
            eggPool.TryRecycleEgg(other.gameObject);

        bool hitByChicken = other.TryGetComponent(out ChickenHealth chicken);
        bool hitByAsteroid = other.TryGetComponent(out AsteroidHealth asteroid);

        if (!hitByEgg && !hitByChicken && !hitByAsteroid)
            return;

        if (hitByChicken)
            chicken.TakeDamage(int.MaxValue, false);

        if (hitByAsteroid)
            asteroid.ReturnToPool();

        if (lives <= 0 || Time.time < nextDamageTime)
            return;

        lives--;
        nextDamageTime = Time.time + invulnerabilitySeconds;
        shooting?.LowerPowerLevel();

        if (GameManager.Instance != null)
            GameManager.Instance.SetLives(lives);

        if (shipSprite != null)
            StartCoroutine(Blink());

        Debug.Log($"Ship hit. Lives remaining: {lives}", this);
    }

    private IEnumerator Blink()
    {
        while (Time.time < nextDamageTime)
        {
            shipSprite.enabled = !shipSprite.enabled;
            float remainingTime = nextDamageTime - Time.time;
            yield return new WaitForSeconds(Mathf.Min(0.1f, remainingTime));
        }

        shipSprite.enabled = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (shipSprite != null)
            shipSprite.enabled = true;
    }
}