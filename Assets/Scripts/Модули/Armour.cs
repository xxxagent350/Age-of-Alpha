using UnityEngine;
using System;

public class Armour : MonoBehaviour
{
    [Header("Настройка")]
    public float maxHP = 100;
    public float resistanceToFireDamage = 0; //устойчивсть к тепловому урону
    public float resistanceToEnergyDamage = 0; //устойчивсть к энерго урону
    public float resistanceToPhysicalDamage = 0; //устойчивсть к физическому урону

    [Header("Отладка")]
    [SerializeField] float HP;

    private void Start()
    {
        HP = maxHP;
        CheckResistancesLimits();
    }

    public void Damage(float damage, DamageTypes damageType)
    {
        CheckResistancesLimits();
        float damageWithResistance = damage;
        if (damageType == DamageTypes.fire)
            damageWithResistance = damage * (1 - resistanceToFireDamage);
        if (damageType == DamageTypes.energy)
            damageWithResistance = damage * (1 - resistanceToEnergyDamage);
        if (damageType == DamageTypes.physical)
            damageWithResistance = damage * (1 - resistanceToPhysicalDamage);

        HP -= damageWithResistance;
        if (HP <= 0)
        {
            HP = 0;
            Destroy(gameObject);
        }
    }

    void CheckResistancesLimits()
    {
        if (resistanceToFireDamage < 0)
        {
            Debug.LogWarning("Сопротивление к тепловому урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToFireDamage + " на объекте " + gameObject.name);
            resistanceToFireDamage = 0;
        }
        if (resistanceToFireDamage > 1)
        {
            Debug.LogWarning("Сопротивление к тепловому урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToFireDamage + " на объекте " + gameObject.name);
            resistanceToFireDamage = 1;
        }

        if (resistanceToEnergyDamage < 0)
        {
            Debug.LogWarning("Сопротивление к энерго урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToEnergyDamage + " на объекте " + gameObject.name);
            resistanceToEnergyDamage = 0;
        }
        if (resistanceToEnergyDamage > 1)
        {
            Debug.LogWarning("Сопротивление к энерго урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToEnergyDamage + " на объекте " + gameObject.name);
            resistanceToEnergyDamage = 1;
        }

        if (resistanceToPhysicalDamage < 0)
        {
            Debug.LogWarning("Сопротивление к физическому урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToPhysicalDamage + " на объекте " + gameObject.name);
            resistanceToPhysicalDamage = 0;
        }
        if (resistanceToPhysicalDamage > 1)
        {
            Debug.LogWarning("Сопротивление к физическому урону должно находиться в рамках (0 - 1), а оно составляло " + resistanceToPhysicalDamage + " на объекте " + gameObject.name);
            resistanceToPhysicalDamage = 1;
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