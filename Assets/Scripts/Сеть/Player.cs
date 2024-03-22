using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    [Header("Ќастройка")]
    [SerializeField] uint playerShipNum;

    [Header("ќтладка")]
    [SerializeField] Ship playerShip;
    [SerializeField] ulong ownerClientID;
    [SerializeField] NetworkVariable<NetworkString> teamID = new NetworkVariable<NetworkString>();

    GameObject playerShipGO;
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
            string playerShipName = DataOperator.instance.shipsPrefabs[playerShipNum].GetComponent<ItemData>().Name.EnglishText;
            ModuleOnShipData[] modulesOnPlayerShip = DataOperator.instance.LoadDataModulesOnShip("ModulesOnShipData(" + playerShipName + ")");
            Ship newPlayerShip = new Ship(playerShipNum, modulesOnPlayerShip);
            SendPlayerShipDataToServerRpc(newPlayerShip);
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
            Debug.LogWarning("ѕопытка заспавнить корабль игроку, у которого сейчас и так есть заспавненный корабль; откат");
            return;
        }
        if (playerShipNum < DataOperator.instance.shipsPrefabs.Length)
        {
            GameObject playerShipPrefab = DataOperator.instance.shipsPrefabs[playerShip.shipPrefabNum];
            GameObject playerShipSpawned = Instantiate(playerShipPrefab, new Vector3(1, 1, 0), Quaternion.identity);

            ModuleOnShipData[] modulesOnPlayerShipData = playerShip.modulesOnShipData;
            if (modulesOnPlayerShipData == null)
            {
                modulesOnPlayerShipData = new ModuleOnShipData[0];
            }
            ShipGameModulesCreator shipGameModulesCreator = playerShipSpawned.GetComponent<ShipGameModulesCreator>();
            shipGameModulesCreator.modulesOnShip = modulesOnPlayerShipData;
            shipGameModulesCreator.CreateShipModules();

            ShipStats playerShipStats = playerShipSpawned.GetComponent<ShipStats>();
            playerShipStats.Initialize();
            playerShipStats.modulesOnShip = modulesOnPlayerShipData;
            playerShipStats.CalculateShipStats();

            playerShipGO = playerShipSpawned;
            playerShipSpawned.GetComponent<NetworkObject>().SpawnWithOwnership(ownerClientID);
            playerShipSpawned.GetComponent<ShipGameStats>().InitializeRpc();
        }
    }

    [Rpc(SendTo.Server)]
    void SendPlayerShipDataToServerRpc(Ship newPlayerShip)
    {
        playerShip = newPlayerShip;
        readyToSpawn = true;
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

/*
[Serializable]
public struct NetworkString : INetworkSerializeByMemcpy
{
    public string String;

    public NetworkString(string inputString)
    {
        String = inputString;
    }

    public string GetString()
    {
        return String;
    }
}
*/


[Serializable]
public struct NetworkString : INetworkSerializeByMemcpy
{
    char[] symbols;

    public NetworkString(string inputString)
    {
        symbols = new char[inputString.Length];
        for (int symbolNum = 0; symbolNum < inputString.Length; symbolNum++)
        {
            symbols[symbolNum] = inputString[symbolNum];
        }
    }

    public string GetString()
    {
        string outoutString = "";
        for (int symbolNum = 0; symbolNum < symbols.Length; symbolNum++)
        {
            outoutString += symbols[symbolNum];
        }
        return outoutString;
    }
}
