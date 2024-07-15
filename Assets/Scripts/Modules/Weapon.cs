using System.Collections;
using Unity.Netcode;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Отладка(менять не нужно)")]
    [Tooltip("Ведётся ли стрельба из орудия")]
    public bool FIRE;
    [Tooltip("В рабочем ли состоянии орудие")]
    public bool isWorking = true;
    [Tooltip("Номер орудия (например если игрок хочет выстрелить из орудий обозначенных номером 2, то стреляют орудия у которых этот параметр равен 2), выставляется автоматически")]
    public uint weaponNum;

    [SerializeField] protected float _сooldown;
    protected float _сurrentReloadTime = 0;

    [HideInInspector] public ShipGameStats myShipGameStats;
    [HideInInspector] public string teamID;
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
        teamID = myShipGameStats.TeamID.Value.GetString();
        RandomUpdate();
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
            if (!noControl)
            {
                FixedServerUpdate();
            }
        }
    }

    public void RandomUpdate()
    {
        if (!noControl)
        {
            RandomizedServerUpdate();
        }
        Invoke(nameof(RandomUpdate), Random.Range(serverUpdateDeltaTime * 0.5f, serverUpdateDeltaTime * 1.5f));
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
}
