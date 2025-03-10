using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ModulesMenu : MonoBehaviour
{
    [Header("���������")]
    //public GameObject[] DataOperator.instance.modulesPrefabs;
    [SerializeField] private GameObject moduleSlotPrefab;
    [SerializeField] private RectTransform scrollingContent;

    [SerializeField] private GameObject modulesList;
    [SerializeField] private GameObject moduleParametres;
    [SerializeField] private RectTransform moduleInfoContent;
    [SerializeField] private Image moduleParametresImage;
    [SerializeField] private Text moduleParametresName;
    [SerializeField] private TextMeshProUGUI moduleParametresInfo;
    public ModuleInstallationErrorMessage moduleInstallationErrorMessageComponent;
    [SerializeField] private GameObject noModulesOnStorageText;
    [SerializeField] private TextMeshProUGUI shipStatsText;

    [SerializeField] private Image revertButton;
    [SerializeField] private Image repeatButton;

    [SerializeField] private GameObject infoButton;
    [SerializeField] private Sprite infoButtonEnabledSprite;
    [SerializeField] private Sprite infoButtonDisabledSprite;

    [SerializeField] private GameObject applyingWarningPanel;
    [SerializeField] private string OKSceneName;

    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float clickSoundVolume = 1;

    //����� ��������� �������
    [SerializeField] private GameObject weaponSlot;
    [SerializeField] private GameObject defenseSlot;
    [SerializeField] private GameObject energySlot;
    [SerializeField] private GameObject enginesSlot;
    [SerializeField] private GameObject dronesSlot;
    [SerializeField] private GameObject specialSlot;

    [Header("�������")]
    [SerializeField] private bool give999Modules;
    [SerializeField] private GameObject[] menuSlots;
    private ItemData[] modulesComponents;
    private modulesCategories categoryFilter = modulesCategories.None;
    private float moduleInfoContentStartYPos;

    //ModuleData.types typeFilter = ModuleData.types.None;

    //����� ���� ������� ������� �� ������
    private bool weaponCategoryExists;
    private bool defenseCategoryExists;
    private bool energyCategoryExists;
    private bool enginesCategoryExists;
    private bool dronesCategoryExists;
    private bool specialCategoryExists;
    [HideInInspector] public ShipStats shipStats;
    private bool shipStatsButtonEnabled;

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
        modulesComponents = new ItemData[DataOperator.instance.modulesPrefabs.Length];
        for (int i = 0; i < DataOperator.instance.modulesPrefabs.Length; i++)
        {
            modulesComponents[i] = DataOperator.instance.modulesPrefabs[i].GetComponent<ItemData>();
        }
        RenderMenuSlosts();
        BackFromModuleParametres();
    }

    private void TryFoundShipStats()
    {
        if (shipStats == null)
        {
            shipStats = (ShipStats)FindFirstObjectByType(typeof(ShipStats));
        }
    }

    private void CalculateModulesCategoriesAndTypes() //������ ������� ���� ����? �������� � ���� �����������
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
            modulesCategories category = modulesComponents[moduleOnStorageData.module.moduleNum].Category;
            if (category == modulesCategories.Weapon)
            {
                weaponCategoryExists = true;
            }

            if (category == modulesCategories.DefenceModules)
            {
                defenseCategoryExists = true;
            }

            if (category == modulesCategories.EnergyBlocks)
            {
                energyCategoryExists = true;
            }

            if (category == modulesCategories.Engines)
            {
                enginesCategoryExists = true;
            }

            if (category == modulesCategories.Drones)
            {
                dronesCategoryExists = true;
            }

            if (category == modulesCategories.SpecialModules)
            {
                specialCategoryExists = true;
            }
        }
    }

    public void RenderMenuSlosts()
    {
        SetShipStats();
        CalculateModulesCategoriesAndTypes();
        RemoveAllMenuSlots();
        BackFromModuleParametres();
        ChangeTimeButtonsVisualState();

        if (categoryFilter == modulesCategories.None) //���������� �� ����������
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
                {
                    _ = AddSlot(weaponSlot);
                }

                if (defenseCategoryExists)
                {
                    _ = AddSlot(defenseSlot);
                }

                if (energyCategoryExists)
                {
                    _ = AddSlot(energySlot);
                }

                if (enginesCategoryExists)
                {
                    _ = AddSlot(enginesSlot);
                }

                if (dronesCategoryExists)
                {
                    _ = AddSlot(dronesSlot);
                }

                if (specialCategoryExists)
                {
                    _ = AddSlot(specialSlot);
                }
            }
        }
        if (categoryFilter == modulesCategories.Weapon)
        {
            GameObject slot_ = AddSlot(weaponSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromWeaponModules";
            AddSlotsOfCategory(modulesCategories.Weapon);
        }
        if (categoryFilter == modulesCategories.DefenceModules)
        {
            GameObject slot_ = AddSlot(defenseSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromDefenseModules";
            AddSlotsOfCategory(modulesCategories.DefenceModules);
        }
        if (categoryFilter == modulesCategories.EnergyBlocks)
        {
            GameObject slot_ = AddSlot(energySlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromEnergyModules";
            AddSlotsOfCategory(modulesCategories.EnergyBlocks);
        }
        if (categoryFilter == modulesCategories.Engines)
        {
            GameObject slot_ = AddSlot(enginesSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromEngineModules";
            AddSlotsOfCategory(modulesCategories.Engines);
        }
        if (categoryFilter == modulesCategories.Drones)
        {
            GameObject slot_ = AddSlot(dronesSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromDroneModules";
            AddSlotsOfCategory(modulesCategories.Drones);
        }
        if (categoryFilter == modulesCategories.SpecialModules)
        {
            GameObject slot_ = AddSlot(specialSlot);
            slot_.GetComponent<ModulesMenuSlot>().behaviour = "backFromSpecialModules";
            AddSlotsOfCategory(modulesCategories.SpecialModules);
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

    private void AddSlotsOfCategory(modulesCategories category)
    {
        ModulesOnStorageData[] modulesOnStorageData = DataOperator.instance.GetModulesOnStorageDataClonedArray();
        foreach (ModulesOnStorageData moduleOnStorageData in modulesOnStorageData)
        {
            GameObject modulePrefab = DataOperator.instance.modulesPrefabs[moduleOnStorageData.module.moduleNum];
            if (modulePrefab.GetComponent<ItemData>().Category == category)
            {
                GameObject slot = AddSlot(moduleSlotPrefab);
                slot.GetComponent<ModulesMenuSlot>().SetModuleData(moduleOnStorageData.module);
            }
        }
    }

    private GameObject AddSlot(GameObject slot)
    {
        Array.Resize(ref menuSlots, menuSlots.Length + 1);
        GameObject slot_ = Instantiate(slot, scrollingContent);
        slot_.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, -5 - ((menuSlots.Length - 1) * 85), 0);
        menuSlots[^1] = slot_;
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

    private void Give999Modules()
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
        categoryFilter = modulesCategories.None;
        RenderMenuSlosts();
    }

    public void ShowWeaponModules()
    {
        categoryFilter = modulesCategories.Weapon;
        RenderMenuSlosts();
    }
    public void ShowDefenseModules()
    {
        categoryFilter = modulesCategories.DefenceModules;
        RenderMenuSlosts();
    }
    public void ShowEnergyModules()
    {
        categoryFilter = modulesCategories.EnergyBlocks;
        RenderMenuSlosts();
    }
    public void ShowEngineModules()
    {
        categoryFilter = modulesCategories.Engines;
        RenderMenuSlosts();
    }
    public void ShowDroneModules()
    {
        categoryFilter = modulesCategories.Drones;
        RenderMenuSlosts();
    }
    public void ShowSpecialModules()
    {
        categoryFilter = modulesCategories.SpecialModules;
        RenderMenuSlosts();
    }

    public void ShowModuleParametres(Module module)
    {
        GameObject modulePrefab = DataOperator.instance.modulesPrefabs[module.moduleNum];

        moduleParametresImage.sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteRenderer>().sprite;
        moduleParametresName.text = modulePrefab.GetComponent<ItemData>().Name.GetTranslatedString();
        moduleParametresInfo.text = GetModuleParametresInfo(module).GetTranslatedString();

        moduleInfoContent.position = new Vector2(moduleInfoContent.position.x, moduleInfoContentStartYPos);
        moduleParametres.SetActive(true);
        modulesList.SetActive(false);
    }

    public void BackFromModuleParametres()
    {
        moduleParametres.SetActive(false);
        modulesList.SetActive(true);
    }

    private TranslatedText GetModuleParametresInfo(Module module)
    {
        TranslatedText text = new();

        GameObject modulePrefab = DataOperator.instance.modulesPrefabs[module.moduleNum];
        ItemData itemData = modulePrefab.GetComponent<ItemData>();
        Durability durabilityComponent = modulePrefab.GetComponent<Durability>();
        Engine engineComponent = modulePrefab.GetComponent<Engine>();
        _ = modulePrefab.GetComponent<Weapon>();
        EnergyGenerator generatorComponent = modulePrefab.GetComponent<EnergyGenerator>();
        Battery batteryComponent = modulePrefab.GetComponent<Battery>();

        text.RussianText += "�����: " + DataOperator.instance.RoundFloat(itemData.Mass);
        text.EnglishText += "Mass: " + DataOperator.instance.RoundFloat(itemData.Mass);
        if (durabilityComponent != null)
        {
            text.RussianText += "\n���������: " + DataOperator.instance.RoundFloat(durabilityComponent.durability.maxDurability);
            text.EnglishText += "\nDurability: " + DataOperator.instance.RoundFloat(durabilityComponent.durability.maxDurability);

            if (durabilityComponent.durability.resistanceToPhysicalDamage > 0)
            {
                text.RussianText += "\n������������� � ����������� �����: " + DataOperator.instance.RoundFloat(durabilityComponent.durability.resistanceToPhysicalDamage * 100) + "%";
                text.EnglishText += "\nResistance to physical damage: " + DataOperator.instance.RoundFloat(durabilityComponent.durability.resistanceToPhysicalDamage * 100) + "%";
            }
            if (durabilityComponent.durability.resistanceToFireDamage > 0)
            {
                text.RussianText += "\n������������� � ��������� �����: " + DataOperator.instance.RoundFloat(durabilityComponent.durability.resistanceToFireDamage * 100) + "%";
                text.EnglishText += "\nResistance to heat damage: " + DataOperator.instance.RoundFloat(durabilityComponent.durability.resistanceToFireDamage * 100) + "%";
            }
            if (durabilityComponent.durability.resistanceToEnergyDamage > 0)
            {
                text.RussianText += "\n������������� � ������ �����: " + DataOperator.instance.RoundFloat(durabilityComponent.durability.resistanceToEnergyDamage * 100) + "%";
                text.EnglishText += "\nResistance to energy damage: " + DataOperator.instance.RoundFloat(durabilityComponent.durability.resistanceToEnergyDamage * 100) + "%";
            }
        }
        else
        {
            TranslatedText debugText = new()
            {
                RussianText = "�� ������� ������ " + modulePrefab.name + " ����������� ��������� Durability, ������� ������ ���� �� ���� ������� (�� �������� �� ��������� ������)",
                EnglishText = "On the module's prefab " + modulePrefab.name + " is missing Durability component, which should be on all modules (it is responsible for the maxHP of the module)"
            };
            Debug.LogError(debugText.GetTranslatedString());
            return debugText;
        }
        if (generatorComponent != null)
        {
            text.RussianText += "\n��������� ������� � �������: " + DataOperator.instance.RoundFloat(generatorComponent.power);
            text.EnglishText += "\nEnergy generation per second: " + DataOperator.instance.RoundFloat(generatorComponent.power);
        }
        if (batteryComponent != null)
        {
            text.RussianText += "\n����� �������: " + DataOperator.instance.RoundFloat(batteryComponent.maxCapacity);
            text.EnglishText += "\nEnergy reserve: " + DataOperator.instance.RoundFloat(batteryComponent.maxCapacity);
        }
        if (engineComponent != null)
        {
            text.RussianText += "\n����: " + DataOperator.instance.RoundFloat(engineComponent.accelerationPower);
            text.EnglishText += "\nThrust: " + DataOperator.instance.RoundFloat(engineComponent.accelerationPower);

            text.RussianText += "\n�������� ������: " + DataOperator.instance.RoundFloat(engineComponent.angularPower);
            text.EnglishText += "\nTorque: " + DataOperator.instance.RoundFloat(engineComponent.angularPower);

            text.RussianText += "\n����������� ������� � �������: " + DataOperator.instance.RoundFloat(engineComponent.powerConsumption);
            text.EnglishText += "\nPower consumption per second: " + DataOperator.instance.RoundFloat(engineComponent.powerConsumption);
        }

        text.RussianText += "\n\n" + itemData.Description.RussianText;
        text.EnglishText += "\n\n" + itemData.Description.EnglishText;
        return text;
    }

    public void RemoveAllModulesFromShip()
    {
        SlotsPutter slotsPutter = (SlotsPutter)FindFirstObjectByType(typeof(SlotsPutter));
        ShipStats shipInstalledModulesData;
        if (slotsPutter != null)
        {
            shipInstalledModulesData = slotsPutter.ItemData.GetComponent<ShipStats>();
        }
        else
        {
            return;
        }

        if (shipInstalledModulesData == null)
        {
            return;
        }

        shipInstalledModulesData.RemoveAllModules();
    }

    public void SetShipStats()
    {
        TryFoundShipStats();

        if (shipStats != null)
        {
            shipStats.CalculateShipStats();

            TranslatedText shipStatsTranslatedText = new();
            shipStatsTranslatedText.RussianText += "�����: " + DataOperator.instance.RoundFloat(shipStats.totalMass);
            shipStatsTranslatedText.EnglishText += "Mass: " + DataOperator.instance.RoundFloat(shipStats.totalMass);

            shipStatsTranslatedText.RussianText += "\n����� �������: " + DataOperator.instance.RoundFloat(shipStats.totalEnergyCapacity);
            shipStatsTranslatedText.EnglishText += "\nEnergy capacity: " + DataOperator.instance.RoundFloat(shipStats.totalEnergyCapacity);

            shipStatsTranslatedText.RussianText += "\n��������� �������: " + DataOperator.instance.RoundFloat(shipStats.totalEnergyGeneration);
            shipStatsTranslatedText.EnglishText += "\nEnergy generation: " + DataOperator.instance.RoundFloat(shipStats.totalEnergyGeneration);

            if (shipStats.totalEnginesConsumption > 0)
            {
                if (shipStats.totalEnginesConsumption < shipStats.totalEnergyGeneration / 2)
                {
                    shipStatsTranslatedText.RussianText += "\n����������� �����������: <color=green>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines �onsumption: <color=green>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
                if (shipStats.totalEnginesConsumption >= shipStats.totalEnergyGeneration / 2 && shipStats.totalEnginesConsumption <= shipStats.totalEnergyGeneration)
                {
                    shipStatsTranslatedText.RussianText += "\n����������� �����������: <color=yellow>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines �onsumption: <color=yellow>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
                if (shipStats.totalEnginesConsumption > shipStats.totalEnergyGeneration)
                {
                    shipStatsTranslatedText.RussianText += "\n����������� �����������: <color=red>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines �onsumption: <color=red>" + DataOperator.instance.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
            }

            if (shipStats.totalWeaponConsumption > 0)
            {
                shipStatsTranslatedText.RussianText += "\n����������� �����������: " + DataOperator.instance.RoundFloat(shipStats.totalWeaponConsumption);
                shipStatsTranslatedText.EnglishText += "\nWeapons consumption: " + DataOperator.instance.RoundFloat(shipStats.totalWeaponConsumption);
            }

            if (shipStats.totalSystemsConsumption > 0)
            {
                shipStatsTranslatedText.RussianText += "\n����������� ���������: " + DataOperator.instance.RoundFloat(shipStats.totalSystemsConsumption);
                shipStatsTranslatedText.EnglishText += "\nSystems consumption: " + DataOperator.instance.RoundFloat(shipStats.totalSystemsConsumption);
            }

            shipStatsTranslatedText.RussianText += "\n��������: " + DataOperator.instance.RoundFloat(shipStats.totalSpeed);
            shipStatsTranslatedText.EnglishText += "\nSpeed: " + DataOperator.instance.RoundFloat(shipStats.totalSpeed);

            shipStatsTranslatedText.RussianText += "\n�������� ��������: " + DataOperator.instance.RoundFloat(shipStats.totalAngularSpeed);
            shipStatsTranslatedText.EnglishText += "\nRotation speed: " + DataOperator.instance.RoundFloat(shipStats.totalAngularSpeed);


            shipStatsText.text = shipStatsTranslatedText.GetTranslatedString();
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

    private void EnableShipStats()
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

    private void DisableShipStats()
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

    private void ChangeTimeButtonsVisualState()
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

        //�������� ���� �� �� ������� ���� ����������, ���������, ������
        bool controlBlockExists = false;
        bool engineExists = false;
        bool weaponExists = false;
        foreach (ModuleOnShipData moduleOnShip in shipStats.modulesOnShip)
        {
            //���� ����������
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ItemData>().Type == modulesTypes.ControlModules)
            {
                controlBlockExists = true;
            }

            //���������
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ItemData>().Category == modulesCategories.Engines)
            {
                engineExists = true;
            }

            //������
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ItemData>().Category == modulesCategories.Weapon)
            {
                weaponExists = true;
            }
        }
        if (!controlBlockExists)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = "�� ���������� ���� ���������� - �� �� ������� ��������� �������, �� ����� ����������?",
                EnglishText = "No control block installed - you won't be able to control the ship, still continue?"
            };
            applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedString();
            applyingWarningPanel.SetActive(true);
            return;
        }
        if (!engineExists)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = "�� ����������� �� ������ ���������, �� ����� ����������?",
                EnglishText = "No engine installed, still continue?"
            };
            applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedString();
            applyingWarningPanel.SetActive(true);
            return;
        }
        if (!weaponExists)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = "�� ������� ��� ������ - �� ������� ������� ���� ������ �������, �� ����� ����������?",
                EnglishText = "There are no weapons on the ship - you can only do damage with a battering ram, still continue?"
            };
            applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedString();
            applyingWarningPanel.SetActive(true);
            return;
        }

        //�������� ������� �� ������� �� ���������
        if (shipStats.totalEnginesConsumption > shipStats.totalEnergyGeneration)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = "������������ ����������� ������� ��� ����������� ������ ����������, �� ����� ����������?",
                EnglishText = "Not enough power generators to keep the engines running continuously, still continue?"
            };
            applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedString();
            applyingWarningPanel.SetActive(true);
            return;
        }

        LoadOKScene();
    }

    public void LoadOKScene()
    {
        DataOperator.ChangeScene(OKSceneName);
    }
}



[Serializable]
public struct ModulesOnStorageData
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
public struct Module : INetworkSerializable
{
    public int moduleNum;
    public ModuleUpgrade[] moduleUpgrades;

    public Module(int moduleNum_, ModuleUpgrade[] moduleUpgrades_)
    {
        moduleNum = moduleNum_;
        moduleUpgrades = moduleUpgrades_;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref moduleNum);

        // Length
        int length = 0;
        if (!serializer.IsReader)
        {
            length = moduleUpgrades.Length;
        }

        serializer.SerializeValue(ref length);

        // Array
        if (serializer.IsReader)
        {
            moduleUpgrades = new ModuleUpgrade[length];
        }

        for (int n = 0; n < length; ++n)
        {
            serializer.SerializeValue(ref moduleUpgrades[n]);
        }
    }
}

[Serializable]
public struct ModuleUpgrade : INetworkSerializable
{
    public ModuleUpgradesTypes upgradeType;
    public float upgradeMod;

    public ModuleUpgrade(ModuleUpgradesTypes upgradeType_, float upgradeMod_)
    {
        upgradeType = upgradeType_;
        upgradeMod = upgradeMod_;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref upgradeType);
        serializer.SerializeValue(ref upgradeMod);
    }
}

public enum ModuleUpgradesTypes
{
    mass, //�����
    maxHP, //���������
    energyGeneration, //��������� �������
    energyMaxCapacity, //������������
    damage, //����
    projectileTimelife, //����� ����� �������
    projectileMass //�����

    //���������� ����� ���� ��������� ������ �����!
}