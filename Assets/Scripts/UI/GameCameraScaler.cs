using UnityEngine;

public class GameCameraScaler : MonoBehaviour
{
    //этот класс масштабирует 3д камеру, перемещая её по оси z (работает для касаний экрана и колёсика мыши)

    [Header("Настройка")]
    [SerializeField] UIPressedInfo touchesDetector;
    public float maxZoom;
    public float minZoom;
    [SerializeField] float mouseScrollSensitivity = 0.1f;

    [Tooltip("Коэффициент, показывающий отношение минимального размера камеры к размеру корабля. Т. е. если установить это значение на 3, то минимальный размер камеры будет равен примерно трём диагоналям корабля игрока")]
    [SerializeField] float minZoomRelativeToPlayerShipSize = 1.2f;
    [Tooltip("Коэффициент, показывающий отношение максимального размера камеры для отображения ячеек прочности модулей к размеру корабля. Т. е. если установить это значение на 5, то ячейки прочности модулей начнут отображаться когда размер камеры будет равен примерно пяти диагоналям корабля игрока")]
    [SerializeField] float maxZoomRelativeToPlayerShipSizeToShowHealthCells = 2f;
    [Tooltip("Максимальный зум камеры при котором ячейки прочности модулей могут отображаться. При превышении этого значения отображаться они не будут ни при каких обстоятельствах")]
    public float peakMaxZoomToShowHealthCells = 100;

    [Header("Отладка")]
    public float zoom;
    public bool showHealthCells { get; private set; }

    bool readyToScale;
    float lastDistance;
    public static GameCameraScaler instance;
    private float maxZoomToShowHealthCells;


    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("На сцене несколько GameCameraScaler, чего быть не должно");
        }
        else
        {
            instance = this;
        }
        zoom = -transform.position.z;
    }

    public void SetCameraLimits(float playersShipSize)
    {
        minZoom = playersShipSize * minZoomRelativeToPlayerShipSize;
        maxZoomToShowHealthCells = playersShipSize * maxZoomRelativeToPlayerShipSizeToShowHealthCells;
    }

    private void Update()
    {
        //вычисление средней позиции нажатия в пикселях на экране
        Vector2 averageTouchPoint = new Vector2();
        int averageTouchPointMass = 0;
        for (int touchPoint = 0; touchPoint < touchesDetector.publicTouchesPositions.Length; touchPoint++)
        {
            averageTouchPoint = (averageTouchPoint * averageTouchPointMass + touchesDetector.publicTouchesPositions[touchPoint]) / (averageTouchPointMass + 1);
            averageTouchPointMass++;
        }

        //зум
        if (touchesDetector.publicTouchesPositions.Length >= 2) //если больше или равно 2 нажатий на экран
        {
            float averageDistance = 0;
            int averageDistanceMass = 0;
            foreach (Vector2 point in touchesDetector.publicTouchesPositions)
            {
                averageDistance = (averageDistance * averageDistanceMass + Vector2.Distance(averageTouchPoint, point)) / (averageDistanceMass + 1);
            }

            if (averageDistance > 0)
            {
                if (readyToScale == false)
                {
                    readyToScale = true;
                    lastDistance = averageDistance;
                }
                else
                {
                    float toScale = averageDistance / lastDistance;
                    zoom /= toScale;
                    lastDistance = averageDistance;
                }
            }

        }
        else
        {
            readyToScale = false;
        }
        //

        //масштабирование колесиком мыши
        zoom /= 1 + (Input.mouseScrollDelta.y * mouseScrollSensitivity);
        //

        //возвращение зума в заданные границы максимума и минимума при их пересечении
        if (zoom > maxZoom)
        {
            zoom = maxZoom;
        }
        if (zoom < minZoom)
        {
            zoom = minZoom;
        }
        //

        //изменение положения камеры по оси z
        Vector3 oldPos = transform.position;
        transform.position = new Vector3(oldPos.x, oldPos.y, -zoom);

        //определяем показывать ли ячеёки прочности модулей
        if (zoom < maxZoomToShowHealthCells)
        {
            showHealthCells = true;
        }
        else
        {
            showHealthCells = false;
        }
    }
}
