using UnityEngine;
using Unity.Netcode;
using System;

public class Durability: MonoBehaviour
{
    [Header("Настройка")]
    [Tooltip("Прочность")]
    public DurabilityStruct durability;
    
    [Header("Отладка")]
    public string teamID = "none";

    private void Start()
    {
#if UNITY_EDITOR
        durability.CheckResistancesLimits();
#endif

        if (NetworkManager.Singleton.IsServer)
        {
            durability.SetMaxDurability();
        }
    }

    private void FixedUpdate()
    {
        if (durability.NoDurability())
        {
            Explode();
        }
    }

    bool alreadyExploded = false;
    private void Explode()
    {
        if (!alreadyExploded)
        {
            alreadyExploded = true;
            //нужно сказать кораблю что его характеристики просели
            Destroy(gameObject);
        }
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
    bool allDamageUsed = false;

    public bool AllDamageUsed()
    {
        return allDamageUsed;
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
    public float resistanceToFireDamage;
    [Tooltip("Устойчивость к энерго урону(0 == без защиты от энерго урона, 1 == неуязвим для энерго урона)")]
    public float resistanceToEnergyDamage;
    [Tooltip("Устойчивость к физическому урону(0 == без защиты от физического урона, 1 == неуязвим для физического урона)")]
    public float resistanceToPhysicalDamage;

    [Tooltip("Текущая прочность (не трогать)")]
    public float currentDurability;

    bool noDurability;

    public bool NoDurability()
    {
        return noDurability;
    }

    public void SetMaxDurability()
    {
        currentDurability = maxDurability;
    }

    public void TakeDamage(Damage damage)
    {   //также отнимает принятый урон у переданного класса Damage
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
            noDurability = true;
            currentDurability = 0;
            float usedDamagePart = currentDurability / fullDamage;
            damage.UseDamage(damage.fireDamage * usedDamagePart, damage.energyDamage * usedDamagePart, damage.physicalDamage * usedDamagePart);
        }
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