using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    [Header("Настройка")]
    [SerializeField] uint playerShipNum; 

    [Header("Отладка")]
    [SerializeField] Ship playerShip;
    [SerializeField] ulong ownerClientID;
    [SerializeField] NetworkVariable<NetworkString> teamID = new NetworkVariable<NetworkString>();

    [Header("Отображается на сервере")]
    [SerializeField] bool movementJoystickPressed;
    [SerializeField] float movementJoystickDirInDegrees;
    [SerializeField] float movementJoystickMagnitude;

    GameObject playerShipGO;
    ShipGameStats playerShipGameStats;
    NetworkObject myNetworkObject;
    bool readyToSpawn; //true когда данные о корабле игрока получены и можно его спавнить
    
    private void Start()
    {
        myNetworkObject = GetComponent<NetworkObject>();
        ownerClientID = myNetworkObject.OwnerClientId;
        if (NetworkManager.Singleton.IsServer)
        {
            WaitingToSpawnPlayerShip();
            teamID.Value = new NetworkString(Random.Range(1000000000, 2000000000) + "" + Random.Range(1000000000, 2000000000) + "" + Random.Range(1000000000, 2000000000));
        }
        if (IsOwner)
        {
            PlayerInterface.Instance.LocalPlayer = this;
            PlayerInterface.Instance.attackButtonStateChangedMessage += ReceiveAttackButtonStateChangedRpc;
            string playerShipName = DataOperator.instance.shipsPrefabs[playerShipNum].GetComponent<ItemData>().Name.EnglishText;
            ModuleOnShipData[] modulesOnPlayerShip = DataOperator.instance.LoadDataModulesOnShip("ModulesOnShipData(" + playerShipName + ")");
            Ship newPlayerShip = new Ship(playerShipNum, modulesOnPlayerShip);
            SendPlayerShipDataToServerRpc(newPlayerShip);
        }
    }

    public override void OnDestroy()
    {
        if (IsOwner)
        {
            PlayerInterface.Instance.attackButtonStateChangedMessage -= ReceiveAttackButtonStateChangedRpc;
        }   
    }

    void WaitingToSpawnPlayerShip()
    {
        if (readyToSpawn)
        {
            SpawnPlayerShip();
        }
        else
        {
            Invoke(nameof(WaitingToSpawnPlayerShip), 0.1f);
        }
    }

    void SpawnPlayerShip()
    {
        if (playerShipGO != null)
        {
            Debug.LogWarning("Попытка заспавнить корабль игроку, у которого сейчас и так есть заспавненный корабль; откат");
            return;
        }
        if (playerShipNum < DataOperator.instance.shipsPrefabs.Length)
        {
            GameObject playerShipPrefab = DataOperator.instance.shipsPrefabs[playerShip.shipPrefabNum];
            GameObject playerShipSpawned = Instantiate(playerShipPrefab, new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0), Quaternion.identity);
            playerShipGameStats = playerShipSpawned.GetComponent<ShipGameStats>();

            ModuleOnShipData[] modulesOnPlayerShipData = playerShip.modulesOnShipData;
            if (modulesOnPlayerShipData == null)
            {
                modulesOnPlayerShipData = new ModuleOnShipData[0];
            }
            ShipGameModulesCreator shipGameModulesCreator = playerShipSpawned.GetComponent<ShipGameModulesCreator>();
            shipGameModulesCreator.modulesOnShip = modulesOnPlayerShipData;
            shipGameModulesCreator.teamID = teamID.Value.GetString();
            shipGameModulesCreator.CreateShipModules();

            ShipStats playerShipStats = playerShipSpawned.GetComponent<ShipStats>();
            playerShipStats.Initialize();
            playerShipStats.modulesOnShip = modulesOnPlayerShipData;
            playerShipStats.CalculateShipStats();

            playerShipGO = playerShipSpawned;
            playerShipSpawned.GetComponent<NetworkObject>().SpawnWithOwnership(ownerClientID);

            ModulesCellsDurabilityShower modulesCellsDurabilityShower = playerShipSpawned.GetComponent<ModulesCellsDurabilityShower>();
            modulesCellsDurabilityShower.RenderHealthCells();

            ShipGameStats shipGameStats = playerShipSpawned.GetComponent<ShipGameStats>();
            shipGameStats.ServerInitialize();
            shipGameStats.TeamID.Value = teamID.Value;
            shipGameStats.MyPlayer = this;
        }
    }

    [Rpc(SendTo.Server)]
    void SendPlayerShipDataToServerRpc(Ship newPlayerShip)
    {
        playerShip = newPlayerShip;
        readyToSpawn = true;
    }

    [Rpc(SendTo.Server)]
    public void SendMovementJoystickInputsDataToServerRpc(bool pressed_, float direction_, float magnitude_)
    {
        movementJoystickPressed = pressed_;
        if (pressed_)
        {
            movementJoystickDirInDegrees = direction_;

            if (magnitude_ > 1)
            {
                magnitude_ = 1;
            }

            if (magnitude_ < 0)
            {
                magnitude_ = 0;
            }

            movementJoystickMagnitude = magnitude_;
        }
        
        if (playerShipGameStats != null)
        {
            playerShipGameStats.MovementJoystickPressed.Value = movementJoystickPressed;
            playerShipGameStats.MovementJoystickDirInDegrees.Value = movementJoystickDirInDegrees;
            playerShipGameStats.MovementJoystickMagnitude.Value = movementJoystickMagnitude;
        }
    }

    [Rpc(SendTo.Server)]
    void ReceiveAttackButtonStateChangedRpc(uint index, bool pressed)
    {
        playerShipGameStats.SendFireStateChange(index, pressed);
    }
}

[Serializable]
public struct Ship : INetworkSerializable
{
    public uint shipPrefabNum;
    public ModuleOnShipData[] modulesOnShipData;

    public Ship(uint newShipPrefabNum, ModuleOnShipData[] newModulesOnShipData)
    {
        shipPrefabNum = newShipPrefabNum;
        modulesOnShipData = newModulesOnShipData;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref shipPrefabNum);

        // Length
        int length = 0;
        if (!serializer.IsReader)
        {
            length = modulesOnShipData.Length;
        }

        serializer.SerializeValue(ref length);

        // Array
        if (serializer.IsReader)
        {
            modulesOnShipData = new ModuleOnShipData[length];
        }

        for (int n = 0; n < length; ++n)
        {
            serializer.SerializeValue(ref modulesOnShipData[n]);
        }
    }
}


[Serializable]
public struct NetworkString : INetworkSerializable
{
    char[] symbols;

    public NetworkString(string inputString)
    {
        if (inputString == null)
        {
            Debug.LogWarning("Не задана inputString для NetworkString");
            symbols = new char[0];
            return;
        }    
        symbols = new char[inputString.Length];
        for (int symbolNum = 0; symbolNum < inputString.Length; symbolNum++)
        {
            symbols[symbolNum] = inputString[symbolNum];
        }
    }

    public string GetString()
    {
        string outoutString = "";
        if (symbols == null)
        {
            Debug.LogWarning("Попытка получить не заданную string из NetworkString");
        }
        for (int symbolNum = 0; symbolNum < symbols.Length; symbolNum++)
        {
            outoutString += symbols[symbolNum];
        }
        return outoutString;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Length
        int length = 0;
        if (symbols != null && !serializer.IsReader)
        {
            length = symbols.Length;
        }

        serializer.SerializeValue(ref length);

        // Array
        if (serializer.IsReader)
        {
            symbols = new char[length];
        }

        if (symbols != null)
        {
            for (int n = 0; n < length; ++n)
            {
                serializer.SerializeValue(ref symbols[n]);
            }
        }
    }
}
