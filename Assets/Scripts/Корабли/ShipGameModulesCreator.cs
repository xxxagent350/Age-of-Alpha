using UnityEngine;
using Unity.Netcode;

public class ShipGameModulesCreator : NetworkBehaviour
{
    [HideInInspector] public ModuleOnShipData[] modulesOnShip;
    Transform modulesParent;
    [HideInInspector] public string teamID;

    public void CreateShipModules()
    {
        TryFoundModulesParent();
        for (int moduleNum = 0; moduleNum < modulesOnShip.Length; moduleNum++)
        {
            GameObject modulePrefab = DataOperator.instance.modulesPrefabs[modulesOnShip[moduleNum].module.moduleNum];
            GameObject moduleSpawned = Instantiate(modulePrefab);
            moduleSpawned.transform.parent = modulesParent;
            moduleSpawned.transform.localPosition = modulesOnShip[moduleNum].position.GetVector2();
            moduleSpawned.transform.Find("Image").GetComponent<SpriteRenderer>().enabled = false;

            Durability modulesDurability = moduleSpawned.GetComponent<Durability>();
            if (modulesDurability != null)
            {
                modulesDurability.teamID = teamID;
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
