using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ModulesMenu : MonoBehaviour
{
    [Header("Настройка")]
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

    //слоты категорий модулей
    [SerializeField] private GameObject weaponSlot;
    [SerializeField] private GameObject defenseSlot;
    [SerializeField] private GameObject energySlot;
    [SerializeField] private GameObject enginesSlot;
    [SerializeField] private GameObject dronesSlot;
    [SerializeField] private GameObject specialSlot;

    [Header("Отладка")]
    [SerializeField] private bool give999Modules;
    [SerializeField] private GameObject[] menuSlots;
    private ItemData[] modulesComponents;
    private modulesCategories categoryFilter = modulesCategories.None;
    private float moduleInfoContentStartYPos;

    //ModuleData.types typeFilter = ModuleData.types.None;

    //какие типы модулей имеются на складе
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

    private void CalculateModulesCategoriesAndTypes() //хочешь развить свой мозг? попробуй в этом разобраться
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

        if (categoryFilter == modulesCategories.None) //сортировка по категориям
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
        EnergyGenerator generatorComponent = modulePrefab.GetComponent<EnergyGenerator>();
        Battery batteryComponent = modulePrefab.GetComponent<Battery>();
        BallisticWeapon ballisticWeaponComponent = modulePrefab.GetComponent<BallisticWeapon>();
        Projectile projectileComponent = null;
        RocketEngine rocketEngineComponent = null;
        if (ballisticWeaponComponent != null)
        {
            projectileComponent = ballisticWeaponComponent.ProjectilePrefab.GetComponent<Projectile>();
            rocketEngineComponent = ballisticWeaponComponent.ProjectilePrefab.GetComponent<RocketEngine>();
        }
        LaserWeapon laserWeaponComponent = modulePrefab.GetComponent<LaserWeapon>();

        text.RussianText += "Масса: " + DataOperator.RoundFloat(itemData.Mass);
        text.EnglishText += "Mass: " + DataOperator.RoundFloat(itemData.Mass);
        if (durabilityComponent != null)
        {
            text.RussianText += "\nПрочность: " + DataOperator.RoundFloat(durabilityComponent.durability.maxDurability);
            text.EnglishText += "\nDurability: " + DataOperator.RoundFloat(durabilityComponent.durability.maxDurability);

            if (durabilityComponent.durability.resistanceToPhysicalDamage > 0)
            {
                text.RussianText += "\nСопротивление к физическому урону: " + DataOperator.RoundFloat(durabilityComponent.durability.resistanceToPhysicalDamage * 100) + "%";
                text.EnglishText += "\nResistance to physical damage: " + DataOperator.RoundFloat(durabilityComponent.durability.resistanceToPhysicalDamage * 100) + "%";
            }
            if (durabilityComponent.durability.resistanceToFireDamage > 0)
            {
                text.RussianText += "\nСопротивление к тепловому урону: " + DataOperator.RoundFloat(durabilityComponent.durability.resistanceToFireDamage * 100) + "%";
                text.EnglishText += "\nResistance to heat damage: " + DataOperator.RoundFloat(durabilityComponent.durability.resistanceToFireDamage * 100) + "%";
            }
            if (durabilityComponent.durability.resistanceToEnergyDamage > 0)
            {
                text.RussianText += "\nСопротивление к энерго урону: " + DataOperator.RoundFloat(durabilityComponent.durability.resistanceToEnergyDamage * 100) + "%";
                text.EnglishText += "\nResistance to energy damage: " + DataOperator.RoundFloat(durabilityComponent.durability.resistanceToEnergyDamage * 100) + "%";
            }
            if (durabilityComponent.ExplodeStrengthOnDestroy > 0)
            {
                text.RussianText += "\nСила взрыва при уничтожении: " + DataOperator.RoundFloat(durabilityComponent.ExplodeStrengthOnDestroy);
                text.EnglishText += "\nThe force of the explosion on destruction: " + DataOperator.RoundFloat(durabilityComponent.ExplodeStrengthOnDestroy);
            }
        }
        else
        {
            TranslatedText debugText = new()
            {
                RussianText = "На префабе модуля " + modulePrefab.name + " отсутствует компонент Durability, который должен быть на всех модулях (он отвечает за прочность модуля)",
                EnglishText = "On the module's prefab " + modulePrefab.name + " is missing Durability component, which should be on all modules (it is responsible for the maxHP of the module)"
            };
            Debug.LogError(debugText.GetTranslatedString());
            return debugText;
        }
        if (generatorComponent != null)
        {
            text.RussianText += "\nГенерация энергии в секунду: " + DataOperator.RoundFloat(generatorComponent.power);
            text.EnglishText += "\nEnergy generation per second: " + DataOperator.RoundFloat(generatorComponent.power);
        }
        if (batteryComponent != null)
        {
            text.RussianText += "\nЗапас энергии: " + DataOperator.RoundFloat(batteryComponent.maxCapacity);
            text.EnglishText += "\nEnergy reserve: " + DataOperator.RoundFloat(batteryComponent.maxCapacity);
        }
        if (engineComponent != null)
        {
            text.RussianText += "\nТяга: " + DataOperator.RoundFloat(engineComponent.accelerationPower);
            text.EnglishText += "\nThrust: " + DataOperator.RoundFloat(engineComponent.accelerationPower);

            text.RussianText += "\nКрутящий момент: " + DataOperator.RoundFloat(engineComponent.angularPower);
            text.EnglishText += "\nTorque: " + DataOperator.RoundFloat(engineComponent.angularPower);

            text.RussianText += "\nПотребление энергии в секунду: " + DataOperator.RoundFloat(engineComponent.powerConsumption);
            text.EnglishText += "\nPower consumption per second: " + DataOperator.RoundFloat(engineComponent.powerConsumption);
        }
        if (laserWeaponComponent != null)
        {
            if (laserWeaponComponent.DamagePerSecond.physicalDamage > 0)
            {
                text.RussianText += $"\nФизический урон в секунду: {DataOperator.RoundFloat(laserWeaponComponent.DamagePerSecond.physicalDamage)}";
                text.EnglishText += $"\nPhysical damage per second: {DataOperator.RoundFloat(laserWeaponComponent.DamagePerSecond.physicalDamage)}";
            }
            if (laserWeaponComponent.DamagePerSecond.fireDamage > 0)
            {
                text.RussianText += $"\nТепловой урон в секунду: {DataOperator.RoundFloat(laserWeaponComponent.DamagePerSecond.fireDamage)}";
                text.EnglishText += $"\nHeat damage per second: {DataOperator.RoundFloat(laserWeaponComponent.DamagePerSecond.fireDamage)}";
            }
            if (laserWeaponComponent.DamagePerSecond.energyDamage > 0)
            {
                text.RussianText += $"\nЭнергетический урон в секунду: {DataOperator.RoundFloat(laserWeaponComponent.DamagePerSecond.energyDamage)}";
                text.EnglishText += $"\nEnergy damage per second: {DataOperator.RoundFloat(laserWeaponComponent.DamagePerSecond.energyDamage)}";
            }
            text.RussianText += $"\nЭнергопотребление в секунду: {DataOperator.RoundFloat(laserWeaponComponent.EnergyPerSecond)}";
            text.EnglishText += $"\nPower consumption per second: {DataOperator.RoundFloat(laserWeaponComponent.EnergyPerSecond)}";

            text.RussianText += $"\nДальность: {DataOperator.RoundFloat(laserWeaponComponent.MaxLaserDistance)}";
            text.EnglishText += $"\nRange: {DataOperator.RoundFloat(laserWeaponComponent.MaxLaserDistance)}";
        }
        if (ballisticWeaponComponent != null)
        {
            text.RussianText += $"\nПерезарядка: {DataOperator.RoundFloat(ballisticWeaponComponent.ReloadTime)}с";
            text.EnglishText += $"\nReload time: {DataOperator.RoundFloat(ballisticWeaponComponent.ReloadTime)}s";

            text.RussianText += $"\nЭнергопотребление на залп: {DataOperator.RoundFloat(ballisticWeaponComponent.EnergyConsumption)}";
            text.EnglishText += $"\nPower consumption per volley: {DataOperator.RoundFloat(ballisticWeaponComponent.EnergyConsumption)}";

            if (ballisticWeaponComponent.ProjectilesPerSalvo > 1)
            {
                text.RussianText += $"\nСнарядов в залпе: {DataOperator.RoundFloat(ballisticWeaponComponent.ProjectilesPerSalvo)}";
                text.EnglishText += $"\nProjectiles per salvo: {DataOperator.RoundFloat(ballisticWeaponComponent.ProjectilesPerSalvo)}";
            }

            text.RussianText += $"\nУгол разброса: {DataOperator.RoundFloat(ballisticWeaponComponent.ScatterAngle)}°";
            text.EnglishText += $"\nScatter angle: {DataOperator.RoundFloat(ballisticWeaponComponent.ScatterAngle)}°";
        }
        if (projectileComponent != null)
        {
            text.RussianText += $"\n\nХарактеристики снаряда: ";
            text.EnglishText += $"\n\nProjectile сharacteristics: ";

            if (projectileComponent.Damage.physicalDamage > 0)
            {
                text.RussianText += $"\nФизический урон: {DataOperator.RoundFloat(projectileComponent.Damage.physicalDamage)}";
                text.EnglishText += $"\nPhysical damage: {DataOperator.RoundFloat(projectileComponent.Damage.physicalDamage)}";
            }
            if (projectileComponent.Damage.fireDamage > 0)
            {
                text.RussianText += $"\nТепловой урон: {DataOperator.RoundFloat(projectileComponent.Damage.fireDamage)}";
                text.EnglishText += $"\nHeat damage: {DataOperator.RoundFloat(projectileComponent.Damage.fireDamage)}";
            }
            if (projectileComponent.Damage.energyDamage > 0)
            {
                text.RussianText += $"\nЭнергетический урон: {DataOperator.RoundFloat(projectileComponent.Damage.energyDamage)}";
                text.EnglishText += $"\nEnergy damage: {DataOperator.RoundFloat(projectileComponent.Damage.energyDamage)}";
            }
            if (projectileComponent.ShockWavePower > 0)
            {
                text.RussianText += $"\nМощь взрывной волны: {DataOperator.RoundFloat(projectileComponent.ShockWavePower)}";
                text.EnglishText += $"\nShock wave power: {DataOperator.RoundFloat(projectileComponent.ShockWavePower)}";
            }
            if (rocketEngineComponent == null)
            {
                text.RussianText += $"\nСкорость: {DataOperator.RoundFloat(projectileComponent.StartSpeed)}";
                text.EnglishText += $"\nSpeed: {DataOperator.RoundFloat(projectileComponent.StartSpeed)}";
            }
            else
            {
                text.RussianText += $"\nСкорость: {DataOperator.RoundFloat(projectileComponent.StartSpeed)}-{DataOperator.RoundFloat(rocketEngineComponent.MaxSpeed)}";
                text.EnglishText += $"\nSpeed: {DataOperator.RoundFloat(projectileComponent.StartSpeed)}-{DataOperator.RoundFloat(rocketEngineComponent.MaxSpeed)}";
            }
            text.RussianText += $"\nМасса: {DataOperator.RoundFloat(projectileComponent.Mass) / 1000f}";
            text.EnglishText += $"\nMass: {DataOperator.RoundFloat(projectileComponent.Mass) / 1000f}";

            if (rocketEngineComponent == null)
            {
                text.RussianText += $"\nДальность: {DataOperator.RoundFloat(projectileComponent.StartSpeed * projectileComponent.Lifetime)}";
                text.EnglishText += $"\nRange: {DataOperator.RoundFloat(projectileComponent.StartSpeed * projectileComponent.Lifetime)}";
            }
            else
            {
                text.RussianText += $"\nВремя полёта: {DataOperator.RoundFloat(projectileComponent.Lifetime)}с";
                text.EnglishText += $"\nFlight time: {DataOperator.RoundFloat(projectileComponent.Lifetime)}s";
            }
        }
        if (rocketEngineComponent != null)
        {
            text.RussianText += $"\nСкорость поворота: {DataOperator.RoundFloat(rocketEngineComponent.MaxRotateSpeed)}°/с";
            text.EnglishText += $"\nRotate speed: {DataOperator.RoundFloat(rocketEngineComponent.MaxRotateSpeed)}°/s";

            text.RussianText += $"\nДальность поиска целей: {DataOperator.RoundFloat(rocketEngineComponent.TargetsSearchingRadius)}";
            text.EnglishText += $"\nTarget search range: {DataOperator.RoundFloat(rocketEngineComponent.TargetsSearchingRadius)}";
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
            shipStatsTranslatedText.RussianText += "Масса: " + DataOperator.RoundFloat(shipStats.totalMass);
            shipStatsTranslatedText.EnglishText += "Mass: " + DataOperator.RoundFloat(shipStats.totalMass);

            shipStatsTranslatedText.RussianText += "\nЗапас энергии: " + DataOperator.RoundFloat(shipStats.totalEnergyCapacity);
            shipStatsTranslatedText.EnglishText += "\nEnergy capacity: " + DataOperator.RoundFloat(shipStats.totalEnergyCapacity);

            shipStatsTranslatedText.RussianText += "\nГенерация энергии: " + DataOperator.RoundFloat(shipStats.totalEnergyGeneration);
            shipStatsTranslatedText.EnglishText += "\nEnergy generation: " + DataOperator.RoundFloat(shipStats.totalEnergyGeneration);

            if (shipStats.totalEnginesConsumption > 0)
            {
                if (shipStats.totalEnginesConsumption < shipStats.totalEnergyGeneration / 2)
                {
                    shipStatsTranslatedText.RussianText += "\nПотребление двигателями: <color=green>" + DataOperator.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines сonsumption: <color=green>" + DataOperator.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
                if (shipStats.totalEnginesConsumption >= shipStats.totalEnergyGeneration / 2 && shipStats.totalEnginesConsumption <= shipStats.totalEnergyGeneration)
                {
                    shipStatsTranslatedText.RussianText += "\nПотребление двигателями: <color=yellow>" + DataOperator.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines сonsumption: <color=yellow>" + DataOperator.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
                if (shipStats.totalEnginesConsumption > shipStats.totalEnergyGeneration)
                {
                    shipStatsTranslatedText.RussianText += "\nПотребление двигателями: <color=red>" + DataOperator.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                    shipStatsTranslatedText.EnglishText += "\nEngines сonsumption: <color=red>" + DataOperator.RoundFloat(shipStats.totalEnginesConsumption) + "</color>";
                }
            }

            if (shipStats.totalWeaponConsumption > 0)
            {
                shipStatsTranslatedText.RussianText += "\nПотребление вооружением: " + DataOperator.RoundFloat(shipStats.totalWeaponConsumption);
                shipStatsTranslatedText.EnglishText += "\nWeapons consumption: " + DataOperator.RoundFloat(shipStats.totalWeaponConsumption);
            }

            if (shipStats.totalSystemsConsumption > 0)
            {
                shipStatsTranslatedText.RussianText += "\nПотребление системами: " + DataOperator.RoundFloat(shipStats.totalSystemsConsumption);
                shipStatsTranslatedText.EnglishText += "\nSystems consumption: " + DataOperator.RoundFloat(shipStats.totalSystemsConsumption);
            }

            shipStatsTranslatedText.RussianText += "\nСкорость: " + DataOperator.RoundFloat(shipStats.totalSpeed);
            shipStatsTranslatedText.EnglishText += "\nSpeed: " + DataOperator.RoundFloat(shipStats.totalSpeed);

            shipStatsTranslatedText.RussianText += "\nСкорость поворота: " + DataOperator.RoundFloat(shipStats.totalAngularSpeed);
            shipStatsTranslatedText.EnglishText += "\nRotation speed: " + DataOperator.RoundFloat(shipStats.totalAngularSpeed);


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

        //проверка есть ли на корабле блок управления, двигатели, оружие
        bool controlBlockExists = false;
        bool engineExists = false;
        bool weaponExists = false;
        foreach (ModuleOnShipData moduleOnShip in shipStats.modulesOnShip)
        {
            //блок управления
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ItemData>().Type == modulesTypes.ControlModules)
            {
                controlBlockExists = true;
            }

            //двигатели
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ItemData>().Category == modulesCategories.Engines)
            {
                engineExists = true;
            }

            //оружие
            if (DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ItemData>().Category == modulesCategories.Weapon)
            {
                weaponExists = true;
            }
        }
        if (!controlBlockExists)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = "Не установлен блок управления - вы не сможете управлять кораблём, всё равно продолжить?",
                EnglishText = "No control block installed - you won't be able to control the ship, still continue?"
            };
            ShowBuildWarning(warningMessageText);
            return;
        }
        if (!engineExists)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = "Не установлено ни одного двигателя, всё равно продолжить?",
                EnglishText = "No engine installed, still continue?"
            };
            ShowBuildWarning(warningMessageText);
            return;
        }
        if (!weaponExists)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = "На корабле нет оружия - вы сможете нанести урон только тараном, всё равно продолжить?",
                EnglishText = "There are no weapons on the ship - you can only do damage with a battering ram, still continue?"
            };
            ShowBuildWarning(warningMessageText);
            return;
        }

        //проверка хватает ли энергии на двигатели
        if (shipStats.totalEnginesConsumption > shipStats.totalEnergyGeneration)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = "Недостаточно генераторов энергии для непрерывной работы двигателей, всё равно продолжить?",
                EnglishText = "Not enough power generators to keep the engines running continuously, still continue?"
            };
            ShowBuildWarning(warningMessageText);
            return;
        }

        float totalEnergyCapacity = shipStats.totalEnergyCapacity;
        float maxEnergyPerSalvo = -1;
        string weaponName = "";

        foreach (ModuleOnShipData moduleOnShipData in shipStats.modulesOnShip)
        {
            GameObject modulePrefab = DataOperator.instance.modulesPrefabs[moduleOnShipData.module.moduleNum];
            BallisticWeapon ballisticWeaponComponent = modulePrefab.GetComponent<BallisticWeapon>();
            if (ballisticWeaponComponent != null && ballisticWeaponComponent.EnergyConsumption > maxEnergyPerSalvo)
            {
                maxEnergyPerSalvo = ballisticWeaponComponent.EnergyConsumption;
                weaponName = modulePrefab.GetComponent<ItemData>().Name.GetTranslatedString();
            }
        }

        if (maxEnergyPerSalvo > totalEnergyCapacity)
        {
            TranslatedText warningMessageText = new()
            {
                RussianText = $"Недостаточно батарей для залпа из {weaponName}, всё равно продолжить?",
                EnglishText = $"Not enough batteries to salvo from {weaponName}, still continue?"
            };
            ShowBuildWarning(warningMessageText);
            return;
        }

        LoadOKScene();
    }

    public void LoadOKScene()
    {
        DataOperator.ChangeScene(OKSceneName);
    }

    private void ShowBuildWarning(TranslatedText warningMessageText)
    {
        applyingWarningPanel.transform.Find("Text").GetComponent<Text>().text = warningMessageText.GetTranslatedString();
        applyingWarningPanel.SetActive(true);
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
    mass, //масса
    maxHP, //прочность
    energyGeneration, //генерация энергии
    energyMaxCapacity, //энергоёмкость
    damage, //урон
    projectileTimelife, //время жизни снаряда
    projectileMass //масса

    //дописывать новые типы улучшений только СНИЗУ!
}