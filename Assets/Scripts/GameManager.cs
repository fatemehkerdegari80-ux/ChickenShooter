using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Lives { get; private set; } = 3;
    public int Bombs { get; private set; } = 2;
    public int PowerLevel { get; private set; } = 1;
    public int Score { get; private set; }

    public bool HasShipPosition { get; private set; }
    public Vector2 ShipViewportPosition { get; private set; }

    public event Action StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SaveShipPosition(Vector2 position)
    {
        ShipViewportPosition = position;
        HasShipPosition = true;
    }

    public void SetLives(int value)
    {
        Lives = Mathf.Max(0, value);
        StateChanged?.Invoke();
    }

    public void SetBombs(int value)
    {
        Bombs = Mathf.Max(0, value);
        StateChanged?.Invoke();
    }

    public void SetPowerLevel(int value)
    {
        PowerLevel = Mathf.Clamp(value, 1, 3);
        StateChanged?.Invoke();
    }

    public void AddScore(int amount)
    {
        Score += Mathf.Max(0, amount);
        StateChanged?.Invoke();
    }

    public void ResetRun()
    {
        Lives = 3;
        Bombs = 2;
        PowerLevel = 1;
        Score = 0;
        HasShipPosition = false;
        ShipViewportPosition = Vector2.zero;
        StateChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}