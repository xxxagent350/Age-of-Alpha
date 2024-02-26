using System;
using UnityEngine;

public class ShipStats : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] GameObject UIModulePrefab;

    [Header("Отладка")]
    [HideInInspector] public string teamID;
    public ModuleOnShipData[] modulesOnShip;
    [SerializeField] float modulesCollidersKeepActiveTime = 3;
    float modulesCollidersKeepActiveTimer;
    bool modulesCollidersActive = true;
    public bool spawnBullet;//
    public GameObject bullet;//

    GameObject[] modulesUI;
    float radiansAngle;
    ModulesMenu modulesMenu;

    private void Awake()
    {
        modulesUI = new GameObject[0];
        teamID = UnityEngine.Random.Range(1000000000, 2000000000) + "" + UnityEngine.Random.Range(1000000000, 2000000000) + "" + UnityEngine.Random.Range(1000000000, 2000000000);
        ModulesCollidersSetActive(false);
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

    public void AddModuleInArray(string moduleDataName, Vector2 position)
    {
        if (modulesMenu == null)
        {
            modulesMenu = (ModulesMenu)FindFirstObjectByType(typeof(ModulesMenu));
        }
        Array.Resize(ref modulesOnShip, modulesOnShip.Length + 1);
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName);
        Module module = modulesOnStorageData.module;

        modulesOnShip[modulesOnShip.Length - 1] = new ModuleOnShipData(module, position);
        RenderModuleUI(module, position);
    }

    public void RenderModuleUI(Module module, Vector2 position)
    {
        GameObject modulePrefab = modulesMenu.modulesPrefabs[module.moduleNum];
        GameObject UImoduleGO = Instantiate(UIModulePrefab, position, Quaternion.identity);
        UImoduleGO.name = modulePrefab.name + " (UI)";
        UImoduleGO.GetComponent<SpriteRenderer>().sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteRenderer>().sprite;
        UImoduleGO.transform.localScale = modulePrefab.transform.Find("Image").localScale;

        Array.Resize(ref modulesUI, modulesUI.Length + 1);
        modulesUI[modulesUI.Length - 1] = UImoduleGO;
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

}


[Serializable]
public class ModuleOnShipData
{
    public Module module;
    public Vector2 position;

    public ModuleOnShipData(Module module_, Vector2 position_)
    {
        module = module_;
        position = position_;
    }
}
