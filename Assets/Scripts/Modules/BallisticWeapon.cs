using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BallisticWeapon : Weapon
{
    [Header("���������")]
    [Tooltip("������ ����")]
    public GameObject ProjectilePrefab;
    [Tooltip("����� ����������� ����� ������� � ��������")]
    public float ReloadTime = 1;
    [Tooltip("����������������� �� 1 ����")]
    public float EnergyConsumption = 1;
    [Tooltip("������� �� ������� �� ������� ����� �������� ����(���������� ������� ������� � ���������)")]
    public List<Vector2> BarrelsPositions;
    [Tooltip("����� ����� ���������� � ��������")]
    public int ProjectilesPerSalvo = 1;
    [Tooltip("������� � ��������")]
    public float ScatterAngle = 5;

    [Tooltip("������� � ����� ��������")]
    [SerializeField] List<string> shootEffectsNames;

    private AttachedToShipEffectsSpawner _destroyedModulesEffectsSpawner;
    private int currentBarrelNum;
    private Rigidbody2D myShipRigidbody2D;
    private float _�urrentReloadTime = 0;
    private ModulesVisualReloadStateSynchronizer _modulesVisualReloadStateSynchronizer;

    public override void Initialize()
    {
#if UNITY_EDITOR
        if (BarrelsPositions.Count == 0)
        {
            Debug.LogWarning($"�������� ���� �� ���� ������� ������ �������� � barrelsPositions ��� {gameObject.name}");
            isWorking = false;
        }
#endif
        if (NetworkManager.Singleton.IsServer)
        {
            myShipRigidbody2D = myShipGameStats.GetComponent<Rigidbody2D>();
            _destroyedModulesEffectsSpawner = GetComponentInParent<AttachedToShipEffectsSpawner>();
            _modulesVisualReloadStateSynchronizer = myShipGameStats.GetComponent<ModulesVisualReloadStateSynchronizer>();
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
        if (isWorking)
        {
            if (isFiring)
            {
                Fire();
            }
        }
    }

    //�������� �� �����������
    public void Reload()
    {
        if (_�urrentReloadTime < ReloadTime)
        {
            _�urrentReloadTime += Time.deltaTime;
            if (_�urrentReloadTime > ReloadTime)
            {
                _�urrentReloadTime = ReloadTime;
            }
            _modulesVisualReloadStateSynchronizer.AddRechargeProgressData(WeaponIndex, _�urrentReloadTime / ReloadTime);
        }
    }

    //�������� �� ��������
    public void Fire()
    {
        if (_�urrentReloadTime >= ReloadTime)
        {
            if (myShipGameStats.TrySpendEnergy(EnergyConsumption))
            {
                _�urrentReloadTime = 0;

                Vector2 shotPoint = (Vector2)transform.position + DataOperator.RotateVector2(BarrelsPositions[currentBarrelNum], transform.eulerAngles.z);
                Vector2 localShotPoint = (Vector2)transform.localPosition + BarrelsPositions[currentBarrelNum];
                Salvo(shotPoint);
                SpawnEffects(localShotPoint);
            }
        }
    }

    private void SpawnEffects(Vector2 localShotPoint)
    {
        _destroyedModulesEffectsSpawner.SpawnAndAttachEffects(shootEffectsNames, localShotPoint, Quaternion.identity);
    }

    //����
    void Salvo(Vector2 shotPoint)
    {
        for (int projectileNum = 0; projectileNum < ProjectilesPerSalvo; projectileNum++)
        {
            SpawnProjectile(shotPoint);
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

    void SpawnProjectile(Vector2 shotPoint)
    {
        Quaternion rotation = transform.rotation * Quaternion.Euler(0, 0, Random.Range(-ScatterAngle, ScatterAngle));
        GameObject projectile = Instantiate(ProjectilePrefab, shotPoint, rotation);
        projectile.GetComponent<Rigidbody2D>().linearVelocity = myShipRigidbody2D.linearVelocity;

        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        projectileComponent.ApplyImpulseToParentShip(myShipRigidbody2D);

        projectile.GetComponent<NetworkObject>().Spawn();
        projectileComponent.TeamID.Value = new NetworkString(TeamID);
    }

    //��������� ������� �� ������� �������� ������� ��� ���������
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector2 myPosition = transform.position;
            foreach (Vector2 barrelPos in BarrelsPositions)
            {
                Gizmos.DrawIcon(myPosition + barrelPos, "Aim icon.png", false);
            }
        }
    }
}
