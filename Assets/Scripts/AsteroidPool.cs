using System.Collections.Generic;
using UnityEngine;

public class AsteroidPool : MonoBehaviour
{
    [SerializeField] private AsteroidHealth asteroidPrefab;
    [SerializeField] private Camera gameCamera;
    [SerializeField, Min(1)] private int asteroidHealth = 3;

    private readonly Queue<AsteroidHealth> availableAsteroids =
        new Queue<AsteroidHealth>();

    public AsteroidHealth Spawn(Vector3 position, float speed)
    {
        AsteroidHealth asteroid = availableAsteroids.Count > 0
            ? availableAsteroids.Dequeue()
            : Instantiate(asteroidPrefab, transform);

        asteroid.transform.position = position;
        asteroid.Initialize(this, gameCamera, asteroidHealth, speed);
        asteroid.gameObject.SetActive(true);

        return asteroid;
    }

    public void Recycle(AsteroidHealth asteroid)
    {
        asteroid.gameObject.SetActive(false);
        availableAsteroids.Enqueue(asteroid);
    }
}