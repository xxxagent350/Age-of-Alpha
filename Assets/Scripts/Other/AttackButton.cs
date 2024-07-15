using UnityEngine;
using UnityEngine.EventSystems;

public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Индекс кнопки")]
    [SerializeField] uint buttonIndex;
    [Tooltip("Реагировать на пробел")]
    [SerializeField] bool useSpace = true;

    public delegate void PointerStateChanged(uint index, bool pressed);
    public event PointerStateChanged pointerStateChangedMessage;

    void Update()
    {
        if (useSpace)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                pointerStateChangedMessage?.Invoke(buttonIndex, true);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                pointerStateChangedMessage?.Invoke(buttonIndex, false);
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerStateChangedMessage?.Invoke(buttonIndex, true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        pointerStateChangedMessage?.Invoke(buttonIndex, false);
    }
}
