using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class ModulesMenu : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] GameObject[] modulesPrefabs;
    [SerializeField] GameObject UImodulesMenuSlot;

    [Header("Отладка")]
    [SerializeField] bool give999Modules;

    private void Update()
    {
        if (give999Modules)
        {
            Give999Modules();
            give999Modules = false;
        }
    }

    void Give999Modules()
    {
        for (int module = 0; module < modulesPrefabs.Length; module++)
        {
            DataOperator.instance.SaveData("ModulesOnStorageData(" + modulesPrefabs[module].name + ")", new ModulesOnStorageData(new Module(module, new string[0], new float[0]), 999));
        }
        Debug.Log("Gived 999 modules!");
    }
}

[Serializable]
public class ModulesOnStorageData
{
    public Module module;
    public int amount;
    public ModulesOnStorageData(Module module_, int amount_)
    {
        module = module_;
        amount = amount_;
    }
}

[Serializable]
public class Module
{
    public int moduleNum;
    public string[] upgrades;
    public float[] upgradesMod;

    public Module(int moduleNum_, string[] upgrades_, float[] upgradesMod_)
    {
        moduleNum = moduleNum_;
        upgrades = upgrades_;
        upgradesMod = upgradesMod_;
    }
}