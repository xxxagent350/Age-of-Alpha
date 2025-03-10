using System;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    [Header("���������")]
    [SerializeField] private uint playerShipNum;

    [Header("�������")]
    [SerializeField] private Ship playerShip;
    [SerializeField] private ulong ownerClientID;
    [SerializeField] private NetworkVariable<NetworkString> teamID = new();

    [Header("������������ �� �������")]
    [SerializeField] private bool movementJoystickPressed;
    [SerializeField] private float movementJoystickDirInDegrees;
    [SerializeField] private float movementJoystickMagnitude;
    private GameObject playerShipGO;
    private ShipGameStats playerShipGameStats;
    private NetworkObject myNetworkObject;
    private bool readyToSpawn; //true ����� ������ � ������� ������ �������� � ����� ��� ��������

    private void Start()
    {
        playerShipNum = (uint)DataOperator.instance.LoadDataInt(ShipChanger.playerShipNumDataName);

        myNetworkObject = GetComponent<NetworkObject>();
        ownerClientID = myNetworkObject.OwnerClientId;
        if (NetworkManager.Singleton.IsServer)
        {
            WaitingToSpawnPlayerShip();
            teamID.Value = new NetworkString(Random.Range(1000000000, 2000000000) + "" + Random.Range(1000000000, 2000000000) + "" + Random.Range(1000000000, 2000000000));
        }
        if (IsOwner)
        {
            ShipInterfaceManager.Instance.LocalPlayer = this;
            ShipInterfaceManager.Instance.attackButtonStateChangedMessage += ReceiveAttackButtonStateChangedRpc;
            string playerShipName = DataOperator.instance.shipsPrefabs[playerShipNum].GetComponent<ItemData>().Name.EnglishText;
            ModuleOnShipData[] modulesOnPlayerShip = DataOperator.instance.LoadDataModulesOnShip("ModulesOnShipData(" + playerShipName + ")");
            Ship newPlayerShip = new(playerShipNum, modulesOnPlayerShip);
            SendPlayerShipDataToServerRpc(newPlayerShip);
        }
    }

    public override void OnDestroy()
    {
        if (IsOwner)
        {
            ShipInterfaceManager.Instance.attackButtonStateChangedMessage -= ReceiveAttackButtonStateChangedRpc;
        }
    }

    private void WaitingToSpawnPlayerShip()
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

    private void SpawnPlayerShip()
    {
        if (playerShipGO != null)
        {
            Debug.LogWarning("������� ���������� ������� ������, � �������� ������ � ��� ���� ������������ �������; �����");
            return;
        }
        if (playerShipNum < DataOperator.instance.shipsPrefabs.Length)
        {
            GameObject playerShipPrefab = DataOperator.instance.shipsPrefabs[playerShip.shipPrefabNum];
            GameObject playerShipSpawned = Instantiate(playerShipPrefab, new Vector3(Random.Range(-20, 20), Random.Range(-20, 20), 0), Quaternion.identity);
            playerShipGameStats = playerShipSpawned.GetComponent<ShipGameStats>();

            ModuleOnShipData[] modulesOnPlayerShipData = playerShip.modulesOnShipData;
            modulesOnPlayerShipData ??= new ModuleOnShipData[0];
            ShipGameModulesCreator shipGameModulesCreator = playerShipSpawned.GetComponent<ShipGameModulesCreator>();
            shipGameModulesCreator.modulesOnShip = modulesOnPlayerShipData;
            shipGameModulesCreator.teamID = teamID.Value.String;
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
    private void SendPlayerShipDataToServerRpc(Ship newPlayerShip)
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
    private void ReceiveAttackButtonStateChangedRpc(uint index, bool pressed)
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
    public char[] Symbols { get; private set; }

    public NetworkString(string inputString)
    {
        if (inputString == null)
        {
            Debug.LogWarning("�� ������ inputString ��� NetworkString");
            Symbols = new char[0];
            return;
        }
        Symbols = new char[inputString.Length];
        for (int symbolNum = 0; symbolNum < inputString.Length; symbolNum++)
        {
            Symbols[symbolNum] = inputString[symbolNum];
        }
    }

    public string String
    {
        get
        {
            string outputString = "";
            if (Symbols == null)
            {
                Debug.LogWarning("������� �������� �� �������� string �� NetworkString");
            }
            foreach (char symbol in Symbols)
            {
                outputString += symbol;
            }
            return outputString;
        }
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        // Length
        int length = 0;
        if (Symbols != null && !serializer.IsReader)
        {
            length = Symbols.Length;
        }

        serializer.SerializeValue(ref length);

        // Array
        if (serializer.IsReader)
        {
            Symbols = new char[length];
        }

        if (Symbols != null)
        {
            for (int n = 0; n < length; ++n)
            {
                serializer.SerializeValue(ref Symbols[n]);
            }
        }
    }
}
