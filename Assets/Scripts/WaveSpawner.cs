using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private LevelDataSO levelData;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private EnemyEggPool eggPool;
    [SerializeField] private AsteroidPool asteroidPool;
    [SerializeField] private Transform playerShip;

    [SerializeField] private UnityEvent<int> onRoundStarted =
        new UnityEvent<int>();

    [SerializeField] private UnityEvent onStageCleared =
        new UnityEvent();

    private IEnumerator Start()
    {
        if (levelData == null || gameCamera == null ||
            levelData.rounds == null || levelData.rounds.Length == 0)
        {
            Debug.LogError("Check Level Data, Game Camera and Rounds.", this);
            yield break;
        }

        for (int roundIndex = 0; roundIndex < levelData.rounds.Length; roundIndex++)
        {
            if (GameManager.Instance != null && GameManager.Instance.Lives <= 0)
                yield break;

            WaveDataSO wave = levelData.rounds[roundIndex];
            int roundNumber = roundIndex + 1;

            if (wave == null || wave.enemyPrefab == null)
            {
                Debug.LogError(
                    $"Round {roundNumber}: check Wave Data and Enemy Prefab.", this);
                yield break;
            }

            if (wave.waveType == WaveType.Asteroids && asteroidPool == null)
            {
                Debug.LogError("Connect Asteroid Pool.", this);
                yield break;
            }

            Debug.Log($"Round {roundNumber} started.", this);
            onRoundStarted.Invoke(roundNumber);

            yield return StartCoroutine(PlayRound(wave));

            Debug.Log($"Round {roundNumber} cleared.", this);
            yield return new WaitForSeconds(1f);
        }

        if (GameManager.Instance != null && GameManager.Instance.Lives <= 0)
            yield break;

        Debug.Log($"Level {levelData.levelNumber} cleared.", this);
        onStageCleared.Invoke();
    }

    private IEnumerator PlayRound(WaveDataSO wave)
    {
        if (wave.waveType == WaveType.Asteroids)
        {
            yield return StartCoroutine(PlayAsteroidRound(wave));
            yield break;
        }

        GameObject[] chickens = new GameObject[wave.enemyCount];

        for (int i = 0; i < chickens.Length; i++)
        {
            Vector3 target = GetFormationPosition(i, wave);
            float entryX = GetEntryX(i, wave.entryPath);

            Vector3 start = ScreenPosition(entryX, 1.15f);
            Vector3 bend = ScreenPosition(1f - entryX, 0.95f);

            GameObject chicken = Instantiate(
                wave.enemyPrefab, start, Quaternion.identity);

            chickens[i] = chicken;

            bool isDiver = wave.diveEvery > 0 &&
                (i + 1) % wave.diveEvery == 0;

            StartCoroutine(FlyIn(
                chicken.transform, start, bend, target, wave, isDiver));

            yield return new WaitForSeconds(0.2f);
        }

        foreach (GameObject chicken in chickens)
        {
            while (chicken != null)
                yield return null;
        }
    }

    private float GetEntryX(int index, WaveEntryPath entryPath)
    {
        switch (entryPath)
        {
            case WaveEntryPath.FromLeft:
                return 0.1f;

            case WaveEntryPath.FromRight:
                return 0.9f;

            default:
                return index % 2 == 0 ? 0.2f : 0.8f;
        }
    }

    private Vector3 GetFormationPosition(int index, WaveDataSO wave)
    {
        int columns = Mathf.Clamp(
            wave.columns, 1, Mathf.Max(1, wave.enemyCount));

        int rows = Mathf.CeilToInt((float)wave.enemyCount / columns);
        int row = index / columns;
        int column = index % columns;

        float columnPosition = columns > 1
            ? (float)column / (columns - 1)
            : 0.5f;

        float rowPosition = rows > 1
            ? (float)row / (rows - 1)
            : 0f;

        float x;
        float y;

        switch (wave.formation)
        {
            case WaveFormation.VShape:
                x = Mathf.Lerp(0.2f, 0.8f, columnPosition);

                float distanceFromCenter =
                    Mathf.Abs(columnPosition - 0.5f) * 2f;

                y = 0.68f + distanceFromCenter * 0.16f
                    - rowPosition * 0.18f;
                break;

            case WaveFormation.Tight:
                x = Mathf.Lerp(0.35f, 0.65f, columnPosition);
                y = Mathf.Lerp(0.82f, 0.68f, rowPosition);
                break;

            default:
                x = Mathf.Lerp(0.25f, 0.75f, columnPosition);
                y = Mathf.Lerp(0.82f, 0.58f, rowPosition);
                break;
        }

        return ScreenPosition(x, y);
    }

    private IEnumerator FlyIn(
        Transform chicken, Vector3 start, Vector3 bend,
        Vector3 target, WaveDataSO wave, bool isDiver)
    {
        float pathLength = Vector3.Distance(start, bend)
            + Vector3.Distance(bend, target);

        float duration = pathLength / Mathf.Max(0.1f, wave.moveSpeed);
        float elapsed = 0f;

        while (elapsed < duration && chicken != null)
        {
            float t = elapsed / duration;
            Vector3 first = Vector3.Lerp(start, bend, t);
            Vector3 second = Vector3.Lerp(bend, target, t);

            chicken.position = Vector3.Lerp(first, second, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (chicken == null)
            yield break;

        chicken.position = target;

        if (isDiver)
        {
            yield return StartCoroutine(DiveAtShip(chicken, wave));
            yield break;
        }

        if (eggPool == null)
            yield break;

        float minInterval = Mathf.Max(0.1f, wave.minEggInterval);
        float maxInterval = Mathf.Max(minInterval, wave.maxEggInterval);

        while (chicken != null)
        {
            yield return new WaitForSeconds(
                Random.Range(minInterval, maxInterval));

            if (chicken != null)
                eggPool.DropEgg(chicken.position + Vector3.down * 0.4f);
        }
    }

    private IEnumerator DiveAtShip(Transform chicken, WaveDataSO wave)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, wave.diveDelay));

        if (chicken == null)
            yield break;

        Vector3 aim = playerShip != null
            ? playerShip.position - chicken.position
            : Vector3.down;

        Vector3 direction = aim.sqrMagnitude > 0.001f
            ? aim.normalized
            : Vector3.down;

        float speed = Mathf.Max(
            0.1f, wave.moveSpeed * wave.diveSpeedMultiplier);

        while (chicken != null)
        {
            chicken.position += direction * (speed * Time.deltaTime);

            Vector3 view = gameCamera.WorldToViewportPoint(
                chicken.position);

            if (view.x < -0.1f || view.x > 1.1f ||
                view.y < -0.1f || view.y > 1.1f)
            {
                Destroy(chicken.gameObject);
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator PlayAsteroidRound(WaveDataSO wave)
    {
        AsteroidHealth[] asteroids =
            new AsteroidHealth[wave.enemyCount];

        for (int i = 0; i < asteroids.Length; i++)
        {
            float x = Random.Range(0.1f, 0.9f);
            Vector3 position = ScreenPosition(x, 1.1f);

            asteroids[i] = asteroidPool.Spawn(
                position, Mathf.Max(0.1f, wave.moveSpeed));

            yield return new WaitForSeconds(0.7f);
        }

        foreach (AsteroidHealth asteroid in asteroids)
        {
            while (asteroid != null && asteroid.gameObject.activeSelf)
                yield return null;
        }
    }

    private Vector3 ScreenPosition(float x, float y)
    {
        float depth = -gameCamera.transform.position.z;

        return gameCamera.ViewportToWorldPoint(
            new Vector3(x, y, depth));
    }
}