using UnityEngine;
using Unity.Netcode;
using System;

public class Durability: MonoBehaviour
{
    [Header("���������")]
    [Tooltip("���������")]
    public DurabilityStruct durability;
    
    [Header("�������")]
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
            //����� ������� ������� ��� ��� �������������� �������
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
    [Tooltip("�������� ����")]
    public float fireDamage;
    [Tooltip("�������������� ����")]
    public float energyDamage;
    [Tooltip("���������� ����")]
    public float physicalDamage;

    [Tooltip("true ����� ���� ���� ������������")]
    bool allDamageUsed = false;

    public bool AllDamageUsed()
    {
        return allDamageUsed;
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
    public float resistanceToFireDamage;
    [Tooltip("������������ � ������ �����(0 == ��� ������ �� ������ �����, 1 == �������� ��� ������ �����)")]
    public float resistanceToEnergyDamage;
    [Tooltip("������������ � ����������� �����(0 == ��� ������ �� ����������� �����, 1 == �������� ��� ����������� �����)")]
    public float resistanceToPhysicalDamage;

    [Tooltip("������� ��������� (�� �������)")]
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
    {   //����� �������� �������� ���� � ����������� ������ Damage
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