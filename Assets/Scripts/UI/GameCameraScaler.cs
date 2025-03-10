using UnityEngine;

public class GameCameraScaler : MonoBehaviour
{
    //���� ����� ������������ 3� ������, ��������� � �� ��� z (�������� ��� ������� ������ � ������� ����)

    [Header("���������")]
    [SerializeField] UIPressedInfo touchesDetector;
    public float maxZoom;
    public float minZoom;
    [SerializeField] float mouseScrollSensitivity = 0.1f;

    [Tooltip("�����������, ������������ ��������� ������������ ������� ������ � ������� �������. �. �. ���� ���������� ��� �������� �� 3, �� ����������� ������ ������ ����� ����� �������� ��� ���������� ������� ������")]
    [SerializeField] float minZoomRelativeToPlayerShipSize = 1.2f;
    [Tooltip("�����������, ������������ ��������� ������������� ������� ������ ��� ����������� ����� ��������� ������� � ������� �������. �. �. ���� ���������� ��� �������� �� 5, �� ������ ��������� ������� ������ ������������ ����� ������ ������ ����� ����� �������� ���� ���������� ������� ������")]
    [SerializeField] float maxZoomRelativeToPlayerShipSizeToShowHealthCells = 2f;
    [Tooltip("������������ ��� ������ ��� ������� ������ ��������� ������� ����� ������������. ��� ���������� ����� �������� ������������ ��� �� ����� �� ��� ����� ���������������")]
    public float peakMaxZoomToShowHealthCells = 100;

    [Header("�������")]
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
            Debug.LogWarning("�� ����� ��������� GameCameraScaler, ���� ���� �� ������");
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
        //���������� ������� ������� ������� � �������� �� ������
        Vector2 averageTouchPoint = new Vector2();
        int averageTouchPointMass = 0;
        for (int touchPoint = 0; touchPoint < touchesDetector.publicTouchesPositions.Length; touchPoint++)
        {
            averageTouchPoint = (averageTouchPoint * averageTouchPointMass + touchesDetector.publicTouchesPositions[touchPoint]) / (averageTouchPointMass + 1);
            averageTouchPointMass++;
        }

        //���
        if (touchesDetector.publicTouchesPositions.Length >= 2) //���� ������ ��� ����� 2 ������� �� �����
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

        //��������������� ��������� ����
        zoom /= 1 + (Input.mouseScrollDelta.y * mouseScrollSensitivity);
        //

        //����������� ���� � �������� ������� ��������� � �������� ��� �� �����������
        if (zoom > maxZoom)
        {
            zoom = maxZoom;
        }
        if (zoom < minZoom)
        {
            zoom = minZoom;
        }
        //

        //��������� ��������� ������ �� ��� z
        Vector3 oldPos = transform.position;
        transform.position = new Vector3(oldPos.x, oldPos.y, -zoom);

        //���������� ���������� �� ����� ��������� �������
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
