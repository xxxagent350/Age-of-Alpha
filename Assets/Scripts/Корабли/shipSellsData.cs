using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shipSellsData : MonoBehaviour
{

    public float shipSize = 1;
    public bool cellsShift;
    public int[] cellsDataX;
    public int[] cellsDataY;
    public int[] cellsDataType;

    private void Start()
    {
        SetSize();
        SetShift();
    }

    void SetSize()
    {
        transform.Find("Image").localScale = new Vector3(shipSize, shipSize, 0);
    }

    void SetShift()
    {
        if (cellsShift)
        {
            transform.Find("Cells").transform.localPosition = new Vector3(0.5f, 0.5f, 0);
        }
        else
        {
            transform.Find("Cells").transform.localPosition = new Vector3(0, 0, 0);
        }
    }





    //ниже для конфигурации корабля в редакторе

    public void ChangeSize(float size)
    {
        shipSize = size;
        SetSize();
    }

    public void ChangeCellsShift(bool shift)
    {
        cellsShift = shift;
        SetShift();
    }


}
