using UnityEngine;

public class GameCameraScaler : MonoBehaviour
{
    //этот класс масштабирует 3д камеру, перемещая её по оси z (работает для касаний экрана и колёсика мыши)

    [Header("Настройка")]
    [SerializeField] UIPressedInfo touchesDetector;
    public float maxZoom;
    public float minZoom;
    [SerializeField] float mouseScrollSensitivity = 0.1f;

    [Header("Отладка")]
    public float zoom;

    bool readyToScale;
    float lastDistance;
    public static GameCameraScaler instance;

    private void Awake()
    {
        instance = this;
        zoom = -transform.position.z;
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
    }
}
