using System;
using UnityEngine;

public class ShipStats : MonoBehaviour
{
    [Header("���������")]
    [SerializeField] GameObject UIModulePrefab;

    [Header("�������")]
    public ModuleOnShipData[] modulesOnShip;

    //���� ��������� ������� ������ � ��������
    public float totalMass; //�����
    public float totalEnergyCapacity; //����. ����� �������
    public float totalEnergyGeneration; //��������� �������
    public float totalEnginesConsumption; //����������� ��������
    public float totalWeaponConsumption; //����������� �������
    public float totalSystemsConsumption; //����������� ���������(�����, �������� ������, ��������� ��� � �. �.)
    public float totalAccelerationPower; //����� ������������� ���� ����������
    public float totalAngularAccelerationPower; //����� ������� ������������� ���� ����������
    public float totalSpeed; //�������� � ���������
    public float totalAngularSpeed; //������� �������� � ���������

    public bool spawnBullet;//
    public GameObject bullet;//

    [HideInInspector] public string teamID; //ID �������. ���������� - ��������, ������ - �����
    float energyGeneration; //��������� ��������� ������� �� ���� �������
    float energyCapacity; //������������ ���������� ���������� ������� �� ���� �������
    float maxAcceleration; //������������ ��������� �� ���� ����������
    float maxAngularRotation; //������������ ������� ��������� �� ���� ����������
    string shipName;
    GameObject[] modulesUI;

    ItemData myItemData;
    float radiansAngle;
    ModulesMenu modulesMenu;
    [SerializeField] float modulesCollidersKeepActiveTime = 3;
    float modulesCollidersKeepActiveTimer;
    bool modulesCollidersActive = true;

    private void Start()
    {
        TryFoundModulesMenu();
        shipName = GetComponent<ItemData>().Name.EnglishText;
        modulesUI = new GameObject[0];
        teamID = UnityEngine.Random.Range(1000000000, 2000000000) + "" + UnityEngine.Random.Range(1000000000, 2000000000) + "" + UnityEngine.Random.Range(1000000000, 2000000000);
        ModulesCollidersSetActive(false);
        myItemData = GetComponent<ItemData>();

        modulesOnShip = DataOperator.instance.LoadDataModulesOnShip("ModulesOnShipData(" + shipName + ")");
        if (modulesOnShip == null)
        {
            modulesOnShip = new ModuleOnShipData[0];
        }
        RenderAllModulesOnShip();
    }

    private void FixedUpdate()
    {
        radiansAngle = (transform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        modulesCollidersKeepActiveTimer += Time.deltaTime;
        if (modulesCollidersKeepActiveTimer >= modulesCollidersKeepActiveTime && modulesCollidersActive == true)
        {
            ModulesCollidersSetActive(false);
        }
        if (spawnBullet)//
        {
            spawnBullet = false;
            GameObject bulletPrepared = Instantiate(bullet, transform.position, transform.rotation);
            Bullet bulletComponent = bulletPrepared.GetComponent<Bullet>();
            bulletComponent.teamID = teamID;
            bulletComponent.CustomStart();
            bulletPrepared.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Cos(radiansAngle), Mathf.Sin(radiansAngle)) * bulletComponent.speed;
        }
    }

    void RenderAllModulesOnShip()
    {
        for (int moduleNum = 0; moduleNum < modulesOnShip.Length; moduleNum++)
        {
            RenderModuleUI(modulesOnShip[moduleNum].module, modulesOnShip[moduleNum].position.GetVector2());
        }
    }

    public void AddModule(Module moduleAdding, Vector2 position)
    {
        
        Array.Resize(ref modulesOnShip, modulesOnShip.Length + 1);
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleAdding);
        Module module = modulesOnStorageData.module;

        modulesOnShip[modulesOnShip.Length - 1] = new ModuleOnShipData(module, position);
        RenderModuleUI(module, position);
        DataOperator.instance.SaveData("ModulesOnShipData(" + shipName + ")", modulesOnShip);
    }

    public void RenderModuleUI(Module module, Vector2 position)
    {
        GameObject modulePrefab = DataOperator.instance.modulesPrefabs[module.moduleNum];
        GameObject UImoduleGO = Instantiate(UIModulePrefab, position, Quaternion.identity);
        UImoduleGO.name = modulePrefab.name + " (UI)";
        UImoduleGO.GetComponent<SpriteChanger>().sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteChanger>().sprite;
        UImoduleGO.transform.localScale = modulePrefab.transform.Find("Image").localScale;

        Array.Resize(ref modulesUI, modulesUI.Length + 1);
        modulesUI[modulesUI.Length - 1] = UImoduleGO;
    }

    public void RemoveAllModules()
    {
        TryFoundModulesMenu();
        for (int moduleNum = 0; moduleNum < modulesOnShip.Length; moduleNum++)
        {
            ModulesOnStorageData modulesOnStorageData = (ModulesOnStorageData)DataOperator.instance.LoadDataModulesOnStorage(modulesOnShip[moduleNum].module).Clone();
            modulesOnStorageData.amount += 1;
            DataOperator.instance.SaveData(modulesOnStorageData);
            Destroy(modulesUI[moduleNum]);
        }
        modulesOnShip = new ModuleOnShipData[0];
        DataOperator.instance.SaveData("ModulesOnShipData(" + shipName + ")", modulesOnShip);
        modulesUI = new GameObject[0];
        modulesMenu.RenderMenuSlosts();
    }

    public void RemoveModule(Vector2 position)
    {
        for (int moduleNum = 0; moduleNum < modulesOnShip.Length; moduleNum++)
        {
            if (Vector2.Distance(position, modulesOnShip[moduleNum].position.GetVector2()) < 0.01f)
            {
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

    void TryFoundModulesMenu()
    {
        if (modulesMenu == null)
        {
            modulesMenu = (ModulesMenu)FindFirstObjectByType(typeof(ModulesMenu));
        }
    }

    public void TakeDamage(float damage)
    {
        modulesCollidersKeepActiveTimer = 0;
        if (!modulesCollidersActive)
        {
            ModulesCollidersSetActive(true);
        }
    }

    void ModulesCollidersSetActive(bool state)
    {
        modulesCollidersActive = state;
        BoxCollider2D[] modulesColliders = GetComponentsInChildren<BoxCollider2D>();
        foreach (BoxCollider2D collider in modulesColliders)
        {
            collider.enabled = state;
        }
    }

    public void CalculateShipStats()
    {
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
            Weapon moduleWeapon = modulePrefab.GetComponent<Weapon>();

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
}


[Serializable]
public class ModuleOnShipData
{
    public Module module;
    public Vector2Serializable position;

    public ModuleOnShipData(Module module_, Vector2 position_)
    {
        module = module_;
        position = new Vector2Serializable(position_);
    }
}
