using UnityEngine;

public class DraggingModule : MonoBehaviour
{
    [Header("Ќастройка")]
    [SerializeField] GameObject greenSlot;
    [SerializeField] GameObject redSlot;
    [SerializeField] AudioClip errorSound;

    [HideInInspector] public string moduleDataName;
    GameObject modulePrefab;
    bool playingOnPhone;
    Camera camera_;
    SpriteRenderer spriteRenderer;
    ItemData moduleData;
    ItemData shipData;
    ShipStats shipInstalledModulesData;
    SlotsPutter slotsPutter;
    Vector2 lastFrameRoundedMousePointInUnits;
    bool canBeInstalled;
    ModulesMenu modulesMenu;

    private void Start()
    {
        cellsUI = new GameObject[0];
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (Input.touchCount > 0)
        {
            playingOnPhone = true;
        }
        slotsPutter = (SlotsPutter)FindFirstObjectByType(typeof(SlotsPutter));
        TryFoundShipData();
        camera_ = (Camera)FindFirstObjectByType(typeof(Camera));
        modulesMenu = (ModulesMenu)FindFirstObjectByType(typeof(ModulesMenu));
        ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName);
        modulePrefab = modulesMenu.modulesPrefabs[modulesOnStorageData.module.moduleNum];
        moduleData = modulePrefab.GetComponent<ItemData>();
        RenderAllCellsUIAndCheckIfModuleFits();
        Update();
    }

    void TryFoundShipData()
    {
        if (shipData == null)
        {
            shipData = slotsPutter.itemData;
        }
        if (shipInstalledModulesData == null)
        {
            shipInstalledModulesData = slotsPutter.itemData.GetComponent<ShipStats>();
        }
    }

    private void Update()
    {
        if (playingOnPhone)
        {
            if (Input.touchCount != 1)
            {
                TryPutModule();
            }
            else
            {
                Vector2 mousePos = Input.GetTouch(0).position;
                mousePos = new Vector2(mousePos.x, mousePos.y + ((float)Screen.height / 8)); //смещает модуль над пальцем при игре на телефоне
                if (mousePos.y > Screen.height)
                {
                    mousePos = new Vector2(mousePos.x, Screen.height);
                }
                lastFrameRoundedMousePointInUnits = GetRoundedMousePointInUnits(mousePos);
            }
        }
        else
        {
            if (Input.GetMouseButtonUp(0))
            {
                TryPutModule();
            }
            else
            {
                lastFrameRoundedMousePointInUnits = GetRoundedMousePointInUnits(Input.mousePosition);
            }
        }
        if (lastFrameRoundedMousePointInUnits != new Vector2(transform.position.x, transform.position.y))
        {
            DragModule();
        }
    }

    void DragModule()
    {
        transform.position = lastFrameRoundedMousePointInUnits;
        RenderAllCellsUIAndCheckIfModuleFits();
    }

    void TryPutModule()
    {
        if (canBeInstalled)
        {
            //устанавливаем модуль на корабль
            TryFoundShipData();
            shipInstalledModulesData.AddModuleInArray(moduleDataName, transform.position);
            //забираем одну штуку со склада
            ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName);
            modulesOnStorageData.amount -= 1;
            DataOperator.instance.SaveData(moduleDataName, modulesOnStorageData);
            //обновл€ем список предметов в меню
            modulesMenu.RenderMenuSlosts();
        }
        else
        {
            DataOperator.instance.PlayUISound(errorSound, 1);
        }
        Destroy(gameObject);
    }

    Vector2 GetRoundedMousePointInUnits(Vector2 mousePos)
    {
        Vector2 worldMousePosInUnits = GetWorldMousePosInUnits(mousePos);
        Vector2 roundedPointInUnits = new Vector2(Mathf.RoundToInt(worldMousePosInUnits.x), Mathf.RoundToInt(worldMousePosInUnits.y));
        Vector2 cellsShift = moduleData.cellsOffset;
        roundedPointInUnits += cellsShift;
        roundedPointInUnits += shipData.cellsOffset;
        return roundedPointInUnits;
    }

    Vector2 GetWorldMousePosInUnits(Vector2 mousePos)
    {
        float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
        Vector2 pointInUnits = mousePos / pixelsPerUnit;
        pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
        Vector2 cellsShift = moduleData.cellsOffset;
        pointInUnits -= cellsShift;
        pointInUnits -= shipData.cellsOffset;
        pointInUnits += new Vector2(camera_.transform.position.x, camera_.transform.position.y);
        return pointInUnits;
    }

    //ниже - дл€ клеток под перетаскиваемым модулем
    GameObject[] cellsUI;

    void RenderAllCellsUIAndCheckIfModuleFits()
    {
        foreach (GameObject cell in cellsUI)
        {
            Destroy(cell);
        }
        canBeInstalled = true;
        Vector2 offset = moduleData.cellsOffset;
        cellsUI = new GameObject[moduleData.cellsDataX.Length];
        for (int cell = 0; cell < moduleData.cellsDataX.Length; cell++)
        {
            Vector2 globalPosition = new Vector2(moduleData.cellsDataX[cell] + transform.position.x, moduleData.cellsDataY[cell] + transform.position.y) + offset - shipData.cellsOffset;
            Vector2 localPosition = new Vector2(moduleData.cellsDataX[cell], moduleData.cellsDataY[cell]) + offset;
            GameObject slot;
            if (CheckIfSlotCanBeInstalled(globalPosition, moduleData.cellsDataType[cell]))
            {
                slot = Instantiate(greenSlot, new Vector3(), Quaternion.identity);
            }
            else
            {
                slot = Instantiate(redSlot, new Vector3(), Quaternion.identity);
                canBeInstalled = false;
            }
            if (!canBeInstalled)
            {
                spriteRenderer.color = new Color(1, 0.35f, 0.35f);
            }
            else
            {
                spriteRenderer.color = new Color(0.35f, 1, 0.35f);
            }
            cellsUI[cell] = slot;
            slot.transform.parent = transform;
            slot.transform.localPosition = new Vector3(localPosition.x * (1 / transform.localScale.x), localPosition.y * (1 / transform.localScale.y), 0);
        }
    }

    bool CheckIfSlotCanBeInstalled(Vector2 position, int slotType)
    {
        TryFoundShipData();
        if (shipData != null)
        {
            //сначала провер€ем не зан€т ли слот корабл€ другим модулем
            foreach (ModuleOnShipData moduleOnShip in shipInstalledModulesData.modulesOnShip)
            {   
                //перебираем все уже установленные модули на корабле
                GameObject modulePrefab_ = modulesMenu.modulesPrefabs[moduleOnShip.module.moduleNum];
                ItemData moduleData_ = modulePrefab_.GetComponent<ItemData>();
                for (int moduleSlot_ = 0; moduleSlot_ < moduleData_.cellsDataX.Length; moduleSlot_++)
                {
                    Vector2 moduleSlotPos = new Vector2(moduleData_.cellsDataX[moduleSlot_] + moduleOnShip.position.x, moduleData_.cellsDataY[moduleSlot_] + moduleOnShip.position.y) + moduleData_.cellsOffset - shipData.cellsOffset;
                    if (Vector2.Distance(position, moduleSlotPos) < 0.01f)
                    {
                        return false;
                    }
                }
            }

            //теперь провер€ем совместим ли слот
            for (int slot = 0; slot < shipData.cellsDataX.Length; slot++)
            {
                if (Mathf.Abs(shipData.cellsDataX[slot] - position.x) < 0.01f && Mathf.Abs(shipData.cellsDataY[slot] - position.y) < 0.01f)
                {
                    if (slotType == 0) //обычна€ €чейка модул€ который перетаскиваетс€
                    {
                        if (shipData.cellsDataType[slot] == 0 || shipData.cellsDataType[slot] == 1)
                            return true;
                    }
                    if (slotType == 1) //универсальна€ €чейка
                    {
                        return true;
                    }
                    if (slotType == 2) //€чейка дл€ двигателей
                    {
                        if (shipData.cellsDataType[slot] == 2 || shipData.cellsDataType[slot] == 1)
                            return true;
                    }
                }
            }
            return false; //если код дошЄл сюда, значит €чейка модул€ Ќ≈ над €чейкой корабл€
        }
        else
        {
            return false;
        }
    }
}
