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

    [HideInInspector] public ShipGameStats myShipGameStats;
    const float serverUpdateDeltaTime = 0.02f;

    public void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            myShipGameStats = GetComponentInParent<ShipGameStats>();
            myShipGameStats.attackButtonStateChangedMessage += ChangeFiringState;
        }
        Initialize();
        RandomUpdate();
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

    public void RandomUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            ServerUpdate();
        }
        Invoke(nameof(RandomUpdate), Random.Range(serverUpdateDeltaTime * 0.5f, serverUpdateDeltaTime * 1.5f));
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
