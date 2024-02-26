using UnityEngine;

public class UImodulesOperator : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] float maxTimerToDragModule; //если столько времени держать палец на модуле, его можно будет передвинуть
    [SerializeField] float distanceFromPressPointToRevertDragging; //измеряется в частях высоты экрана
    [SerializeField] RectTransform menuStartLine; //синяя полоска, после которой начинается меню модулей

    [Header("Отладка")]
    [SerializeField] Vector2 shipMousePoint; //позиция нажатия относительно корабля в юнитах

    UIPressedInfo uiPressedInfo;
    Camera camera_;
    ItemData shipData;
    SlotsPutter slotsPutter;
    float timerToDragModule;
    bool dragging;
    Vector2 clickPoint; //позиция начала нажатия
    bool nowPressed; //нажато ли сейчас на экран постройки

    private void Start()
    {
        uiPressedInfo = GetComponent<UIPressedInfo>();
        camera_ = (Camera)FindFirstObjectByType(typeof(Camera));
        slotsPutter = (SlotsPutter)FindFirstObjectByType(typeof(SlotsPutter));
        TryFoundShipData();
    }

    private void Update()
    {
        if (uiPressedInfo.publicTouchesPositions.Length == 1)
        {
            if (!dragging)
            {
                if (!nowPressed)
                {
                    nowPressed = true;
                    clickPoint = uiPressedInfo.publicTouchesPositions[0];
                }
                shipMousePoint = GetRoundedMousePointInUnits(uiPressedInfo.publicTouchesPositions[0]);

            }
        }
        else
        {
            if (dragging)
            {
                dragging = false;
                nowPressed = false;
            }
        }
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
    }
}
