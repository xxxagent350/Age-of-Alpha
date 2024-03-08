using UnityEngine;

public class DraggingModule : MonoBehaviour
{
    [Header("Ќастройка")]
    [SerializeField] GameObject greenSlot;
    [SerializeField] GameObject redSlot;
    [SerializeField] AudioClip errorSound;
    [SerializeField] float errorSoundVolume = 1;
    [SerializeField] AudioClip[] modulePutSounds;
    [Header("Ёффект сварки")]
    [SerializeField] Effect weldingEffect; //эффекты сварки

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
    Vector2 lastMousePos;
    Vector2 lastMousePosInUnits;
    [SerializeField] RectTransform menuStartLine; //син€€ полоска, после которой начинаетс€ меню модулей
    bool destroyed;

    private void Start()
    {
        menuStartLine = GameObject.Find("Canvas").transform.Find("ModulesMenu").Find("Border").Find("BorderMiddleLeft").GetComponent<RectTransform>();
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
                lastMousePos = Input.GetTouch(0).position;
                lastMousePosInUnits = GetWorldMousePosInUnits(Input.GetTouch(0).position, false);
                lastFrameRoundedMousePointInUnits = GetRoundedMousePointInUnits(Input.GetTouch(0).position);
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
                lastMousePos = Input.mousePosition;
                lastMousePosInUnits = GetWorldMousePosInUnits(Input.mousePosition, false);
                lastFrameRoundedMousePointInUnits = GetRoundedMousePointInUnits(Input.mousePosition);
            }
        }
        transform.position = lastMousePosInUnits;
        if (lastFrameRoundedMousePointInUnits != new Vector2(transform.position.x, transform.position.y))
        {
            DragModule();
        }
    }

    void DragModule()
    {
        TryFoundShipData();
        //transform.position = lastFrameRoundedMousePointInUnits;
        RenderAllCellsUIAndCheckIfModuleFits();
    }

    void TryPutModule()
    {
        if (lastMousePos.x < menuStartLine.position.x)
        {
            if (canBeInstalled)
            {   
                //задаЄм позицию модул€, подход€щую под €чейки
                transform.position = lastFrameRoundedMousePointInUnits;
                //устанавливаем модуль на корабль
                TryFoundShipData();
                shipInstalledModulesData.AddModule(moduleDataName, transform.position);
                //забираем одну штуку со склада
                ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(moduleDataName);
                modulesOnStorageData.amount -= 1;
                DataOperator.instance.SaveData(moduleDataName, modulesOnStorageData);
                //обновл€ем список предметов в меню
                modulesMenu.RenderMenuSlosts();
                //играем случайный звук установки модул€
                DataOperator.instance.PlayRandom3DSound(transform.position, modulePutSounds);
                //создаЄм эффект сварки
                CreateWeldingEffect(transform.position, spriteRenderer.sprite);
            }
            else
            {
                DataOperator.instance.PlayUISound(errorSound, errorSoundVolume);
            }
        }
        destroyed = true;
        foreach (GameObject cell in cellsUI)
        {
            Destroy(cell);
        }
        Destroy(gameObject);
    }

    Vector2 GetRoundedMousePointInUnits(Vector2 mousePos)
    {
        Vector2 worldMousePosInUnits = GetWorldMousePosInUnits(mousePos, true);
        Vector2 roundedPointInUnits = new Vector2(Mathf.RoundToInt(worldMousePosInUnits.x), Mathf.RoundToInt(worldMousePosInUnits.y));
        Vector2 cellsShift = moduleData.cellsOffset;
        roundedPointInUnits += cellsShift;
        roundedPointInUnits += shipData.cellsOffset;
        return roundedPointInUnits;
    }

    Vector2 GetWorldMousePosInUnits(Vector2 mousePos, bool withModuleAndShipOffset)
    {
        float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
        Vector2 pointInUnits = mousePos / pixelsPerUnit;
        pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
        if (withModuleAndShipOffset)
        {
            Vector2 cellsShift = moduleData.cellsOffset;
            pointInUnits -= cellsShift;
            pointInUnits -= shipData.cellsOffset;
        }
        pointInUnits += new Vector2(camera_.transform.position.x, camera_.transform.position.y);
        return pointInUnits;
    }

    //ниже - дл€ клеток под перетаскиваемым модулем
    GameObject[] cellsUI;

    void RenderAllCellsUIAndCheckIfModuleFits()
    {
        if (!destroyed)
        {
            transform.position = lastFrameRoundedMousePointInUnits;//
            foreach (GameObject cell in cellsUI)
            {
                Destroy(cell);
            }
            if (lastMousePos.x < menuStartLine.position.x)
            {
                canBeInstalled = true;
                Vector2 offset = moduleData.cellsOffset;
                cellsUI = new GameObject[moduleData.itemSlotsData.Length];
                for (int cell = 0; cell < moduleData.itemSlotsData.Length; cell++)
                {
                    Vector2 globalPosition = new Vector2(moduleData.itemSlotsData[cell].position.x + transform.position.x, moduleData.itemSlotsData[cell].position.y + transform.position.y) + offset - shipData.cellsOffset;
                    Vector2 localPosition = new Vector2(moduleData.itemSlotsData[cell].position.x, moduleData.itemSlotsData[cell].position.y) + offset;
                    GameObject slot;
                    if (CheckIfSlotCanBeInstalled(globalPosition, moduleData.itemSlotsData[cell].type))
                    {
                        slot = Instantiate(greenSlot, new Vector3(), Quaternion.identity);
                    }
                    else
                    {
                        slot = Instantiate(redSlot, new Vector3(), Quaternion.identity);
                        canBeInstalled = false;
                    }
                    cellsUI[cell] = slot;
                    //slot.transform.parent = transform;
                    //slot.transform.localPosition = new Vector3(localPosition.x * (1 / transform.localScale.x), localPosition.y * (1 / transform.localScale.y), 0);
                    slot.transform.position = globalPosition + shipData.cellsOffset;
                }
            }
            else
            {
                canBeInstalled = false;
            }
            transform.position = lastMousePosInUnits;
            if (!canBeInstalled)
            {
                spriteRenderer.color = new Color(1, 0.35f, 0.35f);
            }
            else
            {
                spriteRenderer.color = new Color(0.35f, 1, 0.35f);
            }
        }
    }

    bool CheckIfSlotCanBeInstalled(Vector2 position, slotsTypes slotType)
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
                for (int moduleSlot_ = 0; moduleSlot_ < moduleData_.itemSlotsData.Length; moduleSlot_++)
                {
                    Vector2 moduleSlotPos = new Vector2(moduleData_.itemSlotsData[moduleSlot_].position.x + moduleOnShip.position.x, moduleData_.itemSlotsData[moduleSlot_].position.y + moduleOnShip.position.y) + moduleData_.cellsOffset - shipData.cellsOffset;
                    if (Vector2.Distance(position, moduleSlotPos) < 0.01f)
                    {
                        return false;
                    }
                }
            }

            //теперь провер€ем совместим ли слот
            for (int slot = 0; slot < shipData.itemSlotsData.Length; slot++)
            {
                if (Mathf.Abs(shipData.itemSlotsData[slot].position.x - position.x) < 0.01f && Mathf.Abs(shipData.itemSlotsData[slot].position.y - position.y) < 0.01f)
                {
                    if (slotType == slotsTypes.standart) //обычна€ €чейка модул€ который перетаскиваетс€
                    {
                        if (shipData.itemSlotsData[slot].type == slotsTypes.standart || shipData.itemSlotsData[slot].type == slotsTypes.universal)
                            return true;
                    }
                    if (slotType == slotsTypes.universal) //универсальна€ €чейка
                    {
                        return true;
                    }
                    if (slotType == slotsTypes.engine) //€чейка дл€ двигателей
                    {
                        if (shipData.itemSlotsData[slot].type == slotsTypes.engine || shipData.itemSlotsData[slot].type == slotsTypes.universal)
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


    //создать эффект сварки
    void CreateWeldingEffect(Vector2 position, Sprite sprite)
    {
        Rect spriteRect = sprite.textureRect;
        Texture2D texture = sprite.texture;
        if (!texture.isReadable)
        {
            Debug.LogError("”становите галочку напротив Read/Write дл€ текстуры " + texture.name);
            return;
        }

        float raycastAccuracy = 100f; //точность райкаста
        int maxWhileIterations = 100;
        int whileIterations = 0;

        bool createdRight = false;
        bool createdUp = false;
        bool createdLeft = false;
        bool createdDown = false;
        int numCreatedSides = 0;

        int slotsNum = moduleData.itemSlotsData.Length;
        int minEffectsNum = Mathf.RoundToInt(slotsNum - Mathf.Pow(Mathf.Sqrt(slotsNum) - 2, 2) + 1) * 4;
        int effectsSpawnedNum = 0;

        while ((numCreatedSides < 3 || effectsSpawnedNum < minEffectsNum) && whileIterations < maxWhileIterations)
        {
            whileIterations++;
            int side = Random.Range(0, 4);
            
            if (side == 0) //райкастим справа (луч влево)
            {
                Vector2 raycastPoint = new Vector2(spriteRect.xMax, Random.Range(spriteRect.yMin, spriteRect.yMax));
                int step = (int)((spriteRect.xMax - spriteRect.xMin) / raycastAccuracy) + 1;
                while (raycastPoint.x > spriteRect.xMin)
                {
                    raycastPoint = new Vector2(raycastPoint.x - step, raycastPoint.y);

                    if (texture.GetPixel((int)raycastPoint.x, (int)raycastPoint.y).a > 0.25f)
                    {
                        Vector2 pivot = sprite.pivot;
                        Vector2 effectPosition = position;
                        effectPosition += (raycastPoint - pivot - new Vector2(spriteRect.xMin, spriteRect.yMin)) / sprite.pixelsPerUnit * modulePrefab.transform.Find("Image").localScale;
                        weldingEffect.SpawnEffects(effectPosition, Quaternion.identity);
                        createdRight = true;
                        effectsSpawnedNum++;
                        break;
                    }
                }
            }

            if (side == 1) //райкастим сверху (луч вниз)
            {
                Vector2 raycastPoint = new Vector2(Random.Range(spriteRect.xMin, spriteRect.xMax), spriteRect.yMax);
                int step = (int)((spriteRect.yMax - spriteRect.yMin) / raycastAccuracy) + 1;
                while (raycastPoint.y > spriteRect.yMin)
                {
                    raycastPoint = new Vector2(raycastPoint.x, raycastPoint.y - step);

                    if (texture.GetPixel((int)raycastPoint.x, (int)raycastPoint.y).a > 0.25f)
                    {
                        Vector2 pivot = sprite.pivot;
                        Vector2 effectPosition = position;
                        effectPosition += (raycastPoint - pivot - new Vector2(spriteRect.xMin, spriteRect.yMin)) / sprite.pixelsPerUnit * modulePrefab.transform.Find("Image").localScale;
                        weldingEffect.SpawnEffects(effectPosition, Quaternion.identity);
                        createdUp = true;
                        effectsSpawnedNum++;
                        break;
                    }
                }
            }

            if (side == 2) //райкастим слева (луч вправо)
            {
                Vector2 raycastPoint = new Vector2(spriteRect.xMin, Random.Range(spriteRect.yMin, spriteRect.yMax));
                int step = (int)((spriteRect.xMax - spriteRect.xMin) / raycastAccuracy) + 1;
                while (raycastPoint.x < spriteRect.xMax)
                {
                    raycastPoint = new Vector2(raycastPoint.x + step, raycastPoint.y);

                    if (texture.GetPixel((int)raycastPoint.x, (int)raycastPoint.y).a > 0.25f)
                    {
                        Vector2 pivot = sprite.pivot;
                        Vector2 effectPosition = position;
                        effectPosition += (raycastPoint - pivot - new Vector2(spriteRect.xMin, spriteRect.yMin)) / sprite.pixelsPerUnit * modulePrefab.transform.Find("Image").localScale;
                        weldingEffect.SpawnEffects(effectPosition, Quaternion.identity);
                        createdLeft = true;
                        effectsSpawnedNum++;
                        break;
                    }
                }
            }

            if (side == 3) //райкастим снизу (луч вверх)
            {
                Vector2 raycastPoint = new Vector2(Random.Range(spriteRect.xMin, spriteRect.xMax), spriteRect.yMin);
                int step = (int)((spriteRect.yMax - spriteRect.yMin) / raycastAccuracy) + 1;
                while (raycastPoint.y < spriteRect.yMax)
                {
                    raycastPoint = new Vector2(raycastPoint.x, raycastPoint.y + step);

                    if (texture.GetPixel((int)raycastPoint.x, (int)raycastPoint.y).a > 0.25f)
                    {
                        Vector2 pivot = sprite.pivot;
                        Vector2 effectPosition = position;
                        effectPosition += (raycastPoint - pivot - new Vector2(spriteRect.xMin, spriteRect.yMin)) / sprite.pixelsPerUnit * modulePrefab.transform.Find("Image").localScale;
                        weldingEffect.SpawnEffects(effectPosition, Quaternion.identity);
                        createdDown = true;
                        effectsSpawnedNum++;
                        break;
                    }
                }
            }

            numCreatedSides = 0;
            if (createdRight)
                numCreatedSides++;
            if (createdUp)
                numCreatedSides++;
            if (createdLeft)
                numCreatedSides++;
            if (createdDown)
                numCreatedSides++;
        }
    }
}
