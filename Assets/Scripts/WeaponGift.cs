using UnityEngine;

public class WeaponGift : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float fallSpeed = 2f;
    [SerializeField, Min(1f)] private float lifetime = 12f;

    private bool collected;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += Vector3.down * (fallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.TryGetComponent(out ShipShooting shooting))
            return;

        collected = true;
        shooting.RaisePowerLevel();
        Destroy(gameObject);
    }
}