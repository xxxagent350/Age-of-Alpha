using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BallisticWeapon : Weapon
{
    [Header("Параметры оружия")]
    [Tooltip("Префаб снаряда")]
    public GameObject ProjectilePrefab;
    [Tooltip("Время перезарядки в секундах")]
    public float ReloadTime = 1f;
    [Tooltip("Потребление энергии за 1 выстрел")]
    public float EnergyConsumption = 1f;
    [Tooltip("Позиции стволов относительно центра (локальные координаты)")]
    public List<Vector2> BarrelsPositions;
    [Tooltip("Количество снарядов в залпе")]
    public int ProjectilesPerSalvo = 1;
    [Tooltip("Угол разброса в градусах")]
    public float ScatterAngle = 5f;

    [Tooltip("Названия эффектов выстрела")]
    [SerializeField] private List<string> shootEffectsNames;

    private AttachedToShipEffectsSpawner _destroyedModulesEffectsSpawner;
    private int currentBarrelNum;
    private Rigidbody2D myShipRigidbody2D;
    private float _currentReloadTime = 0f;
    private ModulesVisualReloadStateSynchronizer _modulesVisualReloadStateSynchronizer;

    public override void Initialize()
    {
#if UNITY_EDITOR
        if (BarrelsPositions == null || BarrelsPositions.Count == 0)
        {
            Debug.LogWarning($"Не заданы позиции стволов (BarrelsPositions) у {gameObject.name}");
            isWorking = false;
        }
#endif
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            if (myShipGameStats != null)
            {
                myShipRigidbody2D = myShipGameStats.GetComponent<Rigidbody2D>();
                _modulesVisualReloadStateSynchronizer = myShipGameStats.GetComponent<ModulesVisualReloadStateSynchronizer>();
            }
            _destroyedModulesEffectsSpawner = GetComponentInParent<AttachedToShipEffectsSpawner>();
        }
    }

    public override void FixedServerUpdate()
    {
        if (isWorking)
        {
            Reload();
        }
    }

    public override void RandomizedServerUpdate(float deltaTime)
    {
        if (isWorking && isFiring)
        {
            Fire();
        }
    }

    // Перезарядка оружия
    public void Reload()
    {
        if (_currentReloadTime < ReloadTime)
        {
            _currentReloadTime += Time.deltaTime;
            if (_currentReloadTime > ReloadTime)
            {
                _currentReloadTime = ReloadTime;
            }

            if (_modulesVisualReloadStateSynchronizer != null)
                _modulesVisualReloadStateSynchronizer.AddRechargeProgressData(WeaponIndex, _currentReloadTime / ReloadTime);
        }
    }

    // Выстрел
    public void Fire()
    {
        if (_currentReloadTime >= ReloadTime)
        {
            if (myShipGameStats != null && myShipGameStats.TrySpendEnergy(EnergyConsumption))
            {
                _currentReloadTime = 0f;

                Vector2 shotPoint = (Vector2)transform.position + DataOperator.RotateVector2(BarrelsPositions[currentBarrelNum], transform.eulerAngles.z);
                Vector2 localShotPoint = (Vector2)transform.localPosition + BarrelsPositions[currentBarrelNum];
                Salvo(shotPoint);
                SpawnEffects(localShotPoint);
            }
        }
    }

    private void SpawnEffects(Vector2 localShotPoint)
    {
        if (_destroyedModulesEffectsSpawner != null)
            _destroyedModulesEffectsSpawner.SpawnAndAttachEffects(shootEffectsNames, localShotPoint, Quaternion.identity);
    }

    // Залп
    void Salvo(Vector2 shotPoint)
    {
        for (int projectileNum = 0; projectileNum < ProjectilesPerSalvo; projectileNum++)
        {
            SpawnProjectile(shotPoint);
            if (BarrelsPositions != null && BarrelsPositions.Count > 0)
            {
                if (currentBarrelNum < BarrelsPositions.Count - 1)
                {
                    currentBarrelNum++;
                }
                else
                {
                    currentBarrelNum = 0;
                }
            }
        }
    }

    void SpawnProjectile(Vector2 shotPoint)
    {
        Quaternion rotation = transform.rotation * Quaternion.Euler(0f, 0f, Random.Range(-ScatterAngle, ScatterAngle));
        GameObject projectile = Instantiate(ProjectilePrefab, shotPoint, rotation);
        if (projectile == null) return;

        var rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null && myShipRigidbody2D != null)
            rb.linearVelocity = myShipRigidbody2D.linearVelocity;

        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        if (projectileComponent != null)
        {
            projectileComponent.ApplyImpulseToParentShip(myShipRigidbody2D);

            var netObj = projectile.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Spawn();

            // Оригинальная логика присвоения TeamID оставлена как была
            projectileComponent.TeamID.Value = new NetworkString(TeamID);
        }
    }

    // Отрисовка гизмо в редакторе
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector2 myPosition = transform.position;
            if (BarrelsPositions != null)
            {
                foreach (Vector2 barrelPos in BarrelsPositions)
                {
                    Gizmos.DrawIcon(myPosition + barrelPos, "Aim icon.png", false);
                }
            }
        }
    }
}
