using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModulesMenuSlot : MonoBehaviour, IPointerDownHandler
{
    [Header("���������")]
    [SerializeField] GameObject[] arrows;
    public string behaviour;
    [SerializeField] AudioClip clickSound;
    [SerializeField] float clickSoundVolume = 1;
    [SerializeField] GameObject moduleDragging; //���������� ��� �������������� ������

    [SerializeField] Image image;
    [SerializeField] Text name_;
    [SerializeField] Text amount;

    string moduleDataName;
    ModulesMenu modulesMenu;
    ScrollRect scrollingMenu;
    RectTransform scrollingContent;

    private void Start()
    {
        scrollingContent = transform.parent.GetComponent<RectTransform>();
        scrollingMenu = GetComponentInParent<ScrollRect>();
        TryToFindModulesMenu();
        if (behaviour == "backFromWeaponModules" ||
            behaviour == "backFromDefenseModules" ||
            behaviour == "backFromEnergyModules" ||
            behaviour == "backFromEngineModules" ||
            behaviour == "backFromDroneModules" ||
            behaviour == "backFromSpecialModules")
            foreach (GameObject arrow in arrows)
            {
                arrow.transform.localScale = new Vector3(-1, 1, 1);
            } 
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

        if (behaviour == "moduleSlot")
        {
            TryToFindModulesMenu();
            modulesMenu.ShowModuleParametres(DataOperator.instance.LoadDataModulesOnStorage(moduleDataName).module);
        }
        if (behaviour == "backFromModuleParametres")
        {
            TryToFindModulesMenu();
            modulesMenu.BackFromModuleParametres();
        }
    }

    public void SetModuleData(string moduleDataName_)
    {
        TryToFindModulesMenu();
        moduleDataName = moduleDataName_;
        behaviour = "moduleSlot";
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName_);
        GameObject modulePrefab = modulesMenu.modulesPrefabs[modulesOnStorageData.module.moduleNum];
        image.sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteRenderer>().sprite;
        name = modulePrefab.GetComponent<ItemData>().Name.EnglishText;
        name_.text = modulePrefab.GetComponent<ItemData>().Name.GetTranslatedText();
        amount.text = modulesOnStorageData.amount + "";
    }

    void TryToFindModulesMenu()
    {
        if (modulesMenu == null)
        {
            modulesMenu = (ModulesMenu)FindFirstObjectByType(typeof(ModulesMenu));
        }
    }


    PointerEventData touch;
    bool checkingClick; //true ����� ������������, ������ �� ���� ����� ���������� ���� ��� ��������� ���
    [SerializeField] float xPosChangeForSlotPutting;
    [SerializeField] float yPosChangeForCancellingSlotPutting;
    Vector2 startTouchPoint;
    float maxTimer = 1f;
    float timer;
    float menuYPosWhenClicked;

    public void OnPointerDown(PointerEventData eventData) //���������� ��� ������� �� ����
    {
        if (behaviour == "moduleSlot")
        {
            scrollingMenu.enabled = true;
            touch = eventData;
            checkingClick = true;
            startTouchPoint = eventData.position;
            timer = 0;
            menuYPosWhenClicked = scrollingContent.position.y;
        }
    }

    private void OnDisable()
    {
        checkingClick = false;
    }

    void Update()
    {
        if (behaviour == "moduleSlot" && checkingClick)
        {
            if (Input.GetMouseButtonUp(0))
            {
                checkingClick = false;
            }
            timer += Time.deltaTime;
            if (timer > maxTimer)
            {
                checkingClick = false;
                scrollingMenu.enabled = true;
            }

            if (Input.touchCount < 2 && touch.position.x < startTouchPoint.x - xPosChangeForSlotPutting)
            {
                //������� �������������� ������ �� ����
                checkingClick = false;
                if (CheckIfModuleCanBeInstalled())
                {
                    DataOperator.instance.PlayUISound(clickSound, clickSoundVolume);
                    scrollingContent.position = new Vector3(scrollingContent.position.x, menuYPosWhenClicked, scrollingContent.position.z);
                    scrollingMenu.enabled = false;
                    CreateModuleDragging();
                }
            }
            if (Mathf.Abs(startTouchPoint.y - touch.position.y) > yPosChangeForCancellingSlotPutting)
            {
                //������, ����� ����� ������� ����
                checkingClick = false;
            }
        }
    }

    bool CheckIfModuleCanBeInstalled()
    {
        //�������� �� ������� �� ������
        if (DataOperator.instance.LoadDataModulesOnStorage(moduleDataName).amount <= 0)
        {
            return false;
        }

        //�������� ����� ���������� (�� ����� ���� ������ ���� �� �������)
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName);
        GameObject modulePrefab = modulesMenu.modulesPrefabs[modulesOnStorageData.module.moduleNum];
        if (modulePrefab.GetComponent<ModuleData>().type == ModuleData.types.ControlModules)
        {
            SlotsPutter slotsPutter = (SlotsPutter)FindFirstObjectByType(typeof(SlotsPutter));
            ShipStats shipInstalledModulesData;
            if (slotsPutter != null)
                shipInstalledModulesData = slotsPutter.itemData.GetComponent<ShipStats>();
            else
                return false;
            if (shipInstalledModulesData == null)
                return false;

            foreach (ModuleOnShipData moduleOnShip in shipInstalledModulesData.modulesOnShip)
            {
                if (modulesMenu.modulesPrefabs[moduleOnShip.module.moduleNum].GetComponent<ModuleData>().type == ModuleData.types.ControlModules)
                {
                    TranslatedText errorMessageText = new TranslatedText();
                    errorMessageText.RussianText = "�� ������� ����� ���� ���������� ������ 1 ���� ����������";
                    errorMessageText.EnglishText = "Only 1 control block can be installed on a ship";
                    modulesMenu.moduleInstallationErrorMessageComponent.ShowErrorMessage(errorMessageText.GetTranslatedText());
                    return false;
                }
            }
        }
        
        //���� ����� ���� �������� ������� �� ������� false, ���������� true
        return true;
    }

    //�������� GameObject ��� �������������� �� ���� �� �������
    void CreateModuleDragging()
    {
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName);
        GameObject modulePrefab = modulesMenu.modulesPrefabs[modulesOnStorageData.module.moduleNum];
        GameObject moduleDragging_ = Instantiate(moduleDragging, new Vector3(), Quaternion.identity);

        moduleDragging_.name = modulePrefab.name + " (���������������)";
        moduleDragging_.GetComponent<SpriteRenderer>().sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteRenderer>().sprite;
        moduleDragging_.transform.localScale = modulePrefab.transform.Find("Image").localScale;
        moduleDragging_.GetComponent<DraggingModule>().moduleDataName = moduleDataName;
    }
}
