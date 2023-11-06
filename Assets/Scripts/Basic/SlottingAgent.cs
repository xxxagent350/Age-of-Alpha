using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlottingAgent : MonoBehaviour
{
    
    public void ChangeSize(string newSize)
    {
        if (newSize != "")
        {
            var numberParts = newSize.Split("."[0]);
            if (numberParts.Length == 2)
            {
                GameObject.FindGameObjectWithTag("Ship").GetComponent<shipSellsData>().ChangeSize(float.Parse(numberParts[0] + "," + numberParts[1]));
            }
            else
            {
                GameObject.FindGameObjectWithTag("Ship").GetComponent<shipSellsData>().ChangeSize(float.Parse(newSize));
            }
            
        }
    }

    public void CellsShift(bool shift)
    {
        GameObject.FindGameObjectWithTag("Ship").GetComponent<shipSellsData>().ChangeCellsShift(shift);

        if (shift)
        {
            GameObject.Find("Сетка главная").transform.position = new Vector3(0.5f, 0.5f, 0);
        }
        else
        {
            GameObject.Find("Сетка главная").transform.position = new Vector3(0, 0, 0);
        }
    }

}
