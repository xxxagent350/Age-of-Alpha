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
    [HideInInspector] public string teamID;
    float serverUpdateDeltaTime;

    public void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            serverUpdateDeltaTime = Time.fixedDeltaTime;
            myShipGameStats = GetComponentInParent<ShipGameStats>();
            myShipGameStats.attackButtonStateChangedMessage += ChangeFiringState;
            teamID = myShipGameStats.teamID;
            RandomUpdate();
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

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            FixedServerUpdate();
        }
    }

    public void RandomUpdate()
    {
        RandomizedServerUpdate();
        Invoke(nameof(RandomUpdate), Random.Range(serverUpdateDeltaTime * 0.5f, serverUpdateDeltaTime * 1.5f));
    }

    public virtual void Initialize()
    {
        //��� ��������������� ������������ ��������
    }

    public virtual void RandomizedServerUpdate()
    {
        //��� ��������������� ������������ ��������
    }

    public virtual void FixedServerUpdate()
    {
        //��� ��������������� ������������ ��������
    }
}
