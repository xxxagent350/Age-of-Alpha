using UnityEngine;
using Unity.Netcode;

public class ShipGameModulesCreator : NetworkBehaviour
{
    [HideInInspector] public ModuleOnShipData[] modulesOnShip;
    Transform modulesParent;
    [HideInInspector] public string teamID;
    private ShipGameStats _myShipGameStats;

    public void CreateShipModules()
    {
        if (_myShipGameStats == null)
        {
            _myShipGameStats = GetComponent<ShipGameStats>();
        }
        TryFoundModulesParent();
        for (int moduleNum = 0; moduleNum < modulesOnShip.Length; moduleNum++)
        {
            GameObject modulePrefab = DataOperator.instance.modulesPrefabs[modulesOnShip[moduleNum].module.moduleNum];
            GameObject moduleSpawned = Instantiate(modulePrefab);
            moduleSpawned.transform.parent = modulesParent;
            moduleSpawned.transform.localPosition = modulesOnShip[moduleNum].position.GetVector2();
            moduleSpawned.transform.Find("Image").GetComponent<SpriteRenderer>().enabled = false;

            ItemData itemData = moduleSpawned.GetComponent<ItemData>();
            if (itemData.Type == modulesTypes.ControlModules)
            {
                _myShipGameStats.ControlBlock = moduleSpawned.GetComponent<Durability>();
            }
            if (itemData.Category == modulesCategories.Engines)
            {
                _myShipGameStats.numOfEngines++;
            }
            if (itemData.Category == modulesCategories.Weapon)
            {
                _myShipGameStats.numOfWeapons++;
            }
            Durability modulesDurability = moduleSpawned.GetComponent<Durability>();
            if (modulesDurability != null)
            {
                modulesDurability.TeamID = teamID;
            }
        }
    }

    void TryFoundModulesParent()
    {
        if (modulesParent == null)
        {
            modulesParent = transform.Find("Modules");
        }
    }
}
