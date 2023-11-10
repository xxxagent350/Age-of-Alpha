using UnityEngine;
using UnityEngine.UI;

public class SlottingAgent : MonoBehaviour
{

    bool starting = true;
    bool shift;
    float shipSize;
    [SerializeField] InputField sizeInput;
    [SerializeField] Toggle shiftInput;
    [SerializeField] GameObject shiftOff;

    private void Start()
    {
        shift = GameObject.FindGameObjectWithTag("Ship").GetComponent<shipSellsData>().cellsShift;
        shiftInput.isOn = shift;
        shipSize = GameObject.FindGameObjectWithTag("Ship").GetComponent<shipSellsData>().shipSize;
        sizeInput.text = shipSize + "";
        UpdateUI();
        starting = false;
    }

    public void ChangeSize(string newSize)
    {
        if (!starting && newSize != "")
        {
            var numberParts = newSize.Split("."[0]);
            if (numberParts.Length == 2)
            {
                shipSize = float.Parse(numberParts[0] + "," + numberParts[1]);
            }
            else
            {
                shipSize = float.Parse(newSize);
            }
            UpdateUI();
        }
    }

    public void CellsShift(bool shiftChange)
    {
        if (!starting)
        {
            GetComponent<AudioSource>().Play();
            shift = shiftChange;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (shift)
        {
            shiftOff.SetActive(false);
            GameObject.Find("Сетка главная").transform.position = new Vector3(0.5f, 0.5f, 0);
        }
        else
        {
            Invoke(nameof(SetShiftOn), 0.12f);
            GameObject.Find("Сетка главная").transform.position = new Vector3(0, 0, 0);
        }

        GameObject.FindGameObjectWithTag("Ship").GetComponent<shipSellsData>().ChangeCellsShift(shift);
        GameObject.FindGameObjectWithTag("Ship").GetComponent<shipSellsData>().ChangeSize(shipSize);

    }

    void SetShiftOn()
    {
        shiftOff.SetActive(true);
    }

}
