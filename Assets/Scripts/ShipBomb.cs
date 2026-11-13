using UnityEngine;
using UnityEngine.InputSystem;

public class ShipBomb : MonoBehaviour
{
    [SerializeField] private InputActionReference bombAction;
    [SerializeField] private EnemyEggPool eggPool;
    [SerializeField] private Camera gameCamera;
    [SerializeField, Min(0)] private int bombsRemaining = 2;

    private void OnEnable()
    {
        bombAction?.action.Enable();
    }

    private void OnDisable()
    {
        bombAction?.action.Disable();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            bombsRemaining = GameManager.Instance.Bombs;
    }

    private void Update()
    {
        if (bombAction == null || gameCamera == null ||
            !bombAction.action.WasPressedThisFrame())
            return;

        if (bombsRemaining <= 0)
        {
            Debug.Log("No bombs left.");
            return;
        }

        bombsRemaining--;

        if (GameManager.Instance != null)
            GameManager.Instance.SetBombs(bombsRemaining);

        eggPool?.ClearAllEggs();

        ChickenHealth[] chickens = FindObjectsByType<ChickenHealth>(
            FindObjectsSortMode.None);

        foreach (ChickenHealth chicken in chickens)
        {
            if (IsInsideScreen(chicken.transform.position))
                chicken.TakeDamage(int.MaxValue);
        }

        AsteroidHealth[] asteroids = FindObjectsByType<AsteroidHealth>(
            FindObjectsSortMode.None);

        foreach (AsteroidHealth asteroid in asteroids)
        {
            if (IsInsideScreen(asteroid.transform.position))
                asteroid.TakeDamage(int.MaxValue);
        }

        Debug.Log($"Bomb used. Remaining: {bombsRemaining}");
    }

    private bool IsInsideScreen(Vector3 position)
    {
        Vector3 view = gameCamera.WorldToViewportPoint(position);

        return view.z > 0f &&
            view.x >= 0f && view.x <= 1f &&
            view.y >= 0f && view.y <= 1f;
    }
}