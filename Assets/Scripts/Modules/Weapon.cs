using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("�������(������ �� �����)")]
    [Tooltip("������ �� �������� �� ������")]
    public bool isFiring;
    [Tooltip("� ������� �� ��������� ������")]
    public bool isWorking = true;
    [Tooltip("����� ������ (�������� ���� ����� ����� ���������� �� ������ ������������ ������� 2, �� �������� ������ � ������� ���� �������� ����� 2), ������������ �������������")]
    public uint WeaponIndex;

    [HideInInspector] public ShipGameStats myShipGameStats;
    public string TeamID { get; private set; }
    bool noControl = false;
    float serverUpdateDeltaTime;
    
    public void Start()
    {
        StartCoroutine(OpenServer());
        Initialize();
    }

    public IEnumerator OpenServer()
    {
        while (NetworkManager.Singleton.IsServer == false)
        {
            yield return new WaitForSeconds(0.1f);
        }

        serverUpdateDeltaTime = Time.fixedDeltaTime;
        myShipGameStats = GetComponentInParent<ShipGameStats>();
        myShipGameStats.attackButtonStateChangedMessage += ChangeFiringState;
        TeamID = myShipGameStats.TeamID.Value.String;
    }

    public void Disconnect()
    {
        noControl = true;
    }

    public void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            myShipGameStats.attackButtonStateChangedMessage -= ChangeFiringState;
        }
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            OnDestroyServer();
        }
    }

    public void ChangeFiringState(uint index, bool fire)
    {
        if (index == WeaponIndex)
        {
            isFiring = fire;
        }
    }

    private void FixedUpdate()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if (!noControl)
            {
                FixedServerUpdate();
            }
        }
    }

    private void OnEnable()
    {
        StartCoroutine(RandomServerUpdateCoroutine());
    }

    private IEnumerator RandomServerUpdateCoroutine()
    {
        while (!noControl)
        {
            float deltaTime = Random.Range(serverUpdateDeltaTime * 0.5f, serverUpdateDeltaTime * 1.5f);
            yield return new WaitForSeconds(deltaTime);
            if (NetworkManager.Singleton.IsServer)
            {
                RandomizedServerUpdate(deltaTime);
            }
        }
    }

    public virtual void Initialize()
    {
        //��� ��������������� ������������ ��������
    }

    public virtual void RandomizedServerUpdate(float deltaTime)
    {
        //��� ��������������� ������������ ��������
    }

    public virtual void FixedServerUpdate()
    {
        //��� ��������������� ������������ ��������
    }

    public virtual void OnDestroyServer()
    {
        
    }
}
