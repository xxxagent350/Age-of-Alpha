using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class AttackButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Настройка")]
    [Tooltip("Реагировать на пробел")]
    [SerializeField] private bool useSpace = true;
    [Tooltip("Image статуса перезарядки")]
    [SerializeField] private Image reloadStatusImage;

    [Header("Отладка")]
    [Tooltip("Индекс кнопки")]
    public uint ButtonIndex;
    

    public delegate void PointerStateChanged(uint index, bool pressed);
    public event PointerStateChanged pointerStateChangedMessage;

    private void Update()
    {
        if (useSpace)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                pointerStateChangedMessage?.Invoke(ButtonIndex, true);
            }
            if (Input.GetKeyUp(KeyCode.Space))
            {
                pointerStateChangedMessage?.Invoke(ButtonIndex, false);
            }
        }
    }

    public void SetVisualReloadProgress(float reloadProgress)
    {
        reloadStatusImage.fillAmount = reloadProgress;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pointerStateChangedMessage?.Invoke(ButtonIndex, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerStateChangedMessage?.Invoke(ButtonIndex, false);
    }
}
