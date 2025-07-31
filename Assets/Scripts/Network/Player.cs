using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class Player : NetworkBehaviour
{
    [Header("Ќастройка")]
    public PlayerShipRespawner PlayerShipRespawner;

    [Header("ќтладка")]
    [SerializeField] private Ship playerShip;
    [SerializeField] private uint playerShipNum;
    [SerializeField] private ulong ownerClientID;
    [SerializeField] private NetworkVariable<NetworkString> teamID = new();

    [Header("ќтображаетс€ на сервере")]
    [SerializeField] private bool movementJoystickPressed;
    [SerializeField] private float movementJoystickDirInDegrees;
    [SerializeField] private float movementJoystickMagnitude;
    private GameObject playerShipGO;
    private ShipGameStats playerShipGameStats;
    private NetworkObject myNetworkObject;
    private bool readyToSpawn; //true когда данные о корабле игрока получены и можно его спавнить

    private void Start()
    {
        playerShipNum = (uint)DataOperator.instance.LoadDataInt(ShipChanger.playerShipNumDataName);

        myNetworkObject = GetComponent<NetworkObject>();
        ownerClientID = myNetworkObject.OwnerClientId;
        if (NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(WaitingToSpawnPlayerShip());
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

    private IEnumerator WaitingToSpawnPlayerShip()
    {
        while (!readyToSpawn)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        SpawnPlayerShip();
    }

    public void SpawnPlayerShip()
    {
        if (playerShipGameStats != null && playerShipGameStats.Destroyed.Value == false)
        {
            Debug.LogWarning("ѕопытка заспавнить корабль игроку, у которого сейчас и так есть заспавненный корабль; откат");
            return;
        }
        if (playerShipNum < DataOperator.instance.shipsPrefabs.Length)
        {
            if (playerShipGameStats != null) //у игрока есть корабль, но он уничтожен
            {
                playerShipGameStats.MyPlayer = null;
                playerShipGameStats.DeactivateNetworkAuthorityChecker();
                playerShipGameStats.GetComponent<ModulesCellsDurabilityShower>().DisableHealthCellsRpc();
            }

            GameObject playerShipPrefab = DataOperator.instance.shipsPrefabs[playerShip.shipPrefabNum];
            GameObject playerShipSpawned = Instantiate(playerShipPrefab, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), Quaternion.identity);
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
            modulesCellsDurabilityShower.ActivateNetworkAuthorityChecker(this);
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