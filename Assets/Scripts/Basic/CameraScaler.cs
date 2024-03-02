using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    [Header("Ќастройки")]
    [SerializeField] UIPressedInfo touchesDetector;
    bool includeAllTouchesWhileScaling;
    public float maxZoom;
    public float minZoom;
    float sensitivity;
    [SerializeField] float mouseScrollSensitivity = 0.1f;
    public Vector2 minPos;
    public Vector2 maxPos;

    [Header("ћожно оставить пустым")]
    [SerializeField] Transform background;

    [HideInInspector] public bool dontMove;
    bool readyToScale;
    float lastDistance;
    int lastTouchCount;
    int touchCount;
    bool readyToTransform;
    Vector2 lastTouchPoint;
    Camera camera_;
    float transformZ;
    bool criticalZoom;

    Vector2 startBackgroundScale;
    float startCameraSize;
    bool backgroundExists;

    private void Awake()
    {
        Application.targetFrameRate = 120;
        camera_ = GetComponent<Camera>();
        transformZ = transform.position.z;
        startCameraSize = camera_.orthographicSize;
        if (background != null)
        {
            backgroundExists = true;
            startBackgroundScale = background.localScale;
        }
    }

    private void Update()
    {
        //вычисление средней позиции нажати€ в пиксел€х на экране
        Vector2 averageTouchPoint = new Vector2();
        int averageTouchPointMass = 0;
        for (int touchPoint = 0; touchPoint < touchesDetector.publicTouchesPositions.Length; touchPoint++)
        {
            averageTouchPoint = (averageTouchPoint * averageTouchPointMass + touchesDetector.publicTouchesPositions[touchPoint]) / (averageTouchPointMass + 1);
            averageTouchPointMass++;
        }
        //вычисление средней позиции нажати€ в юнитах(глобально)
        float pixelsPerUnit = Screen.height / (camera_.orthographicSize * 2);
        Vector3 pointInUnits = averageTouchPoint / pixelsPerUnit;
        pointInUnits -= new Vector3(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
        pointInUnits += new Vector3(transform.position.x, transform.position.y);
        pointInUnits = new Vector3(pointInUnits.x, pointInUnits.y, transformZ);

        //определение на грани ли зум
        float zoom = camera_.orthographicSize;
        if (zoom >= maxZoom || zoom <= minZoom)
        {
            criticalZoom = true;
        }
        else
        {
            criticalZoom = false;
        }
        //

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
                    if (!criticalZoom)
                    {
                        transform.position += (transform.position - pointInUnits) * (1 - toScale);
                        transform.position = new Vector3(transform.position.x, transform.position.y, transformZ);
                    }
                    camera_.orthographicSize /= toScale;
                    lastDistance = averageDistance;
                }
            }

        }
        else
        {
            readyToScale = false;
        }

        //масштабирование колесиком мыши
        camera_.orthographicSize /= 1 + (Input.mouseScrollDelta.y * mouseScrollSensitivity);

        if (!criticalZoom)
        {
            Vector3 pointInUnitsMouse = Input.mousePosition / pixelsPerUnit;
            pointInUnitsMouse -= new Vector3(Screen.width / 2 / pixelsPerUnit, Screen.height / 2 / pixelsPerUnit);
            pointInUnitsMouse += new Vector3(transform.position.x, transform.position.y);
            pointInUnitsMouse = new Vector3(pointInUnitsMouse.x, pointInUnitsMouse.y, transformZ);
            transform.position += (pointInUnitsMouse - transform.position) * Input.mouseScrollDelta.y * mouseScrollSensitivity;
            transform.position = new Vector3(transform.position.x, transform.position.y, transformZ);
        }
        //

        //возвращение зума в заданные границы максимума и минимума при их пересечении
        zoom = camera_.orthographicSize;
        if (zoom > maxZoom)
        {
            camera_.orthographicSize = maxZoom;
        }
        if (zoom < minZoom)
        {
            camera_.orthographicSize = minZoom;
        }
        //

        touchCount = touchesDetector.publicTouchesPositions.Length;

        if (touchCount >= 1 && lastTouchCount == touchCount)
        {
            //движение
            if (readyToTransform == false)
            {
                readyToTransform = true;
                lastTouchPoint = averageTouchPoint;
            }
            else
            {
                sensitivity = (camera_.orthographicSize * 2) / Screen.height;
                Vector2 pos_plus = (averageTouchPoint - lastTouchPoint) * sensitivity;
                if (!dontMove)
                {
                    transform.localPosition -= new Vector3(pos_plus.x, pos_plus.y, 0);
                }

                lastTouchPoint = averageTouchPoint;
            }
        }
        else
        {
            readyToTransform = false;
            lastTouchCount = touchCount;
        }

        if (transform.localPosition.x > maxPos.x)
        {
            transform.localPosition = new Vector3(maxPos.x, transform.localPosition.y, transform.localPosition.z);
        }
        if (transform.localPosition.x < minPos.x)
        {
            transform.localPosition = new Vector3(minPos.x, transform.localPosition.y, transform.localPosition.z);
        }
        if (transform.localPosition.y > maxPos.y)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, maxPos.y, transform.localPosition.z);
        }
        if (transform.localPosition.y < minPos.y)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, minPos.y, transform.localPosition.z);
        }
    }

    private void LateUpdate()
    {
        if (backgroundExists)
        {
            background.localScale = startBackgroundScale * (camera_.orthographicSize / startCameraSize);
        }
    }
}
