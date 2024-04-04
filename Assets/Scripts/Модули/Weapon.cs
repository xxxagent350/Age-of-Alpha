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
        //для переопределения наследуемыми классами
    }

    public virtual void ServerUpdate()
    {
        //для переопределения наследуемыми классами
    }
}
