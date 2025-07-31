using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Durability : MonoBehaviour
{
    [Header("���������")]
    [Tooltip("���������")]
    public DurabilityStruct durability;
    [SerializeField] private List<string> _moduleDestroyEffects;
    [Tooltip("�������� �������� ����������� ������, ������� ����� ����������� � �������. ��������, ��������������� ������ ���� � ���� ����� ������ ����-�� ������������")]
    [SerializeField] private List<string> _moduleDestroyOnShipEffects;
    [Tooltip("���� ������ ��� ����������� ������. ��������, ���� ���������� �������� 5, �� ������ � ������� 1 ������ �� ������ ������� ���� 5, � ������ ������������� �������� ������� ������� ����. ������� ������ ����� ���������� ������� �����")]
    public float ExplodeStrengthOnDestroy = 0;

    [Header("�������")]
    public string TeamID = "none";
    [SerializeField] private Vector2Serializable[] _cellsLocalPositionsInShip;

    public ShipGameStats MyShipsGameStats;
    private Rigidbody2D _shipsRigidbody2D;
    private AttachedToShipEffectsSpawner _destroyedModulesEffectsSpawner;
    private Collider2D[] _colliders2D;

    private void Start()
    {
#if UNITY_EDITOR
        durability.CheckResistancesLimits();
#endif
        if (NetworkManager.Singleton.IsServer)
        {
            _colliders2D = GetComponents<Collider2D>();
        }
    }

    public void InitializeForModule(CellData[] cellsDatas, Vector2 cellsOffset)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SetCellsLocalPositionsInShip(cellsDatas, cellsOffset);
            MyShipsGameStats = GetComponentInParent<ShipGameStats>();
            _shipsRigidbody2D = GetComponentInParent<Rigidbody2D>();
            _destroyedModulesEffectsSpawner = GetComponentInParent<AttachedToShipEffectsSpawner>();
            durability.SetMaxDurability();
            OnModuleDurabilityRatioChangedEvent += GetComponentInParent<ModulesCellsDurabilityShower>().OnHealthCellDurabilityChangedRpc;
            durability.OnDurabilityRatioChangedEvent += SendDurabilityChanged;
            durability.OnNoDurability += Explode;
        }
    }
    /*
    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            OnModuleDurabilityRatioChangedEvent -= GetComponentInParent<ModulesCellsDurabilityShower>().OnHealthCellDurabilityChangedRpc;
            durability.OnDurabilityRatioChanged -= SendDurabilityChanged;
            durability.OnNoDurability -= Explode;
        }
    }
    */
    private delegate void OnModuleDurabilityRatioChangedContainer(float durabilityToMaxDurability, Vector2Serializable[] cellsLocalPositionsInShip);

    private event OnModuleDurabilityRatioChangedContainer OnModuleDurabilityRatioChangedEvent;

    public void SendDurabilityChanged(float durabilityToMaxDurability)
    {
        if (MyShipsGameStats.Destroyed.Value == false)
        {
            OnModuleDurabilityRatioChangedEvent(durabilityToMaxDurability, _cellsLocalPositionsInShip);
        }
    }

    public void SetCellsLocalPositionsInShip(CellData[] cellsDatas, Vector2 cellsOffset)
    {
        _cellsLocalPositionsInShip = new Vector2Serializable[cellsDatas.Length];
        for (int cellNum = 0; cellNum < cellsDatas.Length; cellNum++)
        {
            Vector2 cellLocalPosition = (Vector2)transform.localPosition + cellsOffset + cellsDatas[cellNum].position;
            Vector2Serializable cellLocalPositionSerializable = new(cellLocalPosition);
            _cellsLocalPositionsInShip[cellNum] = cellLocalPositionSerializable;
        }
    }

    public bool AlreadyExploded { get; private set; } = false;

    public void Explode()
    {
        if (NetworkManager.Singleton.IsServer == false)
        {
            Debug.LogError($"{gameObject}: Durability.Explode() ��� ������ �� �� �������, ���� ���� �� ������");
        }

        if (!AlreadyExploded)
        {
            AlreadyExploded = true;
            durability.currentDurability = 0;
            durability.SendOnDurabilityRatioChangedEvent();
            DisableModuleColliders();
            if (ExplodeStrengthOnDestroy > 0)
            {
                ShockWave.CreateShockWave(ExplodeStrengthOnDestroy, transform.position);
            }
            SpawnModuleDestroyEffects();
            if (_moduleDestroyOnShipEffects.Count > 0)
            {
                SpawnOnShipModuleDestroyEffects();
            }

            if (MyShipsGameStats != null)
            {
                MyShipsGameStats.ReduceShip�haracteristics(this);
            }
            Destroy(gameObject);
        }
    }

    private void DisableModuleColliders()
    {
        foreach (Collider2D collider2D in _colliders2D)
        {
            collider2D.enabled = false;
        }
    }

    private void SpawnModuleDestroyEffects()
    {
        RpcHandlerForEffects.SpawnEffectsOnClients(_moduleDestroyEffects, transform.position, Quaternion.identity, _shipsRigidbody2D.linearVelocity);
    }

    private void SpawnOnShipModuleDestroyEffects()
    {
        _destroyedModulesEffectsSpawner.SpawnAndAttachEffects(_moduleDestroyOnShipEffects, transform.localPosition, Quaternion.identity);
    }
}

public enum DamageTypes
{
    fire,
    energy,
    physical,
    special
}

[Serializable]
public class Damage
{
    [Tooltip("�������� ����")]
    public float fireDamage;
    [Tooltip("�������������� ����")]
    public float energyDamage;
    [Tooltip("���������� ����")]
    public float physicalDamage;

    [Tooltip("true ����� ���� ���� ������������")]
    private bool allDamageUsed = false;

    public Damage(float fireDamage_, float energyDamage_, float physicalDamage_)
    {
        fireDamage = fireDamage_;
        energyDamage = energyDamage_;
        physicalDamage = physicalDamage_;
    }

    public bool AllDamageUsed()
    {
        return allDamageUsed;
    }

    public float GetAllDamage()
    {
        return fireDamage + energyDamage + physicalDamage;
    }

    public void UseDamage(float usedFireDamage, float usedEnergyDamage, float usedPhysicalDamage)
    {   //�������� ����
        fireDamage -= usedFireDamage;
        energyDamage -= usedEnergyDamage;
        physicalDamage -= usedPhysicalDamage;

        if (fireDamage < -0.01f)
        {
            Debug.LogError("�������� ���� ������� ����� 0!");
        }
        if (energyDamage < -0.01f)
        {
            Debug.LogError("�������������� ���� ������� ����� 0!");
        }
        if (physicalDamage < -0.01f)
        {
            Debug.LogError("���������� ���� ������� ����� 0!");
        }

        if (fireDamage < 0.01f && energyDamage < 0.01f && physicalDamage < 0.01f)
        {
            allDamageUsed = true;
        }
    }

    public void DamageOtherDamage(Damage otherDamage)
    {   //��� ������������ ���� ��������
        float myFullDamage = fireDamage + energyDamage + physicalDamage;
        float otherFullDamage = otherDamage.fireDamage + otherDamage.energyDamage + otherDamage.physicalDamage;

        if (myFullDamage > otherFullDamage)
        {
            float usedDamagePart = otherFullDamage / myFullDamage;
            otherDamage.UseDamage(otherDamage.fireDamage, otherDamage.energyDamage, otherDamage.physicalDamage);
            UseDamage(fireDamage * usedDamagePart, energyDamage * usedDamagePart, physicalDamage * usedDamagePart);
        }
        else
        {
            float usedDamagePart = myFullDamage / otherFullDamage;
            UseDamage(fireDamage, energyDamage, physicalDamage);
            otherDamage.UseDamage(otherDamage.fireDamage * usedDamagePart, otherDamage.energyDamage * usedDamagePart, otherDamage.physicalDamage * usedDamagePart);
        }
    }
}

[Serializable]
public struct DurabilityStruct
{
    [Tooltip("������������ ���������")]
    public float maxDurability;

    [Tooltip("������������ � ��������� �����(0 == ��� ������ �� ��������� �����, 1 == �������� ��� ��������� �����)")]
    [Range(0f, 1f)] public float resistanceToFireDamage;
    [Tooltip("������������ � ������ �����(0 == ��� ������ �� ������ �����, 1 == �������� ��� ������ �����)")]
    [Range(0f, 1f)] public float resistanceToEnergyDamage;
    [Tooltip("������������ � ����������� �����(0 == ��� ������ �� ����������� �����, 1 == �������� ��� ����������� �����)")]
    [Range(0f, 1f)] public float resistanceToPhysicalDamage;

    [Tooltip("������� ��������� (�� �������)")]
    public float currentDurability;
    private bool noDurability;

    public delegate void OnDurabilityRatioChangedEventContainer(float durabilityToMaxDurabilityRatio);
    public event OnDurabilityRatioChangedEventContainer OnDurabilityRatioChangedEvent;

    public delegate void OnNoDurabilityContainer();
    public event OnNoDurabilityContainer OnNoDurability;

    public bool NoDurability()
    {
        return noDurability;
    }

    public void SetMaxDurability()
    {
        currentDurability = maxDurability;
    }

    public void TakeDamage(Damage damage)
    {
        //����� �������� �������� ���� � ����������� ������ Damage
        float fullDamage = (damage.fireDamage * (1 - resistanceToFireDamage))
            + (damage.energyDamage * (1 - resistanceToEnergyDamage))
            + (damage.physicalDamage * (1 - resistanceToPhysicalDamage));

        if (currentDurability > fullDamage)
        {
            currentDurability -= fullDamage;
            damage.UseDamage(damage.fireDamage, damage.energyDamage, damage.physicalDamage);
        }
        else
        {
            float usedDamagePart = currentDurability / fullDamage;
            damage.UseDamage(damage.fireDamage * usedDamagePart, damage.energyDamage * usedDamagePart, damage.physicalDamage * usedDamagePart);
            noDurability = true;
            currentDurability = 0;
            OnNoDurability?.Invoke();
        }
        SendOnDurabilityRatioChangedEvent();
    }

    public void SendOnDurabilityRatioChangedEvent()
    {
        OnDurabilityRatioChangedEvent(currentDurability / maxDurability);
    }

    public void CheckResistancesLimits()
    {
        if (resistanceToFireDamage < 0)
        {
            Debug.LogWarning("������������� � ��������� ����� ������ ���������� � ������ (0 - 1), � ��� ���������� " + resistanceToFireDamage);
            resistanceToFireDamage = 0;
        }
        if (resistanceToFireDamage > 1)
        {
            Debug.LogWarning("������������� � ��������� ����� ������ ���������� � ������ (0 - 1), � ��� ���������� " + resistanceToFireDamage);
            resistanceToFireDamage = 1;
        }

        if (resistanceToEnergyDamage < 0)
        {
            Debug.LogWarning("������������� � ������ ����� ������ ���������� � ������ (0 - 1), � ��� ���������� " + resistanceToEnergyDamage);
            resistanceToEnergyDamage = 0;
        }
        if (resistanceToEnergyDamage > 1)
        {
            Debug.LogWarning("������������� � ������ ����� ������ ���������� � ������ (0 - 1), � ��� ���������� " + resistanceToEnergyDamage);
            resistanceToEnergyDamage = 1;
        }

        if (resistanceToPhysicalDamage < 0)
        {
            Debug.LogWarning("������������� � ����������� ����� ������ ���������� � ������ (0 - 1), � ��� ���������� " + resistanceToPhysicalDamage);
            resistanceToPhysicalDamage = 0;
        }
        if (resistanceToPhysicalDamage > 1)
        {
            Debug.LogWarning("������������� � ����������� ����� ������ ���������� � ������ (0 - 1), � ��� ���������� " + resistanceToPhysicalDamage);
            resistanceToPhysicalDamage = 1;
        }
    }
}