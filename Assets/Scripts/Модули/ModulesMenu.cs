using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

public class ModulesMenu : MonoBehaviour
{
    [Header("Настройка")]
    public GameObject[] modulesPrefabs;
    [SerializeField] GameObject moduleSlotPrefab;
    [SerializeField] RectTransform scrollingContent;

    //слоты категорий модулей
    [SerializeField] GameObject weaponSlot;
    [SerializeField] GameObject defenseSlot;
    [SerializeField] GameObject energySlot;
    [SerializeField] GameObject enginesSlot;
    [SerializeField] GameObject dronesSlot;
    [SerializeField] GameObject specialSlot;

    [Header("Отладка")]
    [SerializeField] bool give999Modules;

    ModuleData[] modulesComponents;
    ModuleData.categories categoryFilter = ModuleData.categories.None;
    //ModuleData.types typeFilter = ModuleData.types.None;

    //какие типы модулей имеются на складе
    bool weaponCategoryExists;
    bool defenseCategoryExists;
    bool energyCategoryExists;
    bool enginesCategoryExists;
    bool dronesCategoryExists;
    bool specialCategoryExists;

    [SerializeField] GameObject[] menuSlots;

    private void Start()
    {
        menuSlots = new GameObject[0];
        modulesComponents = new ModuleData[modulesPrefabs.Length];
        for (int i = 0; i < modulesPrefabs.Length; i++)
        {
            modulesComponents[i] = modulesPrefabs[i].GetComponent<ModuleData>();
        }
        RenderMenuSlosts();
    }

    void CalculateModulesCategoriesAndTypes() //хочешь развить свой мозг? попробуй в этом разобраться
    {
        foreach (Data data in DataOperator.instance.gameData)
        {
            if (data.dataModulesOnStorage != null)
            {
                ModuleData.categories category = modulesComponents[data.dataModulesOnStorage.module.moduleNum].category;
                if (category == ModuleData.categories.Weapons)
                    weaponCategoryExists = true;
                if (category == ModuleData.categories.DefenceModules)
                    defenseCategoryExists = true;
                if (category == ModuleData.categories.EnergyBlocks)
                    energyCategoryExists = true;
                if (category == ModuleData.categories.Engines)
                    enginesCategoryExists = true;
                if (category == ModuleData.categories.Drones)
                    dronesCategoryExists = true;
                if (category == ModuleData.categories.SpecialModules)
                    specialCategoryExists = true;
            }
        }
    }

    void RenderMenuSlosts()
    {
        foreach (GameObject slot in menuSlots)
        {
            Destroy(slot);
        }
        menuSlots = new GameObject[0];

        if (categoryFilter == ModuleData.categories.None) //сортировка по категориям
        {
            CalculateModulesCategoriesAndTypes();
            if (weaponCategoryExists)
                AddSlot(weaponSlot);
            if (defenseCategoryExists)
                AddSlot(defenseSlot);
            if (energyCategoryExists)
                AddSlot(energySlot);
            if (enginesCategoryExists)
                AddSlot(enginesSlot);
            if (dronesCategoryExists)
                AddSlot(dronesSlot);
            if (specialCategoryExists)
                AddSlot(specialSlot);
        }
        if (categoryFilter == ModuleData.categories.Weapons)
        {
            GameObject slot_ = AddSlot(weaponSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromWeaponModules";
            AddSlotsOfCategory(ModuleData.categories.Weapons);
        }
        if (categoryFilter == ModuleData.categories.DefenceModules)
        {
            GameObject slot_ = AddSlot(defenseSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromDefenseModules";
            AddSlotsOfCategory(ModuleData.categories.DefenceModules);
        }
        if (categoryFilter == ModuleData.categories.EnergyBlocks)
        {
            GameObject slot_ = AddSlot(energySlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromEnergyModules";
            AddSlotsOfCategory(ModuleData.categories.EnergyBlocks);
        }
        if (categoryFilter == ModuleData.categories.Engines)
        {
            GameObject slot_ = AddSlot(enginesSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromEngineModules";
            AddSlotsOfCategory(ModuleData.categories.Engines);
        }
        if (categoryFilter == ModuleData.categories.Drones)
        {
            GameObject slot_ = AddSlot(dronesSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromDroneModules";
            AddSlotsOfCategory(ModuleData.categories.Drones);
        }
        if (categoryFilter == ModuleData.categories.SpecialModules)
        {
            GameObject slot_ = AddSlot(specialSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromSpecialModules";
            AddSlotsOfCategory(ModuleData.categories.SpecialModules);
        }

        scrollingContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (menuSlots.Length * 80) + ((menuSlots.Length + 1) * 5));
        scrollingContent.anchoredPosition = new Vector3();
    }

    void AddSlotsOfCategory(ModuleData.categories category)
    {
        foreach (Data data in DataOperator.instance.gameData)
        {
            GameObject modulePrefab = modulesPrefabs[data.dataModulesOnStorage.module.moduleNum];
            if (modulePrefab.GetComponent<ModuleData>().category == category)
            {
                GameObject slot = AddSlot(moduleSlotPrefab);
                slot.GetComponent<ModulesMenuSlot>().SetModuleData(data.dataName);
            }
        }
    }

    GameObject AddSlot(GameObject slot)
    {
        Array.Resize(ref menuSlots, menuSlots.Length + 1);
        GameObject slot_ = Instantiate(slot, scrollingContent);
        slot_.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5 - ((menuSlots.Length - 1) * 85), 0);
        menuSlots[menuSlots.Length - 1] = slot_;
        return slot_;
    }

    private void LateUpdate()
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
        CalculateModulesCategoriesAndTypes();
        RenderMenuSlosts();
        Debug.Log("Gived 999 modules!");
    }


    public void ShowAllSlots()
    {
        categoryFilter = ModuleData.categories.None;
        RenderMenuSlosts();
    }

    public void ShowWeaponModules()
    {
        categoryFilter = ModuleData.categories.Weapons;
        RenderMenuSlosts();
    }
    public void ShowDefenseModules()
    {
        categoryFilter = ModuleData.categories.DefenceModules;
        RenderMenuSlosts();
    }
    public void ShowEnergyModules()
    {
        categoryFilter = ModuleData.categories.EnergyBlocks;
        RenderMenuSlosts();
    }
    public void ShowEngineModules()
    {
        categoryFilter = ModuleData.categories.Engines;
        RenderMenuSlosts();
    }
    public void ShowDroneModules()
    {
        categoryFilter = ModuleData.categories.Drones;
        RenderMenuSlosts();
    }
    public void ShowSpecialModules()
    {
        categoryFilter = ModuleData.categories.SpecialModules;
        RenderMenuSlosts();
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