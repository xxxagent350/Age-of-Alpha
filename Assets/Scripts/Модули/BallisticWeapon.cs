using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BallisticWeapon : Weapon
{
    [Header("���������")]
    [Tooltip("������ ����")]
    [SerializeField] GameObject projectilePrefab;
    [Tooltip("������� �� ������� �� ������� ����� �������� ����(���������� ������� ������� � ���������)")]
    [SerializeField] List<Vector2> barrelsPositions;
    [Tooltip("����� ����� ���������� � �������� (�������� �� ����������� �������� ����������� ������ �������� ���������� FixedUpdate, ������� ����� ����� �������� ������� �� ����� ������)")]
    [SerializeField] float reloadTime = 0.5f;
    [Tooltip("���������� �������� ����������� �� 1 ����")]
    [SerializeField] int projectilesPerSalvo = 1;
    [Tooltip("������� � ��������")]
    [SerializeField] float scatterAngle = 5;

    float reloadTimer;
    int currentBarrelNum;

    public override void Initialize()
    {
#if UNITY_EDITOR
        if (barrelsPositions.Count == 0)
        {
            Debug.LogWarning($"�������� ���� �� ���� ������� ������ �������� � barrelsPositions ��� {gameObject.name}");
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
            Salvo();
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
        GameObject projectile = Instantiate(projectilePrefab, position, transform.rotation);
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
