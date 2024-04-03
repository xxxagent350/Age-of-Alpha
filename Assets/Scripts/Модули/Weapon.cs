using Unity.Netcode;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("�������(������ �� �����)")]
    [Tooltip("������ �� �������� �� ������")]
    public bool FIRE;
    [Tooltip("� ������� �� ��������� ������")]
    public bool isWorking = true;
    [Tooltip("����� ������ (�������� ���� ����� ����� ���������� �� ������ ������������ ������� 2, �� �������� ������ � ������� ���� �������� ����� 2), ������������ �������������")]
    public uint weaponNum;

    ShipGameStats myShipGameStats;

    public void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            myShipGameStats = GetComponentInParent<ShipGameStats>();
            myShipGameStats.attackButtonStateChangedMessage += ChangeFiringState;
        }
        Initialize();
    }

    public void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            myShipGameStats.attackButtonStateChangedMessage -= ChangeFiringState;
        }
    }

    public void ChangeFiringState(uint index, bool fire)
    {
        if (index == weaponNum)
        {
            FIRE = fire;
        }
    }

    public void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ServerUpdate();
        } 
    }

    public virtual void Initialize()
    {
        //��� ��������������� ������������ ��������
    }

    public virtual void ServerUpdate()
    {
        //��� ��������������� ������������ ��������
    }
}
