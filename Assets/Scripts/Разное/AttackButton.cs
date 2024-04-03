using UnityEngine;
using UnityEngine.EventSystems;

public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Tooltip("Индекс кнопки")]
    [SerializeField] uint buttonIndex;

    public delegate void PointerStateChanged(uint index, bool pressed);
    public event PointerStateChanged pointerStateChangedMessage;

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerStateChangedMessage?.Invoke(buttonIndex, true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        pointerStateChangedMessage?.Invoke(buttonIndex, false);
    }
}
