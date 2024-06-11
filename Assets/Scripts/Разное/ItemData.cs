using System;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    [Header("Настройка")]
    public Transform image;

    public modulesCategories category;
    public modulesTypes type;

    public TranslatedText Name;
    public TranslatedText description;
    public float Size = 1;
    public float Mass;
    public Vector2 cellsOffset;
    public slotsData[] itemSlotsData;

    private void Start()
    {
        SetSize();
    }
    void SetSize()
    {
        image.localScale = new Vector3(Size, Size, 0);
    }

    //ниже для конфигурации корабля в редакторе
    public void ChangeSize(float size)
    {
        Size = size;
        SetSize();
    }
    public void ChangeCellsShift(Vector2 shift)
    {
        cellsOffset = shift;
    }


    public Vector2 GetMinSlotsPosition()
    {
        Vector2 minPosition = new Vector2();
        for (int cell = 0; cell < itemSlotsData.Length; cell++)
        {
            Vector2 position = itemSlotsData[cell].position + cellsOffset;
            if (position.x < minPosition.x)
            {
                minPosition = new Vector2(position.x, minPosition.y);
            }

            if (position.y < minPosition.y)
            {
                minPosition = new Vector2(minPosition.x, position.y);
            }
        }
        return minPosition;
    }

    public Vector2 GetMaxSlotsPosition()
    {
        Vector2 maxPosition = new Vector2();
        for (int cell = 0; cell < itemSlotsData.Length; cell++)
        {
            Vector2 position = itemSlotsData[cell].position + cellsOffset;
            if (position.x > maxPosition.x)
            {
                maxPosition = new Vector2(position.x, maxPosition.y);
            }

            if (position.y > maxPosition.y)
            {
                maxPosition = new Vector2(maxPosition.x, position.y);
            }
        }
        return maxPosition;
    }
}

public enum modulesCategories
{
    None, //для фильтра модулей

    Weapon,
    DefenceModules,
    EnergyBlocks,
    Engines,
    Drones,
    SpecialModules
}

public enum modulesTypes
{
    None, //для фильтра модулей

    //Weapons
    Ballistics,
    Lasers,
    Rockets,
    Special,
    //Defence
    Armour,
    EnergyShields,
    //EnergyBlocks
    Generators,
    Batteries,
    //Engines
    AccelerationEngines,
    RotationEngines,
    //Drones
    //SpecialModules
    ControlModules
}

[Serializable]
public class slotsData
{
    public Vector2Int position;
    public slotsTypes type;
}

public enum slotsTypes
{
    none,
    standart,
    universal,
    engine
}