using UnityEngine;

public enum WaveType
{
    Chickens,
    Asteroids
}

public enum WaveFormation
{
    Grid,
    VShape,
    Tight
}

public enum WaveEntryPath
{
    Alternating,
    FromLeft,
    FromRight
}

[CreateAssetMenu(fileName = "NewWave", menuName = "Chicken Shooter/Wave Data")]
public class WaveDataSO : ScriptableObject
{
    public WaveType waveType;
    public GameObject enemyPrefab;
    public WaveFormation formation;
    public WaveEntryPath entryPath = WaveEntryPath.Alternating;

    [Min(1)] public int enemyCount = 6;
    [Min(0.1f)] public float moveSpeed = 3f;

    [Header("Chicken Formation")]
    [Min(1)] public int columns = 3;

    [Header("Egg Dropping")]
    [Min(0.1f)] public float minEggInterval = 2f;
    [Min(0.1f)] public float maxEggInterval = 4f;

    [Header("Diving")]
    [Min(0)] public int diveEvery = 4;
    [Min(0f)] public float diveDelay = 1.5f;
    [Min(0.1f)] public float diveSpeedMultiplier = 2.5f;
}