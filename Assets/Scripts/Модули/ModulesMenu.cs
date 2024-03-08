using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ModulesMenu : MonoBehaviour
{
    [Header("Настройка")]
    public GameObject[] modulesPrefabs;
    [SerializeField] GameObject moduleSlotPrefab;
    [SerializeField] RectTransform scrollingContent;

    [SerializeField] GameObject modulesList;
    [SerializeField] GameObject moduleParametres;
    [SerializeField] RectTransform moduleInfoContent;
    [SerializeField] Image moduleParametresImage;
    [SerializeField] Text moduleParametresName;
    [SerializeField] TextMeshProUGUI moduleParametresInfo;
    public ModuleInstallationErrorMessage moduleInstallationErrorMessageComponent;

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
    float moduleInfoContentStartYPos;
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
        moduleInfoContentStartYPos = moduleInfoContent.position.y;
        menuSlots = new GameObject[0];
        modulesComponents = new ModuleData[modulesPrefabs.Length];
        for (int i = 0; i < modulesPrefabs.Length; i++)
        {
            modulesComponents[i] = modulesPrefabs[i].GetComponent<ModuleData>();
        }
        RenderMenuSlosts();
        BackFromModuleParametres();
        //string text = "Прочность: 15\nМасса: 600 (550 + <color=red>50</color>)\nМощность: 6 (5 + <color=green>1</color>)";
        //moduleDescription.SetText(text);
    }

    void CalculateModulesCategoriesAndTypes() //хочешь развить свой мозг? попробуй в этом разобраться
    {
        foreach (Data data in DataOperator.instance.gameData)
        {
            if (data.dataModulesOnStorage != null && data.dataModulesOnStorage.amount > 0)
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

    public void RenderMenuSlosts()
    {
        RemoveAllMenuSlots();
        BackFromModuleParametres();

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

    public void RemoveAllMenuSlots()
    {
        foreach (GameObject slot in menuSlots)
        {
            Destroy(slot);
        }
        menuSlots = new GameObject[0];
    }

    void AddSlotsOfCategory(ModuleData.categories category)
    {
        foreach (Data data in DataOperator.instance.gameData)
        {
            if (data.dataModulesOnStorage != null && data.dataModulesOnStorage.amount > 0)
            {
                GameObject modulePrefab = modulesPrefabs[data.dataModulesOnStorage.module.moduleNum];
                if (modulePrefab.GetComponent<ModuleData>().category == category)
                {
                    GameObject slot = AddSlot(moduleSlotPrefab);
                    slot.GetComponent<ModulesMenuSlot>().SetModuleData(data.dataName);
                }
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

    public void ShowModuleParametres(Module module)
    {
        GameObject modulePrefab = modulesPrefabs[module.moduleNum];

        moduleParametresImage.sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteRenderer>().sprite;
        moduleParametresName.text = modulePrefab.GetComponent<ItemData>().Name.GetTranslatedText();
        moduleParametresInfo.text = GetModuleParametresInfo(module).GetTranslatedText();

        moduleInfoContent.position = new Vector2(moduleInfoContent.position.x, moduleInfoContentStartYPos);
        moduleParametres.SetActive(true);
        modulesList.SetActive(false);
    }

    public void BackFromModuleParametres()
    {
        moduleParametres.SetActive(false);
        modulesList.SetActive(true);
    }

    TranslatedText GetModuleParametresInfo(Module module)
    {
        TranslatedText text = new TranslatedText();

        GameObject modulePrefab = modulesPrefabs[module.moduleNum];
        ItemData itemData = modulePrefab.GetComponent<ItemData>();
        Armour armourComponent = modulePrefab.GetComponent<Armour>();
        Engine engineComponent = modulePrefab.GetComponent<Engine>();
        Weapon weaponComponent = modulePrefab.GetComponent<Weapon>();
        EnergyGenerator generatorComponent = modulePrefab.GetComponent<EnergyGenerator>();
        Battery batteryComponent = modulePrefab.GetComponent<Battery>();

        text.RussianText += "Масса: " + itemData.Mass;
        text.EnglishText += "Mass: " + itemData.Mass;
        if (armourComponent != null)
        {
            text.RussianText += "\nПрочность: " + armourComponent.maxHP;
            text.EnglishText += "\nDurability: " + armourComponent.maxHP;
        }
        else
        {
            TranslatedText debugText = new TranslatedText();
            debugText.RussianText = "На префабе модуля " + modulePrefab.name + " отсутствует компонент Armour, который должен быть на всех модулях (он отвечает за прочность модуля)";
            debugText.EnglishText = "On the module's prefab " + modulePrefab.name + " is missing armour component, which should be on all modules (it is responsible for the maxHP of the module)";
            Debug.LogError(debugText.GetTranslatedText());
            return debugText;
        }
        if (generatorComponent != null)
        {
            text.RussianText += "\nГенерация энергии в секунду: " + generatorComponent.power;
            text.EnglishText += "\nEnergy generation per second: " + generatorComponent.power;
        }
        if (batteryComponent != null)
        {
            text.RussianText += "\nЗапас энергии: " + batteryComponent.maxCapacity;
            text.EnglishText += "\nEnergy reserve: " + batteryComponent.maxCapacity;
        }
        if (engineComponent != null)
        {
            text.RussianText += "\nТяга: " + engineComponent.accelerationPower;
            text.EnglishText += "\nThrust: " + engineComponent.accelerationPower;

            text.RussianText += "\nКрутящий момент: " + engineComponent.angularPower;
            text.EnglishText += "\nTorque: " + engineComponent.angularPower;

            text.RussianText += "\nПотребление энергии в секунду: " + engineComponent.powerConsumption;
            text.EnglishText += "\nPower consumption per second: " + engineComponent.powerConsumption;
        }

        text.RussianText += "\n\n" + itemData.description.RussianText;
        text.EnglishText += "\n\n" + itemData.description.EnglishText;
        return text;
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