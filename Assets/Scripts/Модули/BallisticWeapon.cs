using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BallisticWeapon : Weapon
{
    [Header("Настройка")]
    [Tooltip("Префаб пули")]
    [SerializeField] GameObject projectilePrefab;
    [Tooltip("Энергопотребление на 1 залп")]
    [SerializeField] float energyConsumption = 1;
    [Tooltip("Позиции из которых по очереди будут вылетать пули(обозначены значком прицела в редакторе)")]
    [SerializeField] List<Vector2> barrelsPositions;
    [Tooltip("Время между выстрелами в секундах")]
    [SerializeField] float reloadTime = 0.5f;
    [Tooltip("Количество снарядов выпускаемых за 1 залп")]
    [SerializeField] int projectilesPerSalvo = 1;
    [Tooltip("Разброс в градусах")]
    [SerializeField] float scatterAngle = 5;

    float reloadTimer;
    int currentBarrelNum;
    Rigidbody2D myShipRigidbody2D;

    public override void Initialize()
    {
#if UNITY_EDITOR
        if (barrelsPositions.Count == 0)
        {
            Debug.LogWarning($"Добавьте хотя бы одну позицию вылета снарядов в barrelsPositions для {gameObject.name}");
            isWorking = false;
        }
#endif
        if (NetworkManager.Singleton.IsServer)
        {
            myShipRigidbody2D = myShipGameStats.GetComponent<Rigidbody2D>();
        }
    }

    public override void ServerUpdate()
    {
        if (isWorking)
        {
            Reload();
            if (FIRE)
            {
                Fire();
            }
        }
    }

    //отвечает за перезарядку
    void Reload()
    {
        if (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
        }
    }

    //отвечает за стрельбу
    public void Fire()
    {
        if (reloadTimer >= reloadTime)
        {
            reloadTimer = 0;
            if (myShipGameStats.TakeEnergy(energyConsumption))
            {
                Salvo();
            }
        }
    }

    //залп
    void Salvo()
    {
        for (int projectileNum = 0; projectileNum < projectilesPerSalvo; projectileNum++)
        {
            Vector2 myPosition = transform.position;
            SpawnProjectile(myPosition + DataOperator.RotateVector2(barrelsPositions[currentBarrelNum], transform.eulerAngles.z));
            if (currentBarrelNum < barrelsPositions.Count - 1)
            {
                currentBarrelNum++;
            }
            else
            {
                currentBarrelNum = 0;
            }
        }
    }

    void SpawnProjectile(Vector2 position)
    {
        Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(-scatterAngle, scatterAngle));
        GameObject projectile = Instantiate(projectilePrefab, position, rotation);
        projectile.GetComponent<Rigidbody2D>().velocity = myShipRigidbody2D.velocity;
        projectile.GetComponent<NetworkObject>().Spawn();
    }


    //отрисовка позиций из которых вылетают снаряды для редактора
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector2 myPosition = transform.position;
            foreach (Vector2 barrelPos in barrelsPositions)
            {
                Gizmos.DrawIcon(myPosition + barrelPos, "Aim icon.png", false);
            }
        }
    }
}
