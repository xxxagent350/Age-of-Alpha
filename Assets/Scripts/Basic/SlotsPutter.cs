using System;
using UnityEngine;
using UnityEngine.UI;

public class SlotsPutter : MonoBehaviour
{
    [SerializeField] bool editor;
    public ItemData itemData;
    ItemData lastItemData;
    GameObject[] cellsUI;
    [SerializeField] float cellsUISizeMod = -1;
    float cellsUISizeModLastValue;
    [SerializeField] Image mainSlotButton;
    [SerializeField] GameObject mainSlotPrefab;
    [SerializeField] Image universalSlotButton;
    [SerializeField] Text cellsNumInfo;
    [SerializeField] GameObject universalSlotPrefab;
    [SerializeField] Image engineSlotButton;
    [SerializeField] GameObject engineSlotPrefab;
    [SerializeField] AudioSource buttonSoundSource;
    [SerializeField] AudioSource longPressSoundSource;
    int choosenSlotType = -1;

    [SerializeField] AudioClip[] slotPutSounds;
    [SerializeField] AudioSource slotPutSoundSource;
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
    [SerializeField] Text allCellsDeleteText;
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
            itemData = (ItemData)FindFirstObjectByType(typeof(ItemData));
        }
        if (itemData != null)
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
        if (itemData == null)
        {
            return;
        }
        ChangeCellsOpacity();
    }

    public void ChooseMainSlotType()
    {
        if (choosenSlotType != 0)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            choosenSlotType = 0;
            buttonSoundSource.Play();
        }
    }
    public void ChooseUniversalSlotType()
    {
        if (choosenSlotType != 1)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            choosenSlotType = 1;
            buttonSoundSource.Play();
        }
    }
    public void ChooseEngineSlotType()
    {
        if (choosenSlotType != 2)
        {
            mainSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            universalSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 1f);
            engineSlotButton.color = new Color(mainSlotButton.color.r, mainSlotButton.color.g, mainSlotButton.color.b, 0.5f);
            choosenSlotType = 2;
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
            if (itemData == null)
            {
                itemData = (ItemData)FindFirstObjectByType(typeof(ItemData));
            }
            if (itemDataExists)
            {
                if (itemData == null)
                {
                    GameObject.Find("Canvas").GetComponent<ItemDataChanger>().RefreshInfo();
                    itemDataExists = false;
                    RemoveAllCellsUI();
                    cellsNumInfo.text = "Ячеек: ... (... - основные, ... - универсальные, ... - для двигателей)";
                }
            }
            else
            {
                if (itemData != null)
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
            if (itemData != lastItemData && itemData != null)
            {
                lastItemData = itemData;
                ReloadCellsUI();
            }
            if (itemData == null)
            {
                lastItemData = null;
                RemoveAllCellsUI();
            }
        }
    }

    void ChangeCellsOpacity()
    {
        if (itemData == null)
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
        if (itemData == null)
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
        if (itemData == null)
        {
            return;
        }
        GameObject item = itemData.gameObject;

        if (item != null && choosenSlotType != -1)
        {
            float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
            Vector2 pointInUnits = point / pixelsPerUnit;
            pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
            pointInUnits += new Vector2(transform.position.x, transform.position.y);
            Vector2 cellsShift = itemData.cellsOffset;
            pointInUnits -= cellsShift;

            int slotPutSoundNum = UnityEngine.Random.Range(0, slotPutSounds.Length);
            slotPutSoundSource.clip = slotPutSounds[slotPutSoundNum];
            slotPutSoundSource.Play();

            Vector2 roundedPointInUnits = new Vector2(Mathf.RoundToInt(pointInUnits.x), Mathf.RoundToInt(pointInUnits.y));
            AddCellDataInArray(Mathf.RoundToInt(roundedPointInUnits.x), Mathf.RoundToInt(roundedPointInUnits.y), choosenSlotType);
            CountCellsNumber();
            ReloadCellsUI();

        }
    }

    void Painter()
    {
        if (itemData == null)
        {
            return;
        }
        if (paintMode)
        {
            GameObject item = itemData.gameObject;

            if (itemData != null && choosenSlotType != -1)
            {
                float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
                Vector2 pointInUnits = pressPoint / pixelsPerUnit;
                pointInUnits -= new Vector2(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
                pointInUnits += new Vector2(transform.position.x, transform.position.y);
                Vector2 cellsShift = item.GetComponent<ItemData>().cellsOffset;
                pointInUnits -= cellsShift;

                Vector2 roundedPointInUnits = new Vector2(Mathf.RoundToInt(pointInUnits.x), Mathf.RoundToInt(pointInUnits.y));
                int xPos = Mathf.RoundToInt(roundedPointInUnits.x);
                int yPos = Mathf.RoundToInt(roundedPointInUnits.y);

                for (int cell = 0; cell < itemData.cellsDataX.Length; cell++)
                {
                    if (xPos == itemData.cellsDataX[cell] && yPos == itemData.cellsDataY[cell])
                    {
                        if (itemData.cellsDataType[cell] != choosenSlotType)
                        {
                            itemData.cellsDataType[cell] = choosenSlotType;
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
        if (itemData == null)
        {
            return;
        }
        int allCellsNum = itemData.cellsDataX.Length;
        int mainCellsNum = 0;
        int universalNum = 0;
        int engineNum = 0;
        for (int cell = 0; cell < allCellsNum; cell++)
        {
            if (itemData.cellsDataType[cell] == 0)
                mainCellsNum++;
            if (itemData.cellsDataType[cell] == 1)
                universalNum++;
            if (itemData.cellsDataType[cell] == 2)
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
        if (itemData == null)
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
        itemData.cellsDataX = new int[0];
        itemData.cellsDataY = new int[0];
        itemData.cellsDataType = new int[0];
        RemoveAllCellsUI();
        CountCellsNumber();
    }

    public void ReloadCellsUI()
    {
        if (itemData == null)
        {
            return;
        }
        RemoveAllCellsUI();
        RenderAllCellsUI();
    }

    void RenderAllCellsUI()
    {
        if (itemData == null)
        {
            return;
        }
        Vector2 offset = itemData.cellsOffset;

        cellsUI = new GameObject[itemData.cellsDataX.Length];
        for (int cell = 0; cell < itemData.cellsDataX.Length; cell++)
        {
            Vector2 position = new Vector2(itemData.cellsDataX[cell], itemData.cellsDataY[cell]) + offset;
            if (position.x < minCellsPositions.x)
                minCellsPositions = new Vector2(position.x, minCellsPositions.y);
            if (position.y < minCellsPositions.y)
                minCellsPositions = new Vector2(minCellsPositions.x, position.y);
            if (position.x > maxCellsPositions.x)
                maxCellsPositions = new Vector2(position.x, maxCellsPositions.y);
            if (position.y > maxCellsPositions.y)
                maxCellsPositions = new Vector2(maxCellsPositions.x, position.y);

            if (itemData.cellsDataType[cell] == 0)
            {
                cellsUI[cell] = Instantiate(mainSlotPrefab, position, Quaternion.identity);
            }
            if (itemData.cellsDataType[cell] == 1)
            {
                cellsUI[cell] = Instantiate(universalSlotPrefab, position, Quaternion.identity);
            }
            if (itemData.cellsDataType[cell] == 2)
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
                GameObject.Find("Сетка главная").transform.position = new Vector2(0.5f + itemData.cellsOffset.x, 0.5f + itemData.cellsOffset.y);
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

    void AddCellDataInArray(int xPos, int yPos, int type)
    {
        for (int cell = 0; cell < itemData.cellsDataX.Length; cell++)
        {
            if (xPos == itemData.cellsDataX[cell] && yPos == itemData.cellsDataY[cell])
            {
                if (itemData.cellsDataType[cell] != type)
                {
                    itemData.cellsDataType[cell] = type;
                }
                else
                {
                    DeleteCellDataInArray(cell);
                }
                return;
            }
        }

        Array.Resize(ref itemData.cellsDataX, itemData.cellsDataX.Length + 1);
        itemData.cellsDataX[itemData.cellsDataX.Length - 1] = xPos;
        Array.Resize(ref itemData.cellsDataY, itemData.cellsDataY.Length + 1);
        itemData.cellsDataY[itemData.cellsDataY.Length - 1] = yPos;
        Array.Resize(ref itemData.cellsDataType, itemData.cellsDataType.Length + 1);
        itemData.cellsDataType[itemData.cellsDataType.Length - 1] = type;
    }

    void DeleteCellDataInArray(int index)
    {
        if (index == itemData.cellsDataX.Length - 1)
        {
            Array.Resize(ref itemData.cellsDataX, itemData.cellsDataX.Length - 1);
            Array.Resize(ref itemData.cellsDataY, itemData.cellsDataY.Length - 1);
            Array.Resize(ref itemData.cellsDataType, itemData.cellsDataType.Length - 1);
        }
        else
        {
            int xPosRezerve = itemData.cellsDataX[itemData.cellsDataX.Length - 1];
            int yPosRezerve = itemData.cellsDataY[itemData.cellsDataY.Length - 1];
            int typeRezerve = itemData.cellsDataType[itemData.cellsDataType.Length - 1];
            Array.Resize(ref itemData.cellsDataX, itemData.cellsDataX.Length - 1);
            Array.Resize(ref itemData.cellsDataY, itemData.cellsDataY.Length - 1);
            Array.Resize(ref itemData.cellsDataType, itemData.cellsDataType.Length - 1);
            itemData.cellsDataX[index] = xPosRezerve;
            itemData.cellsDataY[index] = yPosRezerve;
            itemData.cellsDataType[index] = typeRezerve;
        }
    }

}
