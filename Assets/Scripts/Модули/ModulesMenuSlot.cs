using UnityEngine;
using UnityEngine.UI;

public class ModulesMenuSlot : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] GameObject[] arrows;
    public string behaviour;
    [SerializeField] AudioClip clickSound;
    [SerializeField] float clickSoundVolume = 1;

    [SerializeField] Image image;
    [SerializeField] Text name_;
    [SerializeField] Text amount;

    string moduleDataName;
    ModulesMenu modulesMenu;

    private void Start()
    {
        TryToFindModulesMenu();
        if (behaviour == "backFromWeaponModules" ||
            behaviour == "backFromDefenseModules" ||
            behaviour == "backFromEnergyModules" ||
            behaviour == "backFromEngineModules" ||
            behaviour == "backFromDroneModules" ||
            behaviour == "backFromSpecialModules")
            foreach (GameObject arrow in arrows)
                arrow.transform.localScale = new Vector3(-1, 1, 1);
    }

    public void Click()
    {
        DataOperator.instance.PlayUISound(clickSound, clickSoundVolume);
        
        if (behaviour == "weaponModulesSorting")
            modulesMenu.ShowWeaponModules();
        if (behaviour == "defenseModulesSorting")
            modulesMenu.ShowDefenseModules();
        if (behaviour == "energyModulesSorting")
            modulesMenu.ShowEnergyModules();
        if (behaviour == "engineModulesSorting")
            modulesMenu.ShowEngineModules();
        if (behaviour == "droneModulesSorting")
            modulesMenu.ShowDroneModules();
        if (behaviour == "specialModulesSorting")
            modulesMenu.ShowSpecialModules();

        if (behaviour == "backFromWeaponModules" ||
            behaviour == "backFromDefenseModules" ||
            behaviour == "backFromEnergyModules" ||
            behaviour == "backFromEngineModules" ||
            behaviour == "backFromDroneModules" ||
            behaviour == "backFromSpecialModules")
            modulesMenu.ShowAllSlots();
    }

    public void SetModuleData(string moduleDataName_)
    {
        TryToFindModulesMenu();
        moduleDataName = moduleDataName_;
        name = moduleDataName_;
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName_);
        GameObject modulePrefab = modulesMenu.modulesPrefabs[modulesOnStorageData.module.moduleNum];
        image.sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteRenderer>().sprite;
        if (DataOperator.instance.userLanguage == "Russian")
            name_.text = modulePrefab.GetComponent<ItemData>().NameRu;
        if (DataOperator.instance.userLanguage == "English")
            name_.text = modulePrefab.GetComponent<ItemData>().NameEng;
        amount.text = modulesOnStorageData.amount + "";
    }

    void TryToFindModulesMenu()
    {
        if (modulesMenu == null)
        {
            modulesMenu = (ModulesMenu)FindFirstObjectByType(typeof(ModulesMenu));
        }
    }
}
