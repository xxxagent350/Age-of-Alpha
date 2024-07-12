using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPressedInfo : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Отладка")]
    public Vector2[] publicTouchesPositions;

    int[] touchIDs;
    bool[] touched;
    Vector2[] touchesPositions;

    private void Awake()
    {
        touchesPositions = new Vector2[0];
        touchIDs = new int[0];
        touched = new bool[0];
        publicTouchesPositions = new Vector2[0];
    }

    private void Update()
    {
        publicTouchesPositions = new Vector2[0];

        for (int num = 0; num < touchesPositions.Length; num++)
        {
            if (touched[num])
            {
                Array.Resize(ref publicTouchesPositions, publicTouchesPositions.Length + 1);
                publicTouchesPositions[publicTouchesPositions.Length - 1] = touchesPositions[num];
            }
        }
    }

    void AddTouchInArray(PointerEventData eventData)
    {
        Array.Resize(ref touchIDs, touchIDs.Length + 1);
        Array.Resize(ref touched, touched.Length + 1);
        Array.Resize(ref touchesPositions, touchesPositions.Length + 1);
        touchIDs[touchIDs.Length - 1] = eventData.pointerId;
        touched[touched.Length - 1] = true;
        touchesPositions[touchesPositions.Length - 1] = eventData.position;
    }


    public delegate void PointerStateChanged(bool pressed);
    public event PointerStateChanged pointerStateChangedMessage;
    public void OnPointerDown(PointerEventData eventData)
    {
        pointerStateChangedMessage?.Invoke(true);

        bool founded = false;
        for (int idNum = 0; idNum < touchIDs.Length; idNum++)
        {
            if (touchIDs[idNum] == eventData.pointerId)
            {
                touchesPositions[idNum] = eventData.position;
                founded = true;
                touched[idNum] = true;
                break;
            }
        }
        if (!founded)
        {
            AddTouchInArray(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        for (int idNum = 0; idNum < touchIDs.Length; idNum++)
        {
            if (touchIDs[idNum] == eventData.pointerId)
            {
                touchesPositions[idNum] = eventData.position;
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerStateChangedMessage?.Invoke(false);

        for (int idNum = 0; idNum < touchIDs.Length; idNum++)
        {
            if (touchIDs[idNum] == eventData.pointerId)
            {
                touchesPositions[idNum] = new Vector2();
                touched[idNum] = false;
                break;
            }
        }
    }
}
