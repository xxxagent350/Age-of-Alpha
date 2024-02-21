using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System;

public class AutoNativeProportionsUI : MonoBehaviour
{
    Vector2 maxScales;
    Image image;
    RectTransform rectTransform;
    Sprite lastFrameSprite;

    private void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        maxScales.x = rectTransform.sizeDelta.x;
        maxScales.y = rectTransform.sizeDelta.y;
        if (rectTransform.anchorMin != rectTransform.anchorMax)
        {
            Debug.LogError("AutoNativeProportionsUI не поддерживает stretch (" + gameObject.name + ")");
        }
        SetNativeProportions();
    }

    void LateUpdate()
    {
        SetNativeProportions();
    }

    void SetNativeProportions()
    {
        Sprite sprite = image.sprite;
        if (lastFrameSprite != sprite)
        {
            lastFrameSprite = sprite;
            Rect spriteRect = sprite.rect;
            if (maxScales.x / spriteRect.width > maxScales.y / spriteRect.height)
            {
                rectTransform.sizeDelta = new Vector2(maxScales.y * (float)(spriteRect.width / (float)spriteRect.height), maxScales.y);
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(maxScales.x, maxScales.x * (float)(spriteRect.height / (float)spriteRect.width));
            }
        }
    }
}