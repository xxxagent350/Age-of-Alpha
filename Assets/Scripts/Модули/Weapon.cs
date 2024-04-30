using Unity.Netcode;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Отладка(менять не нужно)")]
    [Tooltip("Ведётся ли стрельба из орудия")]
    public bool FIRE;
    [Tooltip("В рабочем ли состоянии орудие")]
    public bool isWorking = true;
    [Tooltip("Номер орудия (например если игрок хочет выстрелить из орудий обозначенных номером 2, то стреляют орудия у которых этот параметр равен 2), выставляется автоматически")]
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
