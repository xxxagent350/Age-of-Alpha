using UnityEngine;
using UnityEngine.UI;

public class ItemDataChanger : MonoBehaviour
{

    bool starting = true;
    Vector2 cellsOffset;
    float shipSize;
    [SerializeField] InputField nameInput;
    [SerializeField] InputField sizeInput;
    [SerializeField] InputField bodyMassInput;
    [SerializeField] InputField shiftXInput;
    [SerializeField] InputField shiftYInput;
    bool dontUpdateUI;

    private void Start()
    {
        RefreshInfo();
        starting = false;
    }

    public void RefreshInfo()
    {
        ItemData itemData = (ItemData)FindFirstObjectByType(typeof(ItemData));
        if (itemData != null)
        {
            dontUpdateUI = true;//
            shipSize = itemData.Size;
            sizeInput.text = shipSize + "";
            bodyMassInput.text = itemData.Mass + "";
            nameInput.text = itemData.Name.EnglishText;
            cellsOffset = itemData.CellsOffset;
            shiftXInput.text = cellsOffset.x + "";
            shiftYInput.text = cellsOffset.y + "";
            dontUpdateUI = false;//
            UpdateUI();
        }
        else
        {
            nameInput.text = "";
            sizeInput.text = "";
            bodyMassInput.text = "";
        }
    }

    public void ChangeName(string newName)
    {
        ItemData itemData = (ItemData)FindFirstObjectByType(typeof(ItemData));
        if (itemData != null)
        {
            itemData.name = newName;
            itemData.Name.EnglishText = newName;
        }
    }

    public void ChangeBodyMass(string newBodyMass)
    {
        ItemData itemData = (ItemData)FindFirstObjectByType(typeof(ItemData));
        if (itemData != null && newBodyMass != "")
            itemData.Mass = ConvertStringToFloat(newBodyMass);
    }

    public void ChangeSize(string newSize)
    {
        if (!starting && newSize != "")
        {
            shipSize = ConvertStringToFloat(newSize);
            UpdateUI();
        }
    }

    public void ChangeCellsOffsetX(string newOffsetX)
    {
        if (!starting)
        {
            cellsOffset = new Vector2(ConvertStringToFloat(newOffsetX), cellsOffset.y);
            UpdateUI();
        }
    }
    public void ChangeCellsOffsetY(string newOffsetY)
    {
        if (!starting)
        {
            cellsOffset = new Vector2(cellsOffset.x, ConvertStringToFloat(newOffsetY));
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        if (!dontUpdateUI)
        {
            GameObject.Find("Сетка главная").transform.position = new Vector3(cellsOffset.x + 0.5f, cellsOffset.y + 0.5f, 0);

            ItemData itemData = (ItemData)FindFirstObjectByType(typeof(ItemData));
            if (itemData != null)
            {
                itemData.ChangeCellsShift(cellsOffset);
                itemData.ChangeSize(shipSize);
            }
            SlotsPutter slotsPutter = (SlotsPutter)FindFirstObjectByType(typeof(SlotsPutter));
            slotsPutter.ReloadCellsUI();
        }
    }

    float ConvertStringToFloat(string string_)
    {
        float float_ = 0;
        if (string_ != "" && string_ != "-")
        {
            var numberParts = string_.Split("."[0]);
            if (numberParts.Length == 2)
            {
                float_ = float.Parse(numberParts[0] + "," + numberParts[1]);
            }
            else
            {
                float_ = float.Parse(string_);
            }
        }
        return float_;
    }

}
