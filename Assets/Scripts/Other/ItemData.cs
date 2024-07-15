using System;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    [Header("���������")]
    public Transform Image;
    public modulesCategories Category;
    public modulesTypes Type;
    [Tooltip("������ ������� ����������. ����������� ������ ��� �������. ����� ��� ����������� ������� �������� ������ ��� ��������� ������")]
    public Sprite LowResolutionSprite;

    public TranslatedText Name;
    public TranslatedText Description;
    public float Size = 1;
    public float Mass;
    public Vector2 CellsOffset;
    public CellData[] ItemCellsData;

    public bool IsModule;

    private void Start()
    {
        SetSize();

        if (IsModule)
        {
            Durability myDurability = GetComponent<Durability>();
            if (myDurability != null)
            {
                myDurability.InitializeForModule(ItemCellsData, CellsOffset);
            }
            else
            {
                Debug.LogError($"�� ������ {gameObject.name} ����������� ������������ ��� ���� ������� ��������� Durability. ���� ��� �� ������, ������� ������� � ������� � ItemData.isModule");
            }
        }
    }

    void SetSize()
    {
        Image.localScale = new Vector3(Size, Size, 0);
    }

    //���� ��� ������������ ������� � ���������
    public void ChangeSize(float size)
    {
        Size = size;
        SetSize();
    }
    public void ChangeCellsShift(Vector2 shift)
    {
        CellsOffset = shift;
    }


    public Vector2 GetMinSlotsPosition()
    {
        Vector2 minPosition = new Vector2();
        for (int cell = 0; cell < ItemCellsData.Length; cell++)
        {
            Vector2 position = ItemCellsData[cell].position + CellsOffset;
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
        for (int cell = 0; cell < ItemCellsData.Length; cell++)
        {
            Vector2 position = ItemCellsData[cell].position + CellsOffset;
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
    None, //��� ������� �������

    Weapon,
    DefenceModules,
    EnergyBlocks,
    Engines,
    Drones,
    SpecialModules
}

public enum modulesTypes
{
    None, //��� ������� �������

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
public class CellData
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