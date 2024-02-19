using UnityEngine;
using UnityEngine.UI;

public class AutoNativeProportionsUI : MonoBehaviour
{
    Vector2 maxScales;
    Image image;
    RectTransform rectTransform;

    private void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        maxScales.x = rectTransform.sizeDelta.x;
        maxScales.y = rectTransform.sizeDelta.y;
    }

    private void LateUpdate()
    {
        if (maxScales.x / image.sprite.rect.width > maxScales.y / image.sprite.rect.height)
        {
            rectTransform.sizeDelta = new Vector2(maxScales.y * (float)(image.sprite.rect.width / (float)image.sprite.rect.height), maxScales.y);
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(maxScales.x, maxScales.x * (float)(image.sprite.rect.height / (float)image.sprite.rect.width));
        }
    }

}
