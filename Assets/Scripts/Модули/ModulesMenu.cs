using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TMPro;

public class ModulesMenu : MonoBehaviour
{
    [Header("Настройка")]
    //public GameObject[] DataOperator.instance.modulesPrefabs;
    [SerializeField] GameObject moduleSlotPrefab;
    [SerializeField] RectTransform scrollingContent;

    [SerializeField] GameObject modulesList;
    [SerializeField] GameObject moduleParametres;
    [SerializeField] RectTransform moduleInfoContent;
    [SerializeField] Image moduleParametresImage;
    [SerializeField] Text moduleParametresName;
    [SerializeField] TextMeshProUGUI moduleParametresInfo;
    public ModuleInstallationErrorMessage moduleInstallationErrorMessageComponent;
    [SerializeField] GameObject noModulesOnStorageText;
    [SerializeField] TextMeshProUGUI shipStatsText;

    [SerializeField] Image revertButton;
    [SerializeField] Image repeatButton;

    [SerializeField] GameObject infoButton;
    [SerializeField] Sprite infoButtonEnabledSprite;
    [SerializeField] Sprite infoButtonDisabledSprite;

    [SerializeField] GameObject applyingWarningPanel;
    [SerializeField] UnityEngine.Object OKScene;

    [SerializeField] AudioClip clickSound;
    [SerializeField] float clickSoundVolume = 1;

    //слоты категорий модулей
    [SerializeField] GameObject weaponSlot;
    [SerializeField] GameObject defenseSlot;
    [SerializeField] GameObject energySlot;
    [SerializeField] GameObject enginesSlot;
    [SerializeField] GameObject dronesSlot;
    [SerializeField] GameObject specialSlot;

    [Header("Отладка")]
    [SerializeField] bool give999Modules;
    [SerializeField] GameObject[] menuSlots;

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

    ShipStats shipStats;
    bool shipStatsButtonEnabled;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("ShowShipStatsLastButtonState"))
        {
            PlayerPrefs.SetInt("ShowShipStatsLastButtonState", 1);
        }
        if (PlayerPrefs.GetInt("ShowShipStatsLastButtonState") == 0)
        {
            DisableShipStats();
        }
        else
        {
            EnableShipStats();
        }
        TryFoundShipStats();
        moduleInfoContentStartYPos = moduleInfoContent.position.y;
        menuSlots = new GameObject[0];
        modulesComponents = new ModuleData[DataOperator.instance.modulesPrefabs.Length];
        for (int i = 0; i < DataOperator.instance.modulesPrefabs.Length; i++)
        {
            modulesComponents[i] = DataOperator.instance.modulesPrefabs[i].GetComponent<ModuleData>();
        }
        RenderMenuSlosts();
        BackFromModuleParametres();
    }

    void TryFoundShipStats()
    {
        if (shipStats == null)
        {
            shipStats = (ShipStats)FindFirstObjectByType(typeof(ShipStats));
        }
    }

    void CalculateModulesCategoriesAndTypes() //хочешь развить свой мозг? попробуй в этом разобраться
    {
        weaponCategoryExists = false;
        defenseCategoryExists = false;
        energyCategoryExists = false;
        enginesCategoryExists = false;
        dronesCategoryExists = false;
        specialCategoryExists = false;

        ModulesOnStorageData[] modulesOnStorageData = DataOperator.instance.GetModulesOnStorageDataClonedArray();
        foreach (ModulesOnStorageData moduleOnStorageData in modulesOnStorageData)
        {
            ModuleData.categories category = modulesComponents[moduleOnStorageData.module.moduleNum].category;
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

    public void RenderMenuSlosts()
    {
        SetShipStats();
        CalculateModulesCategoriesAndTypes();
        RemoveAllMenuSlots();
        BackFromModuleParametres();
        ChangeTimeButtonsVisualState();

        if (categoryFilter == ModuleData.categories.None) //сортировка по категориям
        {
            if (!weaponCategoryExists &&
                !defenseCategoryExists &&
                !energyCategoryExists &&
                !enginesCategoryExists &&
                !dronesCategoryExists &&
                !specialCategoryExists)
            {
                noModulesOnStorageText.SetActive(true);
            }
            else
            {
                noModulesOnStorageText.SetActive(false);
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
        ModulesOnStorageData[] modulesOnStorageData = DataOperator.instance.GetModulesOnStorageDataClonedArray();
        foreach (ModulesOnStorageData moduleOnStorageData in modulesOnStorageData)
        {
            GameObject modulePrefab = DataOperator.instance.modulesPrefabs[moduleOnStorageData.module.moduleNum];
            if (modulePrefab.GetComponent<ModuleData>().category == category)
            {
                GameObject slot = AddSlot(moduleSlotPrefab);
                slot.GetComponent<ModulesMenuSlot>().SetModuleData(moduleOnStorageData.module);
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
        for (int module = 0; module < DataOperator.instance.modulesPrefabs.Length; module++)
        {
            DataOperator.instance.SaveData(new ModulesOnStorageData(new Module(module, new ModuleUpgrade[0]), 999));
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
        GameObject modulePrefab = DataOperator.instance.modulesPrefabs[module.moduleNum];

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

        GameObject modulePrefab = DataOperator.instance.modulesPrefabs[module.moduleNum];
        ItemData itemData = modulePrefab.GetComponent<ItemData>();
        Armour armourComponent = modulePrefab.GetComponent<Armour>();
        Engine engineComponent = modulePrefab.GetComponent<Engine>();
        Weapon weaponComponent = modulePrefab.GetComponent<Weapon>();
        EnergyGenerator generatorComponent = modulePrefab.GetComponent<EnergyGenerator>();
        Battery batteryComponent = modulePrefab.GetComponent<Battery>();

        text.RussianText += "Масса: " + DataOperator.instance.RoundFloat(itemData.Mass);
        text.EnglishText += "Mass: " + DataOperator.instance.RoundFloat(itemData.Mass);
        if (armourComponent != null)
        {
            text.RussianText += "\nПрочность: " + DataOperator.instance.RoundFloat(armourComponent.maxHP);
            text.EnglishText += "\nDurability: " + DataOperator.instance.RoundFloat(armourComponent.maxHP);

            if (armourComponent.resistanceToPhysicalDamage > 0)
            {
                text.RussianText += "\nСопротивление к физическому урону: " + DataOperator.instance.RoundFloat(armourComponent.resistanceToPhysicalDamage * 100) + "%";
                text.EnglishText += "\nResistance to physical damage: " + DataOperator.instance.RoundFloat(armourComponent.resistanceToPhysicalDamage * 100) + "%";
            }
            if (armourComponent.resistanceToFireDamage > 0)
            {
                text.RussianText += "\nСопротивление к тепловому урону: " + DataOperator.instance.RoundFloat(armourComponent.resistanceToFireDamage * 100) + "%";
                text.EnglishText += "\nResistance to heat damage: " + DataOperator.instance.RoundFloat(armourComponent.resistanceToFireDamage * 100) + "%";
            }
            if (armourComponent.resistanceToEnergyDamage > 0)
            {
                text.RussianText += "\nСопротивление к энерго урону: " + DataOperator.instance.RoundFloat(armourComponent.resistanceToEnergyDamage * 100) + "%";
                text.EnglishText += "\nResistance to energy damage: " + DataOperator.instance.RoundFloat(armourComponent.resistanceToEnergyDamage * 100) + "%";
            } 
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
            text.RussianText += "\nГенерация энергии в секунду: " + DataOperator.instance.RoundFloat(generatorComponent.power);
            text.EnglishText += "\nEnergy generation per second: " + DataOperator.instance.RoundFloat(generatorComponent.power);
        }
        if (batteryComponent != null)
        {
            text.RussianText += "\nЗапас энергии: " + DataOperator.instance.RoundFloat(batteryComponent.maxCapacity);
            text.EnglishText += "\nEnergy reserve: " + DataOperator.instance.RoundFloat(batteryComponent.maxCapacity);
        }
        if (engineComponent != null)
        {
            text.RussianText += "\nТяга: " + DataOperator.instance.RoundFloat(engineComponent.accelerationPower);
            text.EnglishText += "\nThrust: " + DataOperator.instance.RoundFloat(engineComponent.accelerationPower);

            text.RussianText += "\nКрутящий момент: " + DataOperator.instance.RoundFloat(engineComponent.angularPower);
            text.EnglishText += "\nTorque: " + DataOperator.instance.RoundFloat(engineComponent.angularPower);

            text.RussianText += "\nПотребление энергии в секунду: " + DataOperator.instance.RoundFloat(engineComponent.powerConsumption);
            text.EnglishText += "\nPower consumption per second: " + DataOperator.instance.RoundFloat(engineComponent.powerConsumption);
        }

        text.RussianText += "\n\n" + itemData.description.RussianText;
        text.EnglishText += "\n\n" + itemData.description.EnglishText;
        return text;
    }

    public void RemoveAllModulesFromShip()
    {
        SlotsPutter slotsPutter = (SlotsPutter)FindFirstObjectByType(typeof(SlotsPutter));
        ShipStats shipInstalledModulesData;
        if (slotsPutter != null)
            shipInstalledModulesData = slotsPutter.itemData.GetComponent<ShipStats>();
        else
            return;
        if (shipInstalledModulesData == null)
            return;

        shipInstalledModulesData.RemoveAllModules();
    }

    public void SetShipStats()
    {
        TryFoundShipStats();

        if (shipStats != null)
        {
            shipStats.CalculateShipStats();

            TranslatedText shipStatsTranslatedText = new TranslatedText();
            shipStatsTranslatedText.RussianText += "Масса: " + DataOperator.instance.RoundFloat(shipStats.totalMass);
            shipStatsTranslatedText.EnglishText += "Mass: " + DataOperator.instance.RoundFloat(shipStats.totalMass);

            shipStatsTranslatedText.RussianText += "\nЗапас энергии: " + DataOperator.instance.RoundFloat(shipStats.totalEnergyCapacity);
            shipStatsTranslatedText.EnglishText += "\nEnergy capacity: " + DataOperator.instance.RoundFloat(shipStats.totalEnergyCapacity);

            shipStatsTranslatedText.RussianText += "\nГенерация энергии: " + DataOperator.instance.RoundFloat(shipStats.totalEnergyGeneration);
            shipStatsTranslatedText.EnglishText += "\nEnergy generation: " + DataOperator.instance.RoundFloat(shipStats.totalEnergyGeneration);

            if (shipStats.totalEnginesConsumption > 0)
            {
                if (shipStats.totalEnginesConsumption < shipStats.totalEnergyGeneration / 2)
                {
                    shipStatsTranslatedText.RussianText += "\nПотребление двигателями: <color=green>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines сonsumption: <color=green>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
                if (shipStats.totalEnginesConsumption >= shipStats.totalEnergyGeneration / 2 && shipStats.totalEnginesConsumption <= shipStats.totalEnergyGeneration)
                {
                    shipStatsTranslatedText.RussianText += "\nПотребление двигателями: <color=yellow>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines сonsumption: <color=yellow>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
                if (shipStats.totalEnginesConsumption > shipStats.totalEnergyGeneration)
                {
                    shipStatsTranslatedText.RussianText += "\nПотребление двигателями: <color=red>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines сonsumption: <color=red>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
            }

            if (shipStats.totalWeaponConsumption > 0)
            {
                shipStatsTranslatedText.RussianText += "\nПотребление вооружением: " + DataOperator.instance.RoundFloat(shipStats.totalWeaponConsumption);
                shipStatsTranslatedText.EnglishText += "\nWeapons consumption: " + DataOperator.instance.RoundFloat(shipStats.totalWeaponConsumption);
            }

            if (shipStats.totalSystemsConsumption > 0)
            {
                shipStatsTranslatedText.RussianText += "\nПотребление системами: " + DataOperator.instance.RoundFloat(shipStats.totalSystemsConsumption);
                shipStatsTranslatedText.EnglishText += "\nSystems consumption: " + DataOperator.instance.RoundFloat(shipStats.totalSystemsConsumption);
            }

            shipStatsTranslatedText.RussianText += "\nСкорость: " + DataOperator.instance.RoundFloat(shipStats.totalSpeed);
            shipStatsTranslatedText.EnglishText += "\nSpeed: " + DataOperator.instance.RoundFloat(shipStats.totalSpeed);

            shipStatsTranslatedText.RussianText += "\nСкорость поворота: " + DataOperator.instance.RoundFloat(shipStats.totalAngularSpeed);
            shipStatsTranslatedText.EnglishText += "\nRotation speed: " + DataOperator.instance.RoundFloat(shipStats.totalAngularSpeed);


            shipStatsText.text = shipStatsTranslatedText.GetTranslatedText();
        }
    }



    public void PressShipStatsButton()
    {
        if (shipStatsButtonEnabled)
        {
            PlayerPrefs.SetInt("ShowShipStatsLastButtonState", 0);
            DisableShipStats();
        }
        else
        {
            PlayerPrefs.SetInt("ShowShipStatsLastButtonState", 1);
            EnableShipStats();
        }
    }
    void EnableShipStats()
    {
        shipStatsButtonEnabled = true;
        shipStatsText.gameObject.SetActive(true);
        Color oldBackgroundColor = infoButton.GetComponent<Image>().color;
        infoButton.GetComponent<Image>().color = new Color(oldBackgroundColor.r, oldBackgroundColor.g, oldBackgroundColor.b, 0.35f);

        Image image = infoButton.transform.Find("Image").GetComponent<Image>();
        image.sprite = infoButtonEnabledSprite;
        Color oldImageColor = image.color;
        image.color = new Color(oldImageColor.r, oldImageColor.g, oldImageColor.b, 1f);
    }
    void DisableShipStats()
    {
        shipStatsButtonEnabled = false;
        shipStatsText.gameObject.SetActive(false);
        Color oldColor = infoButton.GetComponent<Image>().color;
        infoButton.GetComponent<Image>().color = new Color(oldColor.r, oldColor.g, oldColor.b, 0.15f);

        Image image = infoButton.transform.Find("Image").GetComponent<Image>();
        image.sprite = infoButtonDisabledSprite;
        Color oldImageColor = image.color;
        image.color = new Color(oldImageColor.r, oldImageColor.g, oldImageColor.b, 0.7f);
    }


    public void ClickRevertButton()
    {
        TryFoundShipStats();
        if (shipStats != null)
        {
            if (shipStats.pastHistory.Length > 0)
            {
                DataOperator.instance.PlayUISound(clickSound, clickSoundVolume);
                shipStats.BackInTime();
            } 
        }
    }

    public void ClickRepeatButton()
    {
        TryFoundShipStats();
        if (shipStats != null)
        {
            if (shipStats.futureHistory.Length > 0)
            {
                DataOperator.instance.PlayUISound(clickSound, clickSoundVolume);
                shipStats.ForwardInTime();
            } 
        }
    }

    void ChangeTimeButtonsVisualState()
    {
        TryFoundShipStats();
        if (shipStats != null)
        {
            if (shipStats.pastHistory.Length > 0)
            {
                Color oldBackgroundColor = revertButton.color;
                revertButton.color = new Color(oldBackgroundColor.r, oldBackgroundColor.g, oldBackgroundColor.b, 0.35f);

                Color oldImageColor = revertButton.transform.Find("Image").GetComponent<Image>().color;
                revertButton.transform.Find("Image").GetComponent<Image>().color = new Color(oldImageColor.r, oldImageColor.g, oldImageColor.b, 0.6f);
            }
            else
            {
                Color oldBackgroundColor = revertButton.color;
                revertButton.color = new Color(oldBackgroundColor.r, oldBackgroundColor.g, oldBackgroundColor.b, 0.15f);

                Color oldImageColor = revertButton.transform.Find("Image").GetComponent<Image>().color;
                revertButton.transform.Find("Image").GetComponent<Image>().color = new Color(oldImageColor.r, oldImageColor.g, oldImageColor.b, 0.3f);
            }

            if (shipStats.futureHistory.Length > 0)
            {
                Color oldBackgroundColor = repeatButton.color;
                repeatButton.color = new Color(oldBackgroundColor.r, oldBackgroundColor.g, oldBackgroundColor.b, 0.35f);

                Color oldImageColor = repeatButton.transform.Find("Image").GetComponent<Image>().color;
                repeatButton.transform.Find("Image").GetComponent<Image>().color = new Color(oldImageColor.r, oldImageColor.g, oldImageColor.b, 0.6f);
            }
            else
            {
                Color oldBackgroundColor = repeatButton.color;
                repeatButton.color = new Color(oldBackgroundColor.r, oldBackgroundColor.g, oldBackgroundColor.b, 0.15f);

                Color oldImageColor = repeatButton.transform.Find("Image").GetComponent<Image>().color;
                repeatButton.transform.Find("Image").GetComponent<Image>().color = new Color(oldImageColor.r, oldImageColor.g, oldImageColor.b, 0.3f);
            }
        }
    }

    public void ClickOKButton()
    {
        TryFoundShipStats();
        shipStats.CalculateShipStats();

        //проверка есть ли на корабле блок управления, двигатели, оружие
        bool controlBlockExists = false;
        bool engineExists = false;
        bool weaponExists = false;
        foreach (ModuleOnShipData moduleOnShip in shipStats.modulesOnShip)
        {
            //блок управления
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ModuleData>().type == ModuleData.types.ControlModules)
                controlBlockExists = true;

            //двигатели
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ModuleData>().category == ModuleData.categories.Engines)
                engineExists = true;

            //оружие
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ModuleData>().category == ModuleData.categories.Weapons)
                weaponExists = true;
        }
        if (!controlBlockExists)
        {
            TranslatedText warningMessageText = new TranslatedText();
            warningMessageText.RussianText = "Не установлен блок управления - вы не сможете управлять кораблём, всё равно продолжить?";
            warningMessageText.EnglishText = "No control block installed - you won't be able to control the ship, still continue?";
            applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedText();
            applyingWarningPanel.SetActive(true);
            return;
        }
        if (!engineExists)
        {
            TranslatedText warningMessageText = new TranslatedText();
            warningMessageText.RussianText = "Не установлено ни одного двигателя, всё равно продолжить?";
            warningMessageText.EnglishText = "No engine installed, still continue?";
            applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedText();
            applyingWarningPanel.SetActive(true);
            return;
        }
        if (!weaponExists)
        {
            TranslatedText warningMessageText = new TranslatedText();
            warningMessageText.RussianText = "На корабле нет оружия - вы сможете нанести урон только тараном, всё равно продолжить?";
            warningMessageText.EnglishText = "There are no weapons on the ship - you can only do damage with a battering ram, still continue?";
            applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedText();
            applyingWarningPanel.SetActive(true);
            return;
        }

        //проверка хватает ли энергии на двигатели
        if (shipStats.totalEnginesConsumption > shipStats.totalEnergyGeneration)
        {
            TranslatedText warningMessageText = new TranslatedText();
            warningMessageText.RussianText = "Недостаточно генераторов энергии для непрерывной работы двигателей, всё равно продолжить?";
            warningMessageText.EnglishText = "Not enough power generators to keep the engines running continuously, still continue?";
            applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedText();
            applyingWarningPanel.SetActive(true);
            return;
        }

        LoadOKScene();
    }

    public void LoadOKScene()
    {
        LoadScene(OKScene);
    }

    void LoadScene(UnityEngine.Object scene)
    {
        SceneManager.LoadScene(scene.name);
    }
}



[Serializable]
public class ModulesOnStorageData : ICloneable
{
    public Module module;
    public int amount;
    public ModulesOnStorageData(Module module_, int amount_)
    {
        module = module_;
        amount = amount_;
    }

    public object Clone()
    {
        return new ModulesOnStorageData((Module)module.Clone(), amount);
    }
}

[Serializable]
public class Module : ICloneable
{
    public int moduleNum;
    public ModuleUpgrade[] moduleUpgrades;

    public Module(int moduleNum_, ModuleUpgrade[] moduleUpgrades_)
    {
        moduleNum = moduleNum_;
        moduleUpgrades = moduleUpgrades_;
    }

    public object Clone()
    {
        ModuleUpgrade[] moduleUpgradesCloning = new ModuleUpgrade[moduleUpgrades.Length];
        for (int upgradeNum = 0; upgradeNum < moduleUpgrades.Length; upgradeNum++)
        {
            moduleUpgradesCloning[upgradeNum] = (ModuleUpgrade)moduleUpgrades[upgradeNum].Clone();
        }
        return new Module(moduleNum, (ModuleUpgrade[])moduleUpgradesCloning.Clone());
    }
}

[Serializable]
public class ModuleUpgrade : ICloneable
{
    public ModuleUpgradesTypes upgradeType;
    public float upgradeMod;

    public ModuleUpgrade(ModuleUpgradesTypes upgradeType_, float upgradeMod_)
    {
        upgradeType = upgradeType_;
        upgradeMod = upgradeMod_;
    }

    public object Clone()
    {
        return new ModuleUpgrade(upgradeType, upgradeMod);
    }
}

public enum ModuleUpgradesTypes
{
    mass, //масса
    maxHP, //прочность
    energyGeneration, //генерация энергии
    energyMaxCapacity, //энергоёмкость
    damage, //урон
    projectileTimelife, //время жизни снаряда
    projectileMass //масса

    //дописывать новые типы улучшений только СНИЗУ!
}