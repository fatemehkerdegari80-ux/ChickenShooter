using UnityEngine;

public enum ShotPattern
{
    Single,
    Double,
    Spread
}

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Chicken Shooter/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    public string weaponName = "Laser";
    public Sprite weaponSprite;
    public GameObject projectilePrefab;
    public ShotPattern shotPattern = ShotPattern.Single;

    [Min(1)] public int damage = 1;
    [Min(0.1f)] public float projectileSpeed = 12f;
    [Min(0.1f)] public float shotsPerSecond = 5f;
}