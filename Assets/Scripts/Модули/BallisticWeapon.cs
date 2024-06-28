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
    [SerializeField] int projectilesPerSalvo = 1;
    [Tooltip("������� � ��������")]
    [SerializeField] float scatterAngle = 5;
    [Tooltip("������� � ����� ��������")]
    [SerializeField] List<string> shootEffectsNames;

    private DestroyedModulesEffectsSpawner _destroyedModulesEffectsSpawner;
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
            _destroyedModulesEffectsSpawner = GetComponentInParent<DestroyedModulesEffectsSpawner>();
        }
    }

    public override void FixedServerUpdate()
    {
        if (isWorking)
        {
            Reload();
        }
    }

    public override void RandomizedServerUpdate()
    {
        if (isWorking)
        {
            if (FIRE)
            {
                Fire();
            }
        }
    }

    //�������� �� �����������


    //�������� �� ��������
    public void Fire()
    {
        if (CurrentReloadTime >= Cooldown)
        {
            if (myShipGameStats.TrySpendEnergy(energyConsumption))
            {
                CurrentReloadTime = 0;

                Vector2 shotPoint = (Vector2)transform.position + DataOperator.RotateVector2(barrelsPositions[currentBarrelNum], transform.eulerAngles.z);
                Vector2 localShotPoint = (Vector2)transform.localPosition + barrelsPositions[currentBarrelNum];
                Salvo(shotPoint);
                SpawnEffects(localShotPoint);
            }
        }
    }

    private void SpawnEffects(Vector2 localShotPoint)
    {
        NetworkString[] effectsNamesNetworkStringArray = new NetworkString[shootEffectsNames.Count];
        for (int numInList = 0; numInList < shootEffectsNames.Count; numInList++)
        {
            effectsNamesNetworkStringArray[numInList] = new NetworkString(shootEffectsNames[numInList]);
        }
        _destroyedModulesEffectsSpawner.SpawnAndAttachDestroyedModuleEffectRpc(effectsNamesNetworkStringArray, localShotPoint);
    }

    //����
    void Salvo(Vector2 shotPoint)
    {
        for (int projectileNum = 0; projectileNum < projectilesPerSalvo; projectileNum++)
        {
            SpawnProjectile(shotPoint);
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

    void SpawnProjectile(Vector2 shotPoint)
    {
        Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(-scatterAngle, scatterAngle));
        GameObject projectile = Instantiate(projectilePrefab, shotPoint, rotation);
        projectile.GetComponent<Rigidbody2D>().velocity = myShipRigidbody2D.velocity;

        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        projectileComponent.teamID = teamID;

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
