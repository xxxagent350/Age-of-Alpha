using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class AISpawner : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private string _teamIDString = "enemy";

    [Header("Отладка")]
    [SerializeField] private Ship _shipToSpawn;
    private List<ShipGameStats> _spawnedShips;

    private void Start()
    {
        StartCoroutine(WaitingToSpawnAIShip());
    }

    private IEnumerator WaitingToSpawnAIShip()
    {
        while (NetworkManager.Singleton.IsServer == false)
        {
            yield return new WaitForSecondsRealtime(3);
        }
        SpawnAIShip(_shipToSpawn);
    }

    public void SpawnAIShip(Ship shipToSpawn)
    {
        if (shipToSpawn.shipPrefabNum < DataOperator.instance.shipsPrefabs.Length)
        {
            GameObject spawningShipPrefab = DataOperator.instance.shipsPrefabs[shipToSpawn.shipPrefabNum];
            GameObject spawnedShip = Instantiate(spawningShipPrefab, new Vector3(Random.Range(-100, 100), Random.Range(-100, 100), 0), Quaternion.identity);
            ShipGameStats spawningShipGameStats = spawnedShip.GetComponent<ShipGameStats>();

            ModuleOnShipData[] modulesOnSpawningShipData = shipToSpawn.modulesOnShipData;
            modulesOnSpawningShipData ??= new ModuleOnShipData[0];
            ShipGameModulesCreator shipGameModulesCreator = spawnedShip.GetComponent<ShipGameModulesCreator>();
            shipGameModulesCreator.modulesOnShip = modulesOnSpawningShipData;
            shipGameModulesCreator.teamID = _teamIDString;
            shipGameModulesCreator.CreateShipModules();

            ShipStats spawningShipStats = spawnedShip.GetComponent<ShipStats>();
            spawningShipStats.Initialize();
            spawningShipStats.modulesOnShip = modulesOnSpawningShipData;
            spawningShipStats.CalculateShipStats();

            spawnedShip.GetComponent<NetworkObject>().Spawn();

            ModulesCellsDurabilityShower modulesCellsDurabilityShower = spawnedShip.GetComponent<ModulesCellsDurabilityShower>();
            modulesCellsDurabilityShower.ActivateNetworkAuthorityChecker(null);
            modulesCellsDurabilityShower.RenderHealthCells();

            ShipGameStats shipGameStats = spawnedShip.GetComponent<ShipGameStats>();
            shipGameStats.isControlledByAI = true;
            shipGameStats.ServerInitialize();
            shipGameStats.TeamID.Value = new NetworkString(_teamIDString);
        }
        else
        {
            Debug.LogError($"Невозможно заспавнить корабль с индексом {shipToSpawn.shipPrefabNum}, так как он выходит за рамки массива кораблей в DataOperator");
        }
    }
}
