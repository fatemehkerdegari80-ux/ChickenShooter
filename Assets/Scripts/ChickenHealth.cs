using UnityEngine;

public class ChickenHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int health = 2;
    [SerializeField] private GameObject weaponGiftPrefab;
    [SerializeField, Range(0f, 1f)] private float giftDropChance = 0.3f;
    [SerializeField, Min(0)] private int scoreValue = 100;

    public void TakeDamage(int damage, bool awardRewards = true)
    {
        if (health <= 0)
            return;

        health -= damage;

        if (health > 0)
            return;

        if (awardRewards)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(scoreValue);
                Debug.Log($"Score: {GameManager.Instance.Score}", this);
            }

            if (weaponGiftPrefab != null && Random.value < giftDropChance)
            {
                Instantiate(
                    weaponGiftPrefab,
                    transform.position,
                    Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}