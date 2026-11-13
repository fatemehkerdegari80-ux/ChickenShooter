using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShipShooting : MonoBehaviour
{
    [SerializeField] private InputActionReference fireAction;
    [SerializeField] private WeaponDataSO weapon;
    [SerializeField] private WeaponDataSO weaponLevel2;
    [SerializeField] private WeaponDataSO weaponLevel3;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private LayerMask enemyLayers;
    [SerializeField] private EnemyEggPool eggPool;

    [SerializeField, Range(1, 3)] private int powerLevel = 1;
    [SerializeField, Min(0f)] private float muzzleOffset = 0.55f;
    [SerializeField, Min(0f)] private float doubleShotOffset = 0.18f;
    [SerializeField, Range(0f, 45f)] private float spreadAngle = 15f;

    private readonly Dictionary<GameObject, Queue<GameObject>> bulletPools =
        new Dictionary<GameObject, Queue<GameObject>>();

    private readonly List<ActiveBullet> activeBullets =
        new List<ActiveBullet>();

    private float nextShotTime;

    private struct ActiveBullet
    {
        public GameObject Object;
        public Queue<GameObject> Pool;
        public Vector3 Velocity;
        public int Damage;
    }

    private void OnEnable()
    {
        if (fireAction != null)
            fireAction.action.Enable();
    }

    private void OnDisable()
    {
        if (fireAction != null)
            fireAction.action.Disable();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            powerLevel = GameManager.Instance.PowerLevel;
    }

    private void Update()
    {
        if (gameCamera == null)
            return;

        MoveBullets();

        WeaponDataSO currentWeapon = GetCurrentWeapon();

        if (fireAction == null || currentWeapon == null ||
            currentWeapon.projectilePrefab == null ||
            currentWeapon.shotsPerSecond <= 0f)
            return;

        InputAction fire = fireAction.action;

        if (Time.time >= nextShotTime &&
            (fire.WasPressedThisFrame() || fire.IsPressed()))
        {
            FirePattern(currentWeapon);
            nextShotTime = Time.time + 1f / currentWeapon.shotsPerSecond;
        }
    }

    public void RaisePowerLevel()
    {
        powerLevel = Mathf.Min(3, powerLevel + 1);

        if (GameManager.Instance != null)
            GameManager.Instance.SetPowerLevel(powerLevel);
    }

    public void LowerPowerLevel()
    {
        powerLevel = Mathf.Max(1, powerLevel - 1);

        if (GameManager.Instance != null)
            GameManager.Instance.SetPowerLevel(powerLevel);
    }

    private WeaponDataSO GetCurrentWeapon()
    {
        switch (powerLevel)
        {
            case 2:
                return weaponLevel2;

            case 3:
                return weaponLevel3;

            default:
                return weapon;
        }
    }

    private void FirePattern(WeaponDataSO currentWeapon)
    {
        switch (currentWeapon.shotPattern)
        {
            case ShotPattern.Double:
                FireBullet(currentWeapon, -doubleShotOffset, 0f);
                FireBullet(currentWeapon, doubleShotOffset, 0f);
                break;

            case ShotPattern.Spread:
                FireBullet(currentWeapon, 0f, -spreadAngle);
                FireBullet(currentWeapon, 0f, 0f);
                FireBullet(currentWeapon, 0f, spreadAngle);
                break;

            default:
                FireBullet(currentWeapon, 0f, 0f);
                break;
        }
    }

    private void FireBullet(
        WeaponDataSO currentWeapon, float sideOffset, float angle)
    {
        GameObject prefab = currentWeapon.projectilePrefab;

        if (!bulletPools.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            bulletPools.Add(prefab, pool);
        }

        Vector3 position = transform.position
            + Vector3.up * muzzleOffset
            + Vector3.right * sideOffset;

        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

        GameObject bullet = pool.Count > 0
            ? pool.Dequeue()
            : Instantiate(prefab, position, rotation);

        bullet.transform.SetPositionAndRotation(position, rotation);
        bullet.SetActive(true);

        activeBullets.Add(new ActiveBullet
        {
            Object = bullet,
            Pool = pool,
            Velocity = rotation * Vector3.up * currentWeapon.projectileSpeed,
            Damage = currentWeapon.damage
        });
    }

    private void MoveBullets()
    {
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            ActiveBullet bullet = activeBullets[i];

            Vector3 start = bullet.Object.transform.position;
            Vector3 end = start + bullet.Velocity * Time.deltaTime;

            RaycastHit2D hit = Physics2D.Linecast(start, end, enemyLayers);

            if (hit.collider != null &&
                hit.collider.TryGetComponent(out ChickenHealth chicken))
            {
                chicken.TakeDamage(bullet.Damage);
                RecycleBullet(i);
                continue;
            }

            if (hit.collider != null && eggPool != null &&
                eggPool.TryRecycleEgg(hit.collider.gameObject))
            {
                RecycleBullet(i);
                continue;
            }

            if (hit.collider != null &&
                hit.collider.TryGetComponent(out AsteroidHealth asteroid))
            {
                asteroid.TakeDamage(bullet.Damage);
                RecycleBullet(i);
                continue;
            }

            bullet.Object.transform.position = end;
            Vector3 view = gameCamera.WorldToViewportPoint(end);

            if (view.y > 1.05f || view.x < -0.05f || view.x > 1.05f)
                RecycleBullet(i);
        }
    }

    private void RecycleBullet(int index)
    {
        ActiveBullet bullet = activeBullets[index];
        bullet.Object.SetActive(false);
        bullet.Pool.Enqueue(bullet.Object);
        activeBullets.RemoveAt(index);
    }
}