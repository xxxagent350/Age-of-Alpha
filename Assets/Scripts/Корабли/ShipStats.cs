using System;
using Unity.Netcode;
using UnityEngine;

public class ShipStats : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private GameObject UIModulePrefab;

    [Header("Отладка")]
    //ниже параметры корабля вместе с модулями
    public float totalMass; //масса
    public float totalEnergyCapacity; //макс. запас энергии
    public float totalEnergyGeneration; //генерация энергии
    public float totalEnginesConsumption; //потребление движками
    public float totalWeaponConsumption; //потребление оружием
    public float totalSystemsConsumption; //потребление системами(дроны, лазерная защита, ремонтный бот и т. д.)
    public float totalAccelerationPower; //общая ускорительная мощь двигателей
    public float totalAngularAccelerationPower; //общая угловая ускорительная мощь двигателей
    public float totalSpeed; //скорость и ускорение
    public float totalAngularSpeed; //угловая скорость и ускорение

    public ModuleOnShipData[] modulesOnShip;

    //история расстановки модулей для стрелочек вперёд и назад
    public ModulesInstallingHistory[] pastHistory;
    public ModulesInstallingHistory[] futureHistory;

    [HideInInspector] public string teamID; //ID команды. Одинаковый - союзники, разный - враги
    private string shipName;
    private GameObject[] modulesUI;
    private ItemData myItemData;
    private ModulesMenu modulesMenu;
    private bool _shipBuildingScene;

    private void Awake()
    {
        _shipBuildingScene = GameObject.Find("ModulesMenu") != null;

        Initialize();
        if (_shipBuildingScene)
        {
            InitializeForShipBuildingScene();
        }
    }

    public void Initialize()
    {
        shipName = GetComponent<ItemData>().Name.EnglishText;
        TryFoundItemData();
    }

    private void TryFoundItemData()
    {
        if (myItemData == null)
        {
            myItemData = GetComponent<ItemData>();
        }
    }

    private void InitializeForShipBuildingScene()
    {
        modulesOnShip = DataOperator.instance.LoadDataModulesOnShip("ModulesOnShipData(" + shipName + ")");
        //Debug.Log("1: " + modulesOnShip.Length);
        modulesOnShip ??= new ModuleOnShipData[0];
        pastHistory = new ModulesInstallingHistory[0];
        futureHistory = new ModulesInstallingHistory[0];
        TryFoundModulesMenu();
        modulesUI = new GameObject[0];
        RenderAllModulesOnShip();
    }

    private void RenderAllModulesOnShip()
    {
        for (int moduleNum = 0; moduleNum < modulesOnShip.Length; moduleNum++)
        {
            RenderModuleUI(modulesOnShip[moduleNum].module, modulesOnShip[moduleNum].position.GetVector2());
        }
    }

    public void AddModule(Module moduleAdding, Vector2 position, Times time)
    {
        Array.Resize(ref modulesOnShip, modulesOnShip.Length + 1);
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleAdding);
        Module module = modulesOnStorageData.module;
        modulesOnShip[^1] = new ModuleOnShipData(module, position);
        if (time == Times.Past)
        {
            Array.Resize(ref pastHistory, pastHistory.Length - 1);
            Array.Resize(ref futureHistory, futureHistory.Length + 1);
            futureHistory[^1] = new ModulesInstallingHistory(module, position, true);
        }
        if (time == Times.Present)
        {
            futureHistory = new ModulesInstallingHistory[0];
            Array.Resize(ref pastHistory, pastHistory.Length + 1);
            pastHistory[^1] = new ModulesInstallingHistory(module, position, true);
        }
        if (time == Times.Future)
        {
            Array.Resize(ref pastHistory, pastHistory.Length + 1);
            Array.Resize(ref futureHistory, futureHistory.Length - 1);
            pastHistory[^1] = new ModulesInstallingHistory(module, position, true);
        }
        RenderModuleUI(module, position);
        DataOperator.instance.SaveData("ModulesOnShipData(" + shipName + ")", modulesOnShip);
    }

    public void RenderModuleUI(Module module, Vector2 position)
    {
        GameObject modulePrefab = DataOperator.instance.modulesPrefabs[module.moduleNum];
        GameObject UImoduleGO = Instantiate(UIModulePrefab, position, Quaternion.identity);
        UImoduleGO.name = modulePrefab.name + " (UI)";
        UImoduleGO.GetComponent<SpriteRenderer>().sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteRenderer>().sprite;
        UImoduleGO.transform.localScale = modulePrefab.transform.Find("Image").localScale;

        Array.Resize(ref modulesUI, modulesUI.Length + 1);
        modulesUI[^1] = UImoduleGO;
    }

    private void OnDisable()
    {
        if (_shipBuildingScene)
        {
            DestroyAllUIModules();
        }
    }

    public void DestroyAllUIModules()
    {
        foreach (GameObject moduleUI in modulesUI)
        {
            Destroy(moduleUI);
        }
        modulesUI = new GameObject[0];
    }

    public void RemoveAllModules()
    {
        TryFoundModulesMenu();
        for (int moduleNum = 0; moduleNum < modulesOnShip.Length; moduleNum++)
        {
            ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(modulesOnShip[moduleNum].module);
            modulesOnStorageData.amount += 1;
            DataOperator.instance.SaveData(modulesOnStorageData);
        }
        modulesOnShip = new ModuleOnShipData[0];
        pastHistory = new ModulesInstallingHistory[0];
        futureHistory = new ModulesInstallingHistory[0];
        DataOperator.instance.SaveData("ModulesOnShipData(" + shipName + ")", modulesOnShip);
        DestroyAllUIModules();
        modulesMenu.RenderMenuSlosts();
    }

    public void RemoveModule(Vector2 position, Times time)
    {
        for (int moduleNum = 0; moduleNum < modulesOnShip.Length; moduleNum++)
        {
            if (Vector2.Distance(position, modulesOnShip[moduleNum].position.GetVector2()) < 0.01f)
            {
                if (time == Times.Past)
                {
                    Array.Resize(ref pastHistory, pastHistory.Length - 1);
                    Array.Resize(ref futureHistory, futureHistory.Length + 1);
                    futureHistory[^1] = new ModulesInstallingHistory(modulesOnShip[moduleNum].module, position, false);
                }
                if (time == Times.Present)
                {
                    futureHistory = new ModulesInstallingHistory[0];
                    Array.Resize(ref pastHistory, pastHistory.Length + 1);
                    pastHistory[^1] = new ModulesInstallingHistory(modulesOnShip[moduleNum].module, position, false);
                }
                if (time == Times.Future)
                {
                    Array.Resize(ref pastHistory, pastHistory.Length + 1);
                    Array.Resize(ref futureHistory, futureHistory.Length - 1);
                    pastHistory[^1] = new ModulesInstallingHistory(modulesOnShip[moduleNum].module, position, false);
                }

                if (moduleNum != modulesOnShip.Length - 1)
                {
                    for (int movingModule = moduleNum; movingModule < modulesOnShip.Length - 1; movingModule++)
                    {
                        modulesOnShip[movingModule] = modulesOnShip[movingModule + 1];
                    }
                }

                Array.Resize(ref modulesOnShip, modulesOnShip.Length - 1);
                RemoveModuleUI(moduleNum);
                break;
            }
        }
        DataOperator.instance.SaveData("ModulesOnShipData(" + shipName + ")", modulesOnShip);
    }

    public void RemoveModuleUI(int numInArray)
    {
        Destroy(modulesUI[numInArray]);
        if (numInArray != modulesUI.Length - 1)
        {
            for (int movingModule = numInArray; movingModule < modulesUI.Length - 1; movingModule++)
            {
                modulesUI[movingModule] = modulesUI[movingModule + 1];
            }
        }
        Array.Resize(ref modulesUI, modulesUI.Length - 1);
    }

    private void TryFoundModulesMenu()
    {
        if (modulesMenu == null)
        {
            modulesMenu = (ModulesMenu)FindFirstObjectByType(typeof(ModulesMenu));
        }
    }

    public void CalculateShipStats()
    {
        TryFoundItemData();
        totalMass = 0;
        totalEnergyCapacity = 0;
        totalEnergyGeneration = 0;
        totalEnginesConsumption = 0;
        totalWeaponConsumption = 0;
        totalSystemsConsumption = 0;
        totalAccelerationPower = 0;
        totalAngularAccelerationPower = 0;
        totalSpeed = 0;
        totalAngularSpeed = 0;

        totalMass += myItemData.Mass;

        for (int moduleOnShipNum = 0; moduleOnShipNum < modulesOnShip.Length; moduleOnShipNum++)
        {
            int modulePrefabNum = modulesOnShip[moduleOnShipNum].module.moduleNum;
            GameObject modulePrefab = DataOperator.instance.modulesPrefabs[modulePrefabNum];

            ItemData moduleItemData = modulePrefab.GetComponent<ItemData>();
            Battery moduleBattery = modulePrefab.GetComponent<Battery>();
            EnergyGenerator moduleEnergyGenerator = modulePrefab.GetComponent<EnergyGenerator>();
            Engine moduleEngine = modulePrefab.GetComponent<Engine>();
            _ = modulePrefab.GetComponent<Weapon>();

            if (moduleItemData != null)
            {
                float moduleMass = moduleItemData.Mass;
                totalMass += moduleMass;
            }
            if (moduleBattery != null)
            {
                float moduleEnergyCapacity = moduleBattery.maxCapacity;
                totalEnergyCapacity += moduleEnergyCapacity;
            }
            if (moduleEnergyGenerator != null)
            {
                float moduleEnergyGeneration = moduleEnergyGenerator.power;
                totalEnergyGeneration += moduleEnergyGeneration;
            }
            if (moduleEngine != null)
            {
                float moduleEngineConsumption = moduleEngine.powerConsumption;
                totalEnginesConsumption += moduleEngineConsumption;

                float moduleEngineAccelerationPower = moduleEngine.accelerationPower;
                totalAccelerationPower += moduleEngineAccelerationPower;

                float moduleEngineAngularAccelerationPower = moduleEngine.angularPower;
                totalAngularAccelerationPower += moduleEngineAngularAccelerationPower;
            }
        }

        totalSpeed = totalAccelerationPower / totalMass * 100;
        totalAngularSpeed = totalAngularAccelerationPower / totalMass * 100;
    }

    public bool ControlBlockExists()
    {
        for (int moduleOnShipNum = 0; moduleOnShipNum < modulesOnShip.Length; moduleOnShipNum++)
        {
            int modulePrefabNum = modulesOnShip[moduleOnShipNum].module.moduleNum;
            GameObject modulePrefab = DataOperator.instance.modulesPrefabs[modulePrefabNum];
            ItemData moduleItemData = modulePrefab.GetComponent<ItemData>();
            if (moduleItemData.Type == modulesTypes.ControlModules)
            {
                return true;
            }
        }

        return false;
    }


    public void BackInTime()
    {
        if (pastHistory.Length > 0)
        {
            ModulesInstallingHistory targetModuleInstallingHistory = pastHistory[^1];
            if (targetModuleInstallingHistory.moduleInstalled == true) //модуль был установлен, снимаем
            {
                //добавляем одну штуку на склад
                ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(targetModuleInstallingHistory.module);
                modulesOnStorageData.amount += 1;
                DataOperator.instance.SaveData(modulesOnStorageData);

                //снимаем с корабля
                RemoveModule(targetModuleInstallingHistory.position, Times.Past);
            }
            else //модуль был снят, ставим обратно
            {
                ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(targetModuleInstallingHistory.module);

                if (modulesOnStorageData.amount > 0)
                {
                    //ставим модуль на корабль
                    AddModule(targetModuleInstallingHistory.module, targetModuleInstallingHistory.position, Times.Past);

                    //забираем одну штуку со склада
                    modulesOnStorageData.amount -= 1;
                    DataOperator.instance.SaveData(modulesOnStorageData);
                }
            }
        }
    }

    public void ForwardInTime()
    {
        if (futureHistory.Length > 0)
        {
            ModulesInstallingHistory targetModuleInstallingHistory = futureHistory[^1];
            if (targetModuleInstallingHistory.moduleInstalled == true) //модуль был установлен, снимаем
            {
                //добавляем одну штуку на склад
                ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(targetModuleInstallingHistory.module);
                modulesOnStorageData.amount += 1;
                DataOperator.instance.SaveData(modulesOnStorageData);

                //снимаем с корабля
                RemoveModule(targetModuleInstallingHistory.position, Times.Future);
            }
            else //модуль был снят, ставим обратно
            {
                ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(targetModuleInstallingHistory.module);

                if (modulesOnStorageData.amount > 0)
                {
                    //ставим модуль на корабль
                    AddModule(targetModuleInstallingHistory.module, targetModuleInstallingHistory.position, Times.Future);

                    //забираем одну штуку со склада
                    modulesOnStorageData.amount -= 1;
                    DataOperator.instance.SaveData(modulesOnStorageData);
                }
            }
        }
    }
}


[Serializable]
public struct ModuleOnShipData : INetworkSerializable
{
    public Module module;
    public Vector2Serializable position;

    public ModuleOnShipData(Module module_, Vector2 position_)
    {
        module = module_;
        position = new Vector2Serializable(position_);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        module.NetworkSerialize(serializer);
        position.NetworkSerialize(serializer);
    }
}

[Serializable]
public class ModulesInstallingHistory
{
    public Vector2 position;
    public Module module;
    public bool moduleInstalled; //true - модуль был установлен, false - модуль был снят

    public ModulesInstallingHistory(Module module_, Vector2 position_, bool moduleInstalled_)
    {
        position = position_;
        module = module_;
        moduleInstalled = moduleInstalled_;
    }
}

public enum Times
{
    Past,
    Present,
    Future
}
