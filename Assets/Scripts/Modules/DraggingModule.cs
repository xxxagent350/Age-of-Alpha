using UnityEngine;

public class DraggingModule : MonoBehaviour
{
    [Header("Ќастройка")]
    [SerializeField] private GameObject _greenSlot;
    [SerializeField] private GameObject _redSlot;
    [SerializeField] private AudioClip _errorSound;
    [SerializeField] private float _errorSoundVolume = 1;
    [SerializeField] private AudioClip[] _modulePutSounds;
    [Tooltip("Ёффект сварки")]
    [SerializeField] private Effect _weldingEffect; //эффекты сварки

    [Header("ќтладка")]
    public Module MyModule;
    [Tooltip("—прайт низкого разрешени€. Ќужен дл€ определени€ позиции эффектов сварки при установке модул€")]
    public Sprite LowResolutionSprite;

    private GameObject _modulePrefab;
    private bool _playingOnPhone;
    private Camera _camera;
    private SpriteRenderer _spriteRenderer;
    private ItemData _moduleData;
    private ItemData _shipData;
    private ShipStats _shipInstalledModulesData;
    private SlotsPutter _slotsPutter;
    private Vector2 _lastFrameRoundedMousePointInUnits;
    private bool _canBeInstalled;
    private ModulesMenu _modulesMenu;
    private Vector2 _lastMousePos;
    private Vector2 _lastMousePosInUnits;
    [SerializeField] private RectTransform _menuStartLine; //син€€ полоска, после которой начинаетс€ меню модулей
    private bool _destroyed;

    private void Start()
    {
        _menuStartLine = GameObject.Find("Canvas").transform.Find("ModulesMenu").Find("Border").Find("BorderMiddleLeft").GetComponent<RectTransform>();
        cellsUI = new GameObject[0];
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (Input.touchCount > 0)
        {
            _playingOnPhone = true;
        }
        _slotsPutter = (SlotsPutter)FindFirstObjectByType(typeof(SlotsPutter));
        TryFoundShipData();
        _camera = (Camera)FindFirstObjectByType(typeof(Camera));
        _modulesMenu = (ModulesMenu)FindFirstObjectByType(typeof(ModulesMenu));
        _modulePrefab = DataOperator.instance.modulesPrefabs[MyModule.moduleNum];
        _moduleData = _modulePrefab.GetComponent<ItemData>();
        RenderAllCellsUIAndCheckIfModuleFits();
        Update();
    }

    void TryFoundShipData()
    {
        if (_shipData == null)
        {
            _shipData = _slotsPutter.ItemData;
        }
        if (_shipInstalledModulesData == null)
        {
            _shipInstalledModulesData = _slotsPutter.ItemData.GetComponent<ShipStats>();
        }
    }

    private void Update()
    {
        if (_playingOnPhone)
        {
            if (Input.touchCount != 1)
            {
                TryPutModule();
            }
            else
            {
                _lastMousePos = Input.GetTouch(0).position;
                _lastMousePosInUnits = GetWorldMousePosInUnits(Input.GetTouch(0).position, false);
                _lastFrameRoundedMousePointInUnits = GetRoundedMousePointInUnits(Input.GetTouch(0).position);
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
                _lastMousePos = Input.mousePosition;
                _lastMousePosInUnits = GetWorldMousePosInUnits(Input.mousePosition, false);
                _lastFrameRoundedMousePointInUnits = GetRoundedMousePointInUnits(Input.mousePosition);
            }
        }
        transform.position = _lastMousePosInUnits;
        if (_lastFrameRoundedMousePointInUnits != new Vector2(transform.position.x, transform.position.y))
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
        if (_lastMousePos.x < _menuStartLine.position.x)
        {
            if (_canBeInstalled)
            {   
                //задаЄм позицию модул€, подход€щую под €чейки
                transform.position = _lastFrameRoundedMousePointInUnits;
                //устанавливаем модуль на корабль
                TryFoundShipData();
                _shipInstalledModulesData.AddModule(MyModule, transform.position, Times.Present);
                //забираем одну штуку со склада
                ModulesOnStorageData modulesOnStorageData = DataOperator.instance.LoadDataModulesOnStorage(MyModule);
                modulesOnStorageData.amount -= 1;
                DataOperator.instance.SaveData(modulesOnStorageData);
                //обновл€ем список предметов в меню
                _modulesMenu.RenderMenuSlosts();
                //играем случайный звук установки модул€
                DataOperator.instance.PlayRandom3DSound(transform.position, _modulePutSounds);
                //создаЄм эффект сварки
                CreateWeldingEffect(transform.position, LowResolutionSprite);
            }
            else
            {
                DataOperator.instance.PlayUISound(_errorSound, _errorSoundVolume);
            }
        }
        _destroyed = true;
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
        Vector2 cellsShift = _moduleData.CellsOffset;
        roundedPointInUnits += cellsShift;
        roundedPointInUnits += _shipData.CellsOffset;
        return roundedPointInUnits;
    }

    Vector2 GetWorldMousePosInUnits(Vector2 mousePos, bool withModuleAndShipOffset)
    {
        float pixelsPerUnit = Screen.height / (_camera.orthographicSize * 2);
        Vector2 pointInUnits = mousePos / pixelsPerUnit;
        pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
        if (withModuleAndShipOffset)
        {
            Vector2 cellsShift = _moduleData.CellsOffset;
            pointInUnits -= cellsShift;
            pointInUnits -= _shipData.CellsOffset;
        }
        pointInUnits += new Vector2(_camera.transform.position.x, _camera.transform.position.y);
        return pointInUnits;
    }

    //ниже - дл€ клеток под перетаскиваемым модулем
    GameObject[] cellsUI;

    void RenderAllCellsUIAndCheckIfModuleFits()
    {
        if (!_destroyed)
        {
            transform.position = _lastFrameRoundedMousePointInUnits;//
            foreach (GameObject cell in cellsUI)
            {
                Destroy(cell);
            }
            if (_lastMousePos.x < _menuStartLine.position.x)
            {
                _canBeInstalled = true;
                Vector2 offset = _moduleData.CellsOffset;
                cellsUI = new GameObject[_moduleData.ItemCellsData.Length];
                for (int cell = 0; cell < _moduleData.ItemCellsData.Length; cell++)
                {
                    Vector2 globalPosition = new Vector2(_moduleData.ItemCellsData[cell].position.x + transform.position.x, _moduleData.ItemCellsData[cell].position.y + transform.position.y) + offset - _shipData.CellsOffset;
                    Vector2 localPosition = new Vector2(_moduleData.ItemCellsData[cell].position.x, _moduleData.ItemCellsData[cell].position.y) + offset;
                    GameObject slot;
                    if (CheckIfSlotCanBeInstalled(globalPosition, _moduleData.ItemCellsData[cell].type))
                    {
                        slot = Instantiate(_greenSlot, new Vector3(), Quaternion.identity);
                    }
                    else
                    {
                        slot = Instantiate(_redSlot, new Vector3(), Quaternion.identity);
                        _canBeInstalled = false;
                    }
                    cellsUI[cell] = slot;
                    //slot.transform.parent = transform;
                    //slot.transform.localPosition = new Vector3(localPosition.x * (1 / transform.localScale.x), localPosition.y * (1 / transform.localScale.y), 0);
                    slot.transform.position = globalPosition + _shipData.CellsOffset;
                }
            }
            else
            {
                _canBeInstalled = false;
            }
            transform.position = _lastMousePosInUnits;
            if (!_canBeInstalled)
            {
                _spriteRenderer.color = new Color(1, 0.35f, 0.35f);
            }
            else
            {
                _spriteRenderer.color = new Color(0.35f, 1, 0.35f);
            }
        }
    }

    bool CheckIfSlotCanBeInstalled(Vector2 position, slotsTypes slotType)
    {
        TryFoundShipData();
        if (_shipData != null)
        {
            //сначала провер€ем не зан€т ли слот корабл€ другим модулем
            foreach (ModuleOnShipData moduleOnShip in _shipInstalledModulesData.modulesOnShip)
            {   
                //перебираем все уже установленные модули на корабле
                GameObject modulePrefab_ = DataOperator.instance.modulesPrefabs[moduleOnShip.module.moduleNum];
                ItemData moduleData_ = modulePrefab_.GetComponent<ItemData>();
                for (int moduleSlot_ = 0; moduleSlot_ < moduleData_.ItemCellsData.Length; moduleSlot_++)
                {
                    Vector2 moduleSlotPos = new Vector2(moduleData_.ItemCellsData[moduleSlot_].position.x + moduleOnShip.position.x, moduleData_.ItemCellsData[moduleSlot_].position.y + moduleOnShip.position.y) + moduleData_.CellsOffset - _shipData.CellsOffset;
                    if (Vector2.Distance(position, moduleSlotPos) < 0.01f)
                    {
                        return false;
                    }
                }
            }

            //теперь провер€ем совместим ли слот
            for (int slot = 0; slot < _shipData.ItemCellsData.Length; slot++)
            {
                if (Mathf.Abs(_shipData.ItemCellsData[slot].position.x - position.x) < 0.01f && Mathf.Abs(_shipData.ItemCellsData[slot].position.y - position.y) < 0.01f)
                {
                    if (slotType == slotsTypes.standart) //обычна€ €чейка модул€ который перетаскиваетс€
                    {
                        if (_shipData.ItemCellsData[slot].type == slotsTypes.standart || _shipData.ItemCellsData[slot].type == slotsTypes.universal)
                            return true;
                    }
                    if (slotType == slotsTypes.universal) //универсальна€ €чейка
                    {
                        return true;
                    }
                    if (slotType == slotsTypes.engine) //€чейка дл€ двигателей
                    {
                        if (_shipData.ItemCellsData[slot].type == slotsTypes.engine || _shipData.ItemCellsData[slot].type == slotsTypes.universal)
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

        int slotsNum = _moduleData.ItemCellsData.Length;
        int minEffectsNum = Mathf.RoundToInt(slotsNum - Mathf.Pow(Mathf.Sqrt(slotsNum) - 2, 2) + 1) * 4;
        int effectsSpawnedNum = 0;

        while ((numCreatedSides < 3 || effectsSpawnedNum < minEffectsNum) && whileIterations < maxWhileIterations)
        {
            whileIterations++;
            int side = Random.Range(0, 4);
            
            if (side == 0) //райкастим справа (луч влево)
            {
                Vector2 raycastPoint = new Vector2(spriteRect.xMax, Random.Range(spriteRect.yMin, spriteRect.yMax));

                int step = Mathf.RoundToInt((spriteRect.xMax - spriteRect.xMin) / raycastAccuracy);
                if (step <= 0)
                {
                    step = 1;
                }

                while (raycastPoint.x > spriteRect.xMin)
                {
                    raycastPoint = new Vector2(raycastPoint.x - step, raycastPoint.y);

                    if (texture.GetPixel((int)raycastPoint.x, (int)raycastPoint.y).a > 0.25f)
                    {
                        Vector2 pivot = sprite.pivot;
                        Vector2 effectPosition = position;
                        effectPosition += (raycastPoint - pivot - new Vector2(spriteRect.xMin, spriteRect.yMin)) / sprite.pixelsPerUnit * _modulePrefab.transform.Find("Image").localScale;
                        _weldingEffect.SpawnEffects(effectPosition, Quaternion.identity);
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
                        effectPosition += (raycastPoint - pivot - new Vector2(spriteRect.xMin, spriteRect.yMin)) / sprite.pixelsPerUnit * _modulePrefab.transform.Find("Image").localScale;
                        _weldingEffect.SpawnEffects(effectPosition, Quaternion.identity);
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
                        effectPosition += (raycastPoint - pivot - new Vector2(spriteRect.xMin, spriteRect.yMin)) / sprite.pixelsPerUnit * _modulePrefab.transform.Find("Image").localScale;
                        _weldingEffect.SpawnEffects(effectPosition, Quaternion.identity);
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
                        effectPosition += (raycastPoint - pivot - new Vector2(spriteRect.xMin, spriteRect.yMin)) / sprite.pixelsPerUnit * _modulePrefab.transform.Find("Image").localScale;
                        _weldingEffect.SpawnEffects(effectPosition, Quaternion.identity);
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
