using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Отладка(менять не нужно)")]
    [Tooltip("Ведётся ли стрельба из орудия")]
    public bool isFiring;
    [Tooltip("В рабочем ли состоянии орудие")]
    public bool isWorking = true;
    [Tooltip("Номер орудия (например если игрок хочет выстрелить из орудий обозначенных номером 2, то стреляют орудия у которых этот параметр равен 2), выставляется автоматически")]
    public uint weaponNum;

    [SerializeField] protected float _сooldown;
    protected float _сurrentReloadTime = 0;

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
        if (_сurrentReloadTime < _сooldown)
            _сurrentReloadTime += Time.deltaTime;
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
        //для переопределения наследуемыми классами
    }

    public virtual void RandomizedServerUpdate()
    {
        //для переопределения наследуемыми классами
    }

    public virtual void FixedServerUpdate()
    {
        //для переопределения наследуемыми классами
    }

    public virtual void OnDestroyServer()
    {
        
    }
}
