using System;
using UnityEngine;


public class ItemData : MonoBehaviour
{
    public string NameRu;
    public string NameEng;
    public float Size = 1;
    public int Mass;
    public Vector2 cellsOffset;
    public slotsData[] itemSlotsData;

    private void Start()
    {
        SetSize();
    }

    void SetSize()
    {
        transform.Find("Image").localScale = new Vector3(Size, Size, 0);
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