using UnityEngine;
using UnityEngine.Audio;

public class ShipChanger : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private SlotsPutter _slotsPutter;
    [SerializeField] private ModulesMenu _modulesMenu;

    [Header("Отладка")]
    [SerializeField] private uint _playerShipNum;

    private GameObject _playerShipGameObject;

    public const string playerShipNumDataName = "playerShipNum";

    private void Start()
    {
        _playerShipNum = (uint)DataOperator.instance.LoadDataInt(playerShipNumDataName);
        SetPlayerShip(_playerShipNum);
    }

    public void SetNextPlayerShip()
    {
        if (_playerShipNum < DataOperator.instance.shipsPrefabs.Length - 1)
        {
            _playerShipNum++;
        }
        else
        {
            _playerShipNum = 0;
        }
        DataOperator.instance.SaveData(playerShipNumDataName, (int)_playerShipNum);
        SetPlayerShip(_playerShipNum);
    }

    private void SetPlayerShip(uint shipNum)
    {
        if (_playerShipGameObject != null)
        {
            Destroy(_playerShipGameObject);
        }
        _playerShipGameObject = Instantiate(DataOperator.instance.shipsPrefabs[shipNum], Vector3.zero, Quaternion.identity);
        //_slotsPutter.ResetItemData();
        _slotsPutter.ItemData = _playerShipGameObject.GetComponent<ItemData>();

        ShipStats shipStats = _playerShipGameObject.GetComponent<ShipStats>();
        _modulesMenu.shipStats = shipStats;
        //shipStats.InitializeForShipBuildingScene();
        _modulesMenu.SetShipStats();
    }
}
