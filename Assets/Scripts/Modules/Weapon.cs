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
    public uint weaponNum;

    [SerializeField] protected float _�ooldown;
    protected float _�urrentReloadTime = 0;

    [HideInInspector] public ShipGameStats myShipGameStats;
    public string teamID { get; private set; }
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
        teamID = myShipGameStats.TeamID.Value.String;
    }

    public void Disconnect()
    {
        noControl = true;
    }

    public void Reload()
    {
        if (_�urrentReloadTime < _�ooldown)
            _�urrentReloadTime += Time.deltaTime;
    }

    public void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            myShipGameStats.attackButtonStateChangedMessage -= ChangeFiringState;
        }
        if (NetworkManager.Singleton.IsServer)
        {
            OnDestroyServer();
        }
    }

    public void ChangeFiringState(uint index, bool fire)
    {
        if (index == weaponNum)
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
            yield return new WaitForSeconds(Random.Range(serverUpdateDeltaTime * 0.5f, serverUpdateDeltaTime * 1.5f));
            if (NetworkManager.Singleton.IsServer)
            {
                RandomizedServerUpdate();
            }
        }
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

    public virtual void OnDestroyServer()
    {
        
    }
}
