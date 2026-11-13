using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Chicken Shooter/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [Min(1)] public int levelNumber = 1;
    public string sceneName;
    public AudioClip backgroundMusic;
    public Sprite environmentTheme;
    public WaveDataSO[] rounds;
}