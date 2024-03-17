using UnityEngine;

public class ShipGameModulesCreator : MonoBehaviour
{
    [HideInInspector] public ModuleOnShipData[] modulesOnShip;
    Transform modulesParent;

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
