using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Durability : MonoBehaviour
{
    [Header("Настройка")]
    [Tooltip("Прочность")]
    public DurabilityStruct durability;
    [SerializeField] private List<string> _moduleDestroyEffects;
    [Tooltip("Названия эффектов уничтожения модуля, которые будут прикреплены к кораблю. Например, продолжительный эффект огня и дыма после взрыва чего-то огнеопасного")]
    [SerializeField] private List<string> _moduleDestroyOnShipEffects;
    [Tooltip("Сила взрыва при уничтожении модуля. Например, если установить значение 5, то модули в радиусе 1 ячейки от взрыва получат урон 5, а модули расположенные подальше получат меньший урон. Прочные модули могут остановить ударную волну")]
    [SerializeField] private float _explodeStrengthOnDestroy = 0;

    [Header("Отладка")]
    public string teamID = "none";
    [SerializeField] private Vector2Serializable[] _cellsLocalPositionsInShip;

    private Rigidbody2D _shipsRigidbody2D;
    private ShipGameStats _myShipsGameStats;
    private DestroyedModulesEffectsSpawner _destroyedModulesEffectsSpawner;
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
            _myShipsGameStats = GetComponentInParent<ShipGameStats>();
            _shipsRigidbody2D = GetComponentInParent<Rigidbody2D>();
            _destroyedModulesEffectsSpawner = GetComponentInParent<DestroyedModulesEffectsSpawner>();
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
        OnModuleDurabilityRatioChangedEvent(durabilityToMaxDurability, _cellsLocalPositionsInShip);
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
            Debug.LogError($"{gameObject}: Durability.Explode() был вызван не на сервере, чего быть не должно");
        }

        if (!AlreadyExploded)
        {
            AlreadyExploded = true;
            durability.currentDurability = 0;
            durability.SendOnDurabilityRatioChangedEvent();
            DisableModuleColliders();
            if (_explodeStrengthOnDestroy > 0)
            {
                ShockWave.CreateShockWave(_explodeStrengthOnDestroy, transform.position);
            }
            SpawnModuleDestroyEffects();
            if (_moduleDestroyOnShipEffects.Count > 0)
            {
                SpawnOnShipModuleDestroyEffects();
            }

            if (_myShipsGameStats != null)
            {
                _myShipsGameStats.ReduceShipСharacteristics(this);
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
        RpcHandlerForEffects.SpawnEffectsOnClients(_moduleDestroyEffects, transform.position, Quaternion.identity, _shipsRigidbody2D.velocity);
    }

    private void SpawnOnShipModuleDestroyEffects()
    {
        NetworkString[] effectsNamesNetworkStringArray = new NetworkString[_moduleDestroyOnShipEffects.Count];
        for (int numInList = 0; numInList < _moduleDestroyOnShipEffects.Count; numInList++)
        {
            effectsNamesNetworkStringArray[numInList] = new NetworkString(_moduleDestroyOnShipEffects[numInList]);
        }
        _destroyedModulesEffectsSpawner.SpawnAndAttachDestroyedModuleEffectRpc(effectsNamesNetworkStringArray, transform.localPosition);
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
    [Tooltip("Тепловой урон")]
    public float fireDamage;
    [Tooltip("Энергетический урон")]
    public float energyDamage;
    [Tooltip("Физический урон")]
    public float physicalDamage;

    [Tooltip("true когда весь урон израсходован")]
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
    {   //отнимает урон
        fireDamage -= usedFireDamage;
        energyDamage -= usedEnergyDamage;
        physicalDamage -= usedPhysicalDamage;

        if (fireDamage < -0.01f)
        {
            Debug.LogError("Тепловой урон снаряда менее 0!");
        }
        if (energyDamage < -0.01f)
        {
            Debug.LogError("Энергетический урон снаряда менее 0!");
        }
        if (physicalDamage < -0.01f)
        {
            Debug.LogError("Физический урон снаряда менее 0!");
        }

        if (fireDamage < 0.01f && energyDamage < 0.01f && physicalDamage < 0.01f)
        {
            allDamageUsed = true;
        }
    }

    public void DamageOtherDamage(Damage otherDamage)
    {   //для столкновения двух снарядов
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
    [Tooltip("Максимальная прочность")]
    public float maxDurability;

    [Tooltip("Устойчивость к тепловому урону(0 == без защиты от теплового урона, 1 == неуязвим для теплового урона)")]
    [Range(0f, 1f)] public float resistanceToFireDamage;
    [Tooltip("Устойчивость к энерго урону(0 == без защиты от энерго урона, 1 == неуязвим для энерго урона)")]
    [Range(0f, 1f)] public float resistanceToEnergyDamage;
    [Tooltip("Устойчивость к физическому урону(0 == без защиты от физического урона, 1 == неуязвим для физического урона)")]
    [Range(0f, 1f)] public float resistanceToPhysicalDamage;

    [Tooltip("Текущая прочность (не трогать)")]
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
        //также отнимает принятый урон у переданного класса Damage
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
            OnNoDurability();
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
            Debug.LogWarning("Сопротивление к тепловому урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToFireDamage);
            resistanceToFireDamage = 0;
        }
        if (resistanceToFireDamage > 1)
        {
            Debug.LogWarning("Сопротивление к тепловому урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToFireDamage);
            resistanceToFireDamage = 1;
        }

        if (resistanceToEnergyDamage < 0)
        {
            Debug.LogWarning("Сопротивление к энерго урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToEnergyDamage);
            resistanceToEnergyDamage = 0;
        }
        if (resistanceToEnergyDamage > 1)
        {
            Debug.LogWarning("Сопротивление к энерго урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToEnergyDamage);
            resistanceToEnergyDamage = 1;
        }

        if (resistanceToPhysicalDamage < 0)
        {
            Debug.LogWarning("Сопротивление к физическому урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToPhysicalDamage);
            resistanceToPhysicalDamage = 0;
        }
        if (resistanceToPhysicalDamage > 1)
        {
            Debug.LogWarning("Сопротивление к физическому урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToPhysicalDamage);
            resistanceToPhysicalDamage = 1;
        }
    }
}