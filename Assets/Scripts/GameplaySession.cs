using UnityEngine;

public class GameplaySession : MonoBehaviour
{
    [SerializeField] private ShipMovement movement;
    [SerializeField] private ShipShooting shooting;
    [SerializeField] private ShipBomb bomb;
    [SerializeField] private WaveSpawner waveSpawner;

    private GameManager gameManager;
    private bool stopped;
    private float previousTimeScale;

    private void Start()
    {
        gameManager = GameManager.Instance;

        if (gameManager == null)
        {
            Debug.LogError("GameplaySession: GameManager is missing.", this);
            return;
        }

        gameManager.StateChanged += CheckLives;
        CheckLives();
    }

    private void CheckLives()
    {
        if (stopped || gameManager.Lives > 0)
            return;

        stopped = true;

        if (movement != null)
            movement.enabled = false;

        if (shooting != null)
            shooting.enabled = false;

        if (bomb != null)
            bomb.enabled = false;

        if (waveSpawner != null)
            waveSpawner.StopAllCoroutines();

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Debug.Log("Ship destroyed. Gameplay stopped.", this);
    }

    private void OnDestroy()
    {
        if (gameManager != null)
            gameManager.StateChanged -= CheckLives;

        if (stopped)
            Time.timeScale = previousTimeScale;
    }
}