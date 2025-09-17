using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Durability : MonoBehaviour
{
    [Header("Параметры прочности")]
    [Tooltip("Структура прочности")]
    public DurabilityStruct durability;

    [SerializeField] private List<string> _moduleDestroyEffects;

    [Tooltip("Эффекты при уничтожении модуля, которые будут созданы на корабле. Например, разрушенный модуль пушек может заспавнить эффект дыма прямо на корабле.")]
    [SerializeField] private List<string> _moduleDestroyOnShipEffects;

    [Tooltip("Сила взрыва при уничтожении модуля. Например, если параметр равен 5, то модуль при смерти создаст ударную волну силы 5, которая повредит всё вокруг.")]
    public float ExplodeStrengthOnDestroy = 0;

    [Header("Команда")]
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
            OnModuleDurabilityRatioChangedEvent?.Invoke(durabilityToMaxDurability, _cellsLocalPositionsInShip);
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
            Debug.LogError($"{gameObject}: Durability.Explode() вызван на клиенте, а не на сервере!");
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
                MyShipsGameStats.ReduceShipCharacteristics(this);
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
        RpcHandlerForEffects.SpawnEffectsOnClients(
            _moduleDestroyEffects,
            transform.position,
            Quaternion.identity,
            _shipsRigidbody2D.linearVelocity
        );
    }

    private void SpawnOnShipModuleDestroyEffects()
    {
        _destroyedModulesEffectsSpawner.SpawnAndAttachEffects(
            _moduleDestroyOnShipEffects,
            transform.localPosition,
            Quaternion.identity
        );
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
    [Tooltip("Урон огнём")]
    public float fireDamage;

    [Tooltip("Урон энергией")]
    public float energyDamage;

    [Tooltip("Физический урон")]
    public float physicalDamage;

    [Tooltip("true если весь урон уже использован")]
    private bool allDamageUsed = false;

    public Damage(float fireDamage_, float energyDamage_, float physicalDamage_)
    {
        fireDamage = fireDamage_;
        energyDamage = energyDamage_;
        physicalDamage = physicalDamage_;
    }

    public bool AllDamageUsed() => allDamageUsed;

    public float GetAllDamage() => fireDamage + energyDamage + physicalDamage;

    public void UseDamage(float usedFireDamage, float usedEnergyDamage, float usedPhysicalDamage)
    {
        // Списываем урон
        fireDamage -= usedFireDamage;
        energyDamage -= usedEnergyDamage;
        physicalDamage -= usedPhysicalDamage;

        if (fireDamage < -0.01f) Debug.LogError("Огненный урон ушёл меньше 0!");
        if (energyDamage < -0.01f) Debug.LogError("Энергетический урон ушёл меньше 0!");
        if (physicalDamage < -0.01f) Debug.LogError("Физический урон ушёл меньше 0!");

        if (fireDamage < 0.01f && energyDamage < 0.01f && physicalDamage < 0.01f)
        {
            allDamageUsed = true;
        }
    }

    public void DamageOtherDamage(Damage otherDamage)
    {
        // Взаимодействие между двумя уронами
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

    [Tooltip("Сопротивление огненному урону (0 == не защищён, 1 == полностью защищён)")]
    [Range(0f, 1f)] public float resistanceToFireDamage;

    [Tooltip("Сопротивление энергетическому урону (0 == не защищён, 1 == полностью защищён)")]
    [Range(0f, 1f)] public float resistanceToEnergyDamage;

    [Tooltip("Сопротивление физическому урону (0 == не защищён, 1 == полностью защищён)")]
    [Range(0f, 1f)] public float resistanceToPhysicalDamage;

    [Tooltip("Текущая прочность (не больше maxDurability)")]
    public float currentDurability;
    private bool noDurability;

    public delegate void OnDurabilityRatioChangedEventContainer(float durabilityToMaxDurabilityRatio);
    public event OnDurabilityRatioChangedEventContainer OnDurabilityRatioChangedEvent;

    public delegate void OnNoDurabilityContainer();
    public event OnNoDurabilityContainer OnNoDurability;

    public bool NoDurability() => noDurability;

    public void SetMaxDurability() => currentDurability = maxDurability;

    public void TakeDamage(Damage damage)
    {
        // Считаем итоговый урон с учётом сопротивлений
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
        OnDurabilityRatioChangedEvent?.Invoke(currentDurability / maxDurability);
    }

    public void CheckResistancesLimits()
    {
        if (resistanceToFireDamage < 0 || resistanceToFireDamage > 1)
        {
            Debug.LogWarning($"Сопротивление огню должно быть в диапазоне 0-1. Получено: {resistanceToFireDamage}");
            resistanceToFireDamage = Mathf.Clamp01(resistanceToFireDamage);
        }

        if (resistanceToEnergyDamage < 0 || resistanceToEnergyDamage > 1)
        {
            Debug.LogWarning($"Сопротивление энергии должно быть в диапазоне 0-1. Получено: {resistanceToEnergyDamage}");
            resistanceToEnergyDamage = Mathf.Clamp01(resistanceToEnergyDamage);
        }

        if (resistanceToPhysicalDamage < 0 || resistanceToPhysicalDamage > 1)
        {
            Debug.LogWarning($"Сопротивление физическому урону должно быть в диапазоне 0-1. Получено: {resistanceToPhysicalDamage}");
            resistanceToPhysicalDamage = Mathf.Clamp01(resistanceToPhysicalDamage);
        }
    }
}
