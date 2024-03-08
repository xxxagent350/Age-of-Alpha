using UnityEngine;

public class UImodulesOperator : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] float maxTimerToDragModule; //если столько времени держать палец на модуле, его можно будет передвинуть
    [SerializeField] float distanceFromPressPointToRevertDragging; //измеряется в частях высоты экрана
    [SerializeField] GameObject moduleDragging;
    [SerializeField] AudioClip moduleTakeSound;
    [SerializeField] float moduleTakeSoundVolume = 1;

    [Header("Отладка")]
    [SerializeField] Vector2 shipMousePoint; //позиция нажатия относительно корабля в юнитах
    [SerializeField] bool dragging; //сейчас перетаскивается модуль(после снятия с корабля)

    UIPressedInfo uiPressedInfo;
    CameraScaler cameraScaler; //отключаем его когда модуль перетаскивается чтобы камера стояла на месте
    Camera camera_;
    ItemData shipData;
    SlotsPutter slotsPutter;
    ShipStats shipInstalledModulesData;
    [SerializeField] float timerToDragModule;
    Vector2 clickPoint; //позиция начала нажатия
    [SerializeField] bool nowPressed; //нажато ли сейчас на экран постройки
    [SerializeField] bool notDraggingAtThisPress; //больше не пытаться перетаскивать при текущем нажатии
    ModulesMenu modulesMenu;
    Vector2 clickedModuleOnShipPos;

    private void Start()
    {
        uiPressedInfo = GetComponent<UIPressedInfo>();
        camera_ = (Camera)FindFirstObjectByType(typeof(Camera));
        cameraScaler = camera_.GetComponent<CameraScaler>();
        slotsPutter = (SlotsPutter)FindFirstObjectByType(typeof(SlotsPutter));
        modulesMenu = (ModulesMenu)FindFirstObjectByType(typeof(ModulesMenu));
        TryFoundShipData();
    }

    private void Update()
    {
        if (uiPressedInfo.publicTouchesPositions.Length == 1)
        {
            if (!dragging && !notDraggingAtThisPress)
            {
                if (!nowPressed)
                {
                    timerToDragModule = 0;
                    nowPressed = true;
                    clickPoint = uiPressedInfo.publicTouchesPositions[0];
                }
                shipMousePoint = GetRoundedMousePointInUnits(uiPressedInfo.publicTouchesPositions[0]);
                timerToDragModule += Time.deltaTime;
                if (timerToDragModule >= maxTimerToDragModule)
                {
                    //пытаемся начать перетаскивать если есть что
                    string moduleDataName = TryFoundModuleAtPos(shipMousePoint);
                    if (moduleDataName != "")
                    {
                        dragging = true;
                        CreateModuleDragging(moduleDataName);

                        //удаляем модуль с корабля и возвращаем на склад
                        TryFoundShipData();
                        shipInstalledModulesData.RemoveModule(clickedModuleOnShipPos);
                        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName);
                        modulesOnStorageData.amount += 1;
                        DataOperator.instance.SaveData(moduleDataName, modulesOnStorageData);

                        //обновляем список модулей
                        modulesMenu.RenderMenuSlosts();
                    }
                }
                if (Vector2.Distance(clickPoint, uiPressedInfo.publicTouchesPositions[0]) > Screen.height * distanceFromPressPointToRevertDragging)
                {
                    //не перетаскиваем, двигаем камеру
                    notDraggingAtThisPress = true;
                }
            }
        }
        else
        {
            if (uiPressedInfo.publicTouchesPositions.Length < 2)
            {
                if (nowPressed && !notDraggingAtThisPress && !dragging)
                {
                    //показываем параметры модуля если на него кликнули
                    string moduleDataName = TryFoundModuleAtPos(shipMousePoint);
                    if (moduleDataName != "")
                    {
                        DataOperator.instance.PlayUISound(moduleTakeSound, moduleTakeSoundVolume);
                        modulesMenu.ShowModuleParametres(DataOperator.instance.LoadDataModulesOnStorage(moduleDataName).module);
                    }
                    else
                    {
                        modulesMenu.BackFromModuleParametres();
                    }
                }
                dragging = false;
                nowPressed = false;
                notDraggingAtThisPress = false;
            }
            else
            {
                notDraggingAtThisPress = true;
            }
        }
        if (dragging || (!notDraggingAtThisPress && uiPressedInfo.publicTouchesPositions.Length < 2))
        {
            cameraScaler.dontMove = true;
        }
        else
        {
            cameraScaler.dontMove = false;
        }
    }

    void CreateModuleDragging(string moduleDataName)
    {
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName);
        GameObject modulePrefab = modulesMenu.modulesPrefabs[modulesOnStorageData.module.moduleNum];
        GameObject moduleDragging_ = Instantiate(moduleDragging, new Vector3(), Quaternion.identity);

        moduleDragging_.name = modulePrefab.name + " (перетаскивается)";
        moduleDragging_.GetComponent<SpriteRenderer>().sprite = modulePrefab.transform.Find("Image").GetComponent<SpriteRenderer>().sprite;
        moduleDragging_.transform.localScale = modulePrefab.transform.Find("Image").localScale;
        moduleDragging_.GetComponent<DraggingModule>().moduleDataName = moduleDataName;

        DataOperator.instance.PlayUISound(moduleTakeSound, moduleTakeSoundVolume);
    }

    string TryFoundModuleAtPos(Vector2 position)
    {
        TryFoundShipData();
        foreach (ModuleOnShipData moduleOnShip in shipInstalledModulesData.modulesOnShip)
        {
            //перебираем все установленные модули на корабле
            GameObject modulePrefab_ = modulesMenu.modulesPrefabs[moduleOnShip.module.moduleNum];
            ItemData moduleData_ = modulePrefab_.GetComponent<ItemData>();
            for (int moduleSlot_ = 0; moduleSlot_ < moduleData_.itemSlotsData.Length; moduleSlot_++)
            {
                Vector2 moduleSlotPos = new Vector2(moduleData_.itemSlotsData[moduleSlot_].position.x + moduleOnShip.position.x, moduleData_.itemSlotsData[moduleSlot_].position.y + moduleOnShip.position.y) + moduleData_.cellsOffset - shipData.cellsOffset;
                if (Vector2.Distance(position, moduleSlotPos) < 0.01f)
                {
                    clickedModuleOnShipPos = moduleOnShip.position.GetVector2();
                    return "ModulesOnStorageData(" + modulePrefab_.name + ")";
                }
            }
        }
        return ""; //ничего не нашли
    }

    Vector2 GetWorldMousePosInUnits(Vector2 mousePos)
    {
        float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
        Vector2 pointInUnits = mousePos / pixelsPerUnit;
        pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
        pointInUnits += new Vector2(camera_.transform.position.x, camera_.transform.position.y);
        Vector2 cellsShift = shipData.cellsOffset;
        pointInUnits -= cellsShift;
        return pointInUnits;
    }

    Vector2 GetRoundedMousePointInUnits(Vector2 mousePos)
    {
        Vector2 worldMousePosInUnits = GetWorldMousePosInUnits(mousePos);
        Vector2 roundedPointInUnits = new Vector2(Mathf.RoundToInt(worldMousePosInUnits.x), Mathf.RoundToInt(worldMousePosInUnits.y));
        return roundedPointInUnits;
    }

    void TryFoundShipData()
    {
        if (shipData == null)
        {
            shipData = slotsPutter.itemData;
        }
        if (shipData != null && shipInstalledModulesData == null)
        {
            shipInstalledModulesData = shipData.GetComponent<ShipStats>();
        }
    }
}
