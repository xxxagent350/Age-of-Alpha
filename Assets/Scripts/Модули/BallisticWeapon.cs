using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BallisticWeapon : Weapon
{
    [Header("���������")]
    [Tooltip("������ ����")]
    [SerializeField] GameObject projectilePrefab;
    [Tooltip("����������������� �� 1 ����")]
    [SerializeField] float energyConsumption = 1;
    [Tooltip("������� �� ������� �� ������� ����� �������� ����(���������� ������� ������� � ���������)")]
    [SerializeField] List<Vector2> barrelsPositions;
    [Tooltip("����� ����� ���������� � ��������")]
    [SerializeField] float reloadTime = 0.5f;
    [Tooltip("���������� �������� ����������� �� 1 ����")]
    [SerializeField] int projectilesPerSalvo = 1;
    [Tooltip("������� � ��������")]
    [SerializeField] float scatterAngle = 5;

    float reloadTimer;
    int currentBarrelNum;
    Rigidbody2D myShipRigidbody2D;

    public override void Initialize()
    {
#if UNITY_EDITOR
        if (barrelsPositions.Count == 0)
        {
            Debug.LogWarning($"�������� ���� �� ���� ������� ������ �������� � barrelsPositions ��� {gameObject.name}");
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

    //�������� �� �����������
    void Reload()
    {
        if (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
        }
    }

    //�������� �� ��������
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

    //����
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


    //��������� ������� �� ������� �������� ������� ��� ���������
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
