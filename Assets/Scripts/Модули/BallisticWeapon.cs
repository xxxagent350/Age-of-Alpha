using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallisticWeapon : Weapon
{
    [Header("Настройка")]
    [Tooltip("Префаб пули")]
    [SerializeField] GameObject projectilePrefab;
    [Tooltip("Позиции из которых по очереди будут вылетать пули(обозначены значком прицела в редакторе)")]
    [SerializeField] List<Vector2> barrelsPositions;
    [Tooltip("Время между выстрелами в секундах (проверка на возможность стрельбы проверяется каждый интервал обновления FixedUpdate, поэтому очень малые величины ставить не имеет смысла)")]
    [SerializeField] float reloadTime = 0.5f;
    [Tooltip("Количество снарядов выпускаемых за 1 залп")]
    [SerializeField] int projectilesPerSalvo = 1;
    [Tooltip("Разброс в градусах")]
    [SerializeField] float scatterAngle = 5;

    float reloadTimer;
    int currentBarrelNum;

    public override void Initialize()
    {
#if UNITY_EDITOR
        if (barrelsPositions.Count == 0)
        {
            Debug.LogWarning($"Добавьте хотя бы одну позицию вылета снарядов в barrelsPositions для {gameObject.name}");
            isWorking = false;
        }
#endif
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
            Salvo();
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
        GameObject projectile = Instantiate(projectilePrefab, position, transform.rotation);
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
