using System;
using UnityEngine;

public class SpriteChanger : MonoBehaviour
{
    [Header("Спрайты для разных настроек качества")]
    public Sprites sprite;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = sprite.GetSprite();
    }
}

[Serializable]
public class Sprites
{
    public Sprite highQualitySprite;
    public Sprite mediumQualitySprite;
    public Sprite lowQualitySprite;

    public GraphicsPresets graphicsPreset = GraphicsPresets.none;

    public Sprite GetSprite()
    {
        graphicsPreset = GameSettingsKeeper.instance.userGraphics;

        if (graphicsPreset == GraphicsPresets.low)
        {
            return lowQualitySprite;
        }
        if (graphicsPreset == GraphicsPresets.medium)
        {
            return mediumQualitySprite;
        }
        if (graphicsPreset == GraphicsPresets.high)
        {
            return highQualitySprite;
        }
        return null;
    }
}
