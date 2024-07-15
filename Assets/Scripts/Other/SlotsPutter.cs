using System;
using UnityEngine;
using UnityEngine.UI;

public class SlotsPutter : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] bool editor;
    [SerializeField] Text allCellsDeleteText;
    [SerializeField] float cellsUISizeMod = -1;
    [SerializeField] Image mainSlotButton;
    [SerializeField] GameObject mainSlotPrefab;
    [SerializeField] Image universalSlotButton;
    [SerializeField] Text cellsNumInfo;
    [SerializeField] GameObject universalSlotPrefab;
    [SerializeField] Image engineSlotButton;
    [SerializeField] GameObject engineSlotPrefab;
    [SerializeField] AudioSource buttonSoundSource;
    [SerializeField] AudioSource longPressSoundSource;
    [SerializeField] AudioClip[] slotPutSounds;
    [SerializeField] AudioSource slotPutSoundSource;

    [Header("Отладка")]
    public ItemData ItemData;
    [SerializeField] ItemData lastItemData;

    slotsTypes choosenSlotType = slotsTypes.none;
    float cellsUISizeModLastValue;
    GameObject[] cellsUI;
    Camera camera_;
    bool mousePressed;
    float timerPressed;
    const float maxTimerToIncludePress = 0.2f;
    const float timeToEnablePaintMode = 0.6f;
    Vector2 startPressPoint;
    Vector2 pressPoint;
    const float maxDistanceToIncludePress = 10;

    bool paintMode;
    bool dontEnablePaintMode;
    float allCellsDeleteAskTimer = 3;
    float cellsOpacity = 1;
    int lastCellsUICount;
    bool itemDataExists;

    public Vector2 minCellsPositions;
    public Vector2 maxCellsPositions;

    private void Start()
    {
        camera_ = GetComponent<Camera>();
        cellsUI = new GameObject[0];
        cellsUISizeModLastValue = cellsUISizeMod;
        if (editor)
        {
            ItemData = (ItemData)FindFirstObjectByType(typeof(ItemData));
        }
        if (ItemData != null)
        {
            ReloadCellsUI();
            if (editor)
            {
                CountCellsNumber();
                ChangeCellsOpacity();
                itemDataExists = true;
            }
        }
    }

    public void ChangeCellsOpacityValue(float newValue)
    {
        cellsOpacity = newValue;
        if (ItemData == null)
        {
            return;
        }
        ChangeCellsOpacity();
    }

    public void ChooseMainSlotType()
    {
        if (choosenSlotType != slotsTypes.standart)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            choosenSlotType = slotsTypes.standart;
            buttonSoundSource.Play();
        }
    }
    public void ChooseUniversalSlotType()
    {
        if (choosenSlotType != slotsTypes.universal)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            choosenSlotType = slotsTypes.universal;
            buttonSoundSource.Play();
        }
    }
    public void ChooseEngineSlotType()
    {
        if (choosenSlotType != slotsTypes.engine)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            choosenSlotType = slotsTypes.engine;
            buttonSoundSource.Play();
        }
    }

    private void Update()
    {
        if (cellsUISizeMod != cellsUISizeModLastValue)
        {
            cellsUISizeModLastValue = cellsUISizeMod;
            ReloadCellsUI();
        }
        if (editor)
        {
            if (ItemData == null)
            {
                ItemData = (ItemData)FindFirstObjectByType(typeof(ItemData));
            }
            if (itemDataExists)
            {
                if (ItemData == null)
                {
                    GameObject.Find("Canvas").GetComponent<ItemDataChanger>().RefreshInfo();
                    itemDataExists = false;
                    RemoveAllCellsUI();
                    cellsNumInfo.text = "Ячеек: ... (... - основные, ... - универсальные, ... - для двигателей)";
                }
            }
            else
            {
                if (ItemData != null)
                {
                    itemDataExists = true;
                    RenderAllCellsUI();
                    CountCellsNumber();
                    GameObject.Find("Canvas").GetComponent<ItemDataChanger>().RefreshInfo();
                }
            }

            allCellsDeleteAskTimer += Time.deltaTime;
            if (allCellsDeleteAskTimer > 3)
            {
                allCellsDeleteText.text = "Удалить все ячейки";
            }
            else
            {
                allCellsDeleteText.text = "Точно?";
            }
            GetInputs();
            Painter();
            if (lastCellsUICount != cellsUI.Length)
            {
                lastCellsUICount = cellsUI.Length;
                ChangeCellsOpacity();
            }
        }
        else
        {
            if (ItemData != lastItemData && ItemData != null)
            {
                lastItemData = ItemData;
                ReloadCellsUI();
            }
            if (ItemData == null)
            {
                lastItemData = null;
                RemoveAllCellsUI();
            }
        }
    }

    void ChangeCellsOpacity()
    {
        if (ItemData == null)
        {
            return;
        }
        foreach (GameObject cellUI in cellsUI)
        {
            SpriteRenderer spriteRenderer = cellUI.GetComponent<SpriteRenderer>();
            Color lastColor = spriteRenderer.color;
            spriteRenderer.color = new Color(lastColor.r, lastColor.g, lastColor.b, cellsOpacity);
        }
    }

    void GetInputs()
    {
        GetComponent<CameraScaler>().dontMove = paintMode;
        if (Input.GetMouseButtonDown(0) == true)
        {
            timerPressed = 0;
            mousePressed = true;
            startPressPoint = Input.mousePosition;
            paintMode = false;
            dontEnablePaintMode = false;
        }
        if (Input.GetMouseButtonUp(0) == true)
        {
            timerPressed = 0;
            mousePressed = false;
            paintMode = false;
            dontEnablePaintMode = false;
            //CheckClick();
        }
        if (mousePressed)
        {
            timerPressed += Time.deltaTime;
            pressPoint = Input.mousePosition;
        }

        if (!dontEnablePaintMode && timerPressed > timeToEnablePaintMode)
        {
            dontEnablePaintMode = true;
            if (Vector2.Distance(startPressPoint, pressPoint) < maxDistanceToIncludePress)
            {
                paintMode = true;
                longPressSoundSource.Play();
            }
        }

    }

    public void CheckClick()
    {
        if (ItemData == null)
        {
            return;
        }
        if (timerPressed < maxTimerToIncludePress && Vector2.Distance(startPressPoint, pressPoint) < maxDistanceToIncludePress)
        {
            Click(pressPoint);
        }
    }

    void Click(Vector2 point)
    {
        if (ItemData == null)
        {
            return;
        }
        GameObject item = ItemData.gameObject;

        if (item != null && choosenSlotType != slotsTypes.none)
        {
            float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
            Vector2 pointInUnits = point / pixelsPerUnit;
            pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
            pointInUnits += new Vector2(transform.position.x, transform.position.y);
            Vector2 cellsShift = ItemData.CellsOffset;
            pointInUnits -= cellsShift;

            int slotPutSoundNum = UnityEngine.Random.Range(0, slotPutSounds.Length);
            slotPutSoundSource.clip = slotPutSounds[slotPutSoundNum];
            slotPutSoundSource.Play();

            Vector2 roundedPointInUnits = new Vector2(Mathf.RoundToInt(pointInUnits.x), Mathf.RoundToInt(pointInUnits.y));

            CellData newSlotData = new CellData();
            newSlotData.position = new Vector2Int(Mathf.RoundToInt(roundedPointInUnits.x), Mathf.RoundToInt(roundedPointInUnits.y));
            newSlotData.type = choosenSlotType;

            AddCellDataInArray(newSlotData);
            CountCellsNumber();
            ReloadCellsUI();

        }
    }

    void Painter()
    {
        if (ItemData == null)
        {
            return;
        }
        if (paintMode)
        {
            GameObject item = ItemData.gameObject;

            if (ItemData != null && choosenSlotType != slotsTypes.none)
            {
                float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
                Vector2 pointInUnits = pressPoint / pixelsPerUnit;
                pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
                pointInUnits += new Vector2(transform.position.x, transform.position.y);
                Vector2 cellsShift = item.GetComponent<ItemData>().CellsOffset;
                pointInUnits -= cellsShift;

                Vector2 roundedPointInUnits = new Vector2(Mathf.RoundToInt(pointInUnits.x), Mathf.RoundToInt(pointInUnits.y));
                int xPos = Mathf.RoundToInt(roundedPointInUnits.x);
                int yPos = Mathf.RoundToInt(roundedPointInUnits.y);

                for (int cell = 0; cell < ItemData.ItemCellsData.Length; cell++)
                {
                    if (xPos == ItemData.ItemCellsData[cell].position.x && yPos == ItemData.ItemCellsData[cell].position.y)
                    {
                        if (ItemData.ItemCellsData[cell].type != choosenSlotType)
                        {
                            ItemData.ItemCellsData[cell].type = choosenSlotType;
                            ReloadCellsUI();
                        }
                        return;
                    }
                }
                Click(pressPoint);
            }
        }
    }

    void CountCellsNumber()
    {
        if (ItemData == null)
        {
            return;
        }
        int allCellsNum = ItemData.ItemCellsData.Length;
        int mainCellsNum = 0;
        int universalNum = 0;
        int engineNum = 0;
        for (int cell = 0; cell < allCellsNum; cell++)
        {
            if (ItemData.ItemCellsData[cell].type == slotsTypes.standart)
                mainCellsNum++;
            if (ItemData.ItemCellsData[cell].type == slotsTypes.universal)
                universalNum++;
            if (ItemData.ItemCellsData[cell].type == slotsTypes.engine)
                engineNum++;
        }
        if (cellsNumInfo != null)
        {
            cellsNumInfo.text = "Ячеек: " + allCellsNum + " (" + mainCellsNum + " - основные, " + universalNum + " - универсальные, " + engineNum + " - для двигателей)";
        }
    }

    public void DeleteAllCells()
    {
        minCellsPositions = new Vector2();
        maxCellsPositions = new Vector2();
        if (ItemData == null)
        {
            return;
        }
        buttonSoundSource.Play();
        if (allCellsDeleteAskTimer > 3)
        {
            allCellsDeleteAskTimer = 0;
            return;
        }
        else
        {
            allCellsDeleteAskTimer = 3;
        }
        ItemData.ItemCellsData = new CellData[0];
        RemoveAllCellsUI();
        CountCellsNumber();
    }

    public void ReloadCellsUI()
    {
        if (ItemData == null)
        {
            return;
        }
        RemoveAllCellsUI();
        RenderAllCellsUI();
    }

    void RenderAllCellsUI()
    {
        if (ItemData == null)
        {
            return;
        }
        Vector2 offset = ItemData.CellsOffset;

        cellsUI = new GameObject[ItemData.ItemCellsData.Length];
        for (int cell = 0; cell < ItemData.ItemCellsData.Length; cell++)
        {
            Vector2 position = ItemData.ItemCellsData[cell].position + offset;
            if (position.x < minCellsPositions.x)
                minCellsPositions = new Vector2(position.x, minCellsPositions.y);
            if (position.y < minCellsPositions.y)
                minCellsPositions = new Vector2(minCellsPositions.x, position.y);
            if (position.x > maxCellsPositions.x)
                maxCellsPositions = new Vector2(position.x, maxCellsPositions.y);
            if (position.y > maxCellsPositions.y)
                maxCellsPositions = new Vector2(maxCellsPositions.x, position.y);

            if (ItemData.ItemCellsData[cell].type == slotsTypes.standart)
            {
                cellsUI[cell] = Instantiate(mainSlotPrefab, position, Quaternion.identity);
            }
            if (ItemData.ItemCellsData[cell].type == slotsTypes.universal)
            {
                cellsUI[cell] = Instantiate(universalSlotPrefab, position, Quaternion.identity);
            }
            if (ItemData.ItemCellsData[cell].type == slotsTypes.engine)
            {
                cellsUI[cell] = Instantiate(engineSlotPrefab, position, Quaternion.identity);
            }
            if (cellsUISizeMod > 0)
            {
                cellsUI[cell].transform.localScale *= cellsUISizeMod;
            }
        }
        if (!editor)
        {
            if (GameObject.Find("Сетка главная") != null)
            {
                GameObject.Find("Сетка главная").transform.position = new Vector2(0.5f + ItemData.CellsOffset.x, 0.5f + ItemData.CellsOffset.y);
            }
            CameraScaler cameraScaler = GetComponent<CameraScaler>();
            cameraScaler.minPos = minCellsPositions - new Vector2(1, 1);
            cameraScaler.maxPos = maxCellsPositions + new Vector2(1, 1);
            cameraScaler.maxZoom = maxCellsPositions.y - minCellsPositions.y;
            if (cameraScaler.maxZoom < cameraScaler.minZoom)
            {
                cameraScaler.maxZoom = cameraScaler.minZoom;
            }
        }
        ChangeCellsOpacity();
    }

    void RemoveAllCellsUI()
    {
        minCellsPositions = new Vector2();
        maxCellsPositions = new Vector2();

        foreach (GameObject cell in cellsUI)
        {
            Destroy(cell);
        }
        cellsUI = new GameObject[0];
    }

    void AddCellDataInArray(CellData slotData)
    {
        for (int cell = 0; cell < ItemData.ItemCellsData.Length; cell++)
        {
            if (slotData.position.x == ItemData.ItemCellsData[cell].position.x && slotData.position.y == ItemData.ItemCellsData[cell].position.y)
            {
                if (ItemData.ItemCellsData[cell].type != slotData.type)
                {
                    ItemData.ItemCellsData[cell].type = slotData.type;
                }
                else
                {
                    DeleteCellDataInArray(cell);
                }
                return;
            }
        }

        Array.Resize(ref ItemData.ItemCellsData, ItemData.ItemCellsData.Length + 1);
        ItemData.ItemCellsData[ItemData.ItemCellsData.Length - 1] = slotData;
    }

    void DeleteCellDataInArray(int index)
    {
        if (index == ItemData.ItemCellsData.Length - 1)
        {
            Array.Resize(ref ItemData.ItemCellsData, ItemData.ItemCellsData.Length - 1);
        }
        else
        {
            CellData slotDataRezerve = ItemData.ItemCellsData[ItemData.ItemCellsData.Length - 1];
            Array.Resize(ref ItemData.ItemCellsData, ItemData.ItemCellsData.Length - 1);
            ItemData.ItemCellsData[index] = slotDataRezerve;
        }
    }

}
