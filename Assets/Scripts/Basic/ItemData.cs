using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : MonoBehaviour
{

    public string Name;
    public float Size = 1;
    public int Mass;
    public Vector2 cellsOffset;
    public int[] cellsDataX;
    public int[] cellsDataY;
    public int[] cellsDataType;

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
