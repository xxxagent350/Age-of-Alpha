using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Radar : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] private GameObject _shipIconPrefab;
    [SerializeField] private Color allyColor = new(0, 1, 0);
    [SerializeField] private Color enemyColor = new(1, 0, 0);

    [Header("Отладка")]
    [Tooltip("Сколько метров от игрока будет отображено на миникарте")]
    public float MapScale = 500;
    public ShipGameStats PlayerShipGameStats;
    [SerializeField] private List<ObjectOnRadar> _objectsOnRadar = new(0);
    [SerializeField] private int _numOfRegisteredObjects;

    private RectTransform _radarRectTransform;

    private const float iconAppearingSpeed = 2;

    private void Start()
    {
        _numOfRegisteredObjects = 0;
        _radarRectTransform = GetComponent<RectTransform>();
    }

    public void ClearRegisteredObjects()
    {
        foreach (ObjectOnRadar objectOnRadar in _objectsOnRadar)
        {
            Destroy(objectOnRadar.AttachedMinimapImage.gameObject);
        }
        _objectsOnRadar.Clear();
        _numOfRegisteredObjects = 0;
    }

    private void OnEnable()
    {
        StartCoroutine(LowUpdateCoroutine());
    }

    private void OnDisable()
    {
        ClearRegisteredObjects();
    }

    private void Update()
    {
        if (PlayerShipGameStats != null)
        {
            RenderIconsOnRadar();
        }
    }

    private IEnumerator LowUpdateCoroutine()
    {
        const float Frequency = 10;
        while (true)
        {
            LowUpdate();
            yield return new WaitForSeconds(1 / Frequency);
        }
    }

    private void LowUpdate()
    {
        if (PlayerShipGameStats != null)
        {
            CheckNullShips();
            ManageRegisteredShips();
            SetShipsIconsColors();
        }
    }

    private void CheckNullShips()
    {
        foreach (ObjectOnRadar objectOnRadar in _objectsOnRadar)
        {
            if (!objectOnRadar.NoLongerExists && objectOnRadar.AttachedGameObject == null)
            {
                objectOnRadar.NoLongerExists = true;
                _numOfRegisteredObjects--;
            }
        }
    }

    private void ManageRegisteredShips()
    {
        int allShipsNum = GameObjectsSearcher.GetAllShipGameObjects().Length;
        if (allShipsNum > _numOfRegisteredObjects)
        {
            List<GameObject> allShips = new(0);
            foreach (GameObject ship in GameObjectsSearcher.GetAllShipGameObjects())
            {
                allShips.Add(ship);
            }

            for (int shipNum = 0; shipNum < allShips.Count; shipNum++)
            {
                for (int objectOnRadarNum = 0; objectOnRadarNum < _objectsOnRadar.Count; objectOnRadarNum++)
                {
                    if (_objectsOnRadar[objectOnRadarNum].AttachedGameObject == allShips[shipNum])
                    {
                        allShips.Remove(allShips[shipNum]);
                        break;
                    }
                }
            }

            //теперь в allShips остались только ещё не зарегестрированные на радаре корабли, регестрируем их
            foreach (GameObject ship in allShips)
            {
                AddNewObjectOnRadar(ship, ObjectOnRadarType.ship);
            }
        }
    }

    private void AddNewObjectOnRadar(GameObject newGameObject_, ObjectOnRadarType objectOnRadarType)
    {
        if (objectOnRadarType == ObjectOnRadarType.ship)
        {
            RectTransform newIconTransform = Instantiate(_shipIconPrefab, transform).GetComponent<RectTransform>();
            ShipGameStats newShipGameStats = newGameObject_.GetComponent<ShipGameStats>();
            Image newMinimapImage = newIconTransform.GetComponent<Image>();
            newMinimapImage.color = new Color(newMinimapImage.color.r, newMinimapImage.color.g, newMinimapImage.color.b, 0);
            ObjectOnRadar newObjectOnRadar = new(objectOnRadarType, newGameObject_, newIconTransform, newShipGameStats, newMinimapImage);
            _objectsOnRadar.Add(newObjectOnRadar);
            _numOfRegisteredObjects++;
        }
    }

    private void SetShipsIconsColors()
    {
        foreach (ObjectOnRadar objectOnRadar in _objectsOnRadar)
        {
            if (!objectOnRadar.NoLongerExists && objectOnRadar.Type == ObjectOnRadarType.ship)
            {
                if (objectOnRadar.AttachedShipGameStats.TeamID == PlayerShipGameStats.TeamID)
                {
                    objectOnRadar.AttachedMinimapImage.color = new Color(allyColor.r, allyColor.g, allyColor.b, objectOnRadar.AttachedMinimapImage.color.a);
                }
                else
                {
                    objectOnRadar.AttachedMinimapImage.color = new Color(enemyColor.r, enemyColor.g, enemyColor.b, objectOnRadar.AttachedMinimapImage.color.a);
                }
            }
        }
    }

    private void RenderIconsOnRadar()
    {
        for (int objectNum = 0; objectNum < _objectsOnRadar.Count; objectNum++)
        {
            if (_objectsOnRadar[objectNum].NoLongerExists == false && _objectsOnRadar[objectNum].AttachedGameObject == null)
            {
                //объект уничтожен, но его ещё не успели пометить таковым - пока что скипаем
                continue;
            }

            //отключаем иконку игрока
            if (_objectsOnRadar[objectNum].AttachedGameObject == PlayerShipGameStats.gameObject)
            {
                _objectsOnRadar[objectNum].AttachedMinimapImage.enabled = false;
                continue;
            }

            //прозрачность
            if (_objectsOnRadar[objectNum].NoLongerExists == false && _objectsOnRadar[objectNum].AttachedShipGameStats.Destroyed.Value == false)
            {
                if (_objectsOnRadar[objectNum].AttachedMinimapImage.color.a < 1)
                {
                    Color oldColor = _objectsOnRadar[objectNum].AttachedMinimapImage.color;
                    _objectsOnRadar[objectNum].AttachedMinimapImage.color = new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a + (iconAppearingSpeed * Time.deltaTime));
                }
            }
            else
            {
                if (_objectsOnRadar[objectNum].AttachedMinimapImage.color.a > 0)
                {
                    Color oldColor = _objectsOnRadar[objectNum].AttachedMinimapImage.color;
                    _objectsOnRadar[objectNum].AttachedMinimapImage.color = new Color(oldColor.r, oldColor.g, oldColor.b, oldColor.a - (iconAppearingSpeed * Time.deltaTime));
                }
                else
                {
                    //удаление больше не нужных иконок
                    if (_objectsOnRadar[objectNum].AttachedShipGameStats == null)
                    {
                        Destroy(_objectsOnRadar[objectNum].AttachedMinimapImage.gameObject);
                        _objectsOnRadar.Remove(_objectsOnRadar[objectNum]);
                        continue;
                    }
                }
            }

            //перемещение
            if (objectNum < _objectsOnRadar.Count && _objectsOnRadar[objectNum] != null && _objectsOnRadar[objectNum].NoLongerExists == false)
            {
                Vector2 localPositionRelativeToPlayer = _objectsOnRadar[objectNum].AttachedGameObject.transform.position - PlayerShipGameStats.transform.position;
                Vector2 minimapScale = new Vector2(_radarRectTransform.rect.width / 2, _radarRectTransform.rect.height / 2);
                Vector2 relativePosOnMinimap = localPositionRelativeToPlayer / MapScale;

                //смотрим не расположен ли объект вне миникарты
                if (Mathf.Abs(relativePosOnMinimap.x) < 1 && Mathf.Abs(relativePosOnMinimap.y) < 1)
                {
                    _objectsOnRadar[objectNum].AttachedMinimapImage.enabled = true;
                }
                else
                {
                    _objectsOnRadar[objectNum].AttachedMinimapImage.enabled = false;
                }
                Vector2 anchoredPosOnMinimap = relativePosOnMinimap * minimapScale;
                _objectsOnRadar[objectNum].AttachedRadarIcon.anchoredPosition = anchoredPosOnMinimap;
            }
        }
    }
}

[Serializable]
public class ObjectOnRadar
{
    public readonly ObjectOnRadarType Type;
    public readonly GameObject AttachedGameObject;
    public readonly RectTransform AttachedRadarIcon;
    public ShipGameStats AttachedShipGameStats;
    public readonly Image AttachedMinimapImage;
    public bool NoLongerExists;

    public ObjectOnRadar(ObjectOnRadarType objectOnRadarType_, GameObject attachedShip_, RectTransform attachedRadarIcon_, ShipGameStats attachedShipGameStats_, Image attachedMinimapImage_)
    {
        Type = objectOnRadarType_;
        AttachedGameObject = attachedShip_;
        AttachedRadarIcon = attachedRadarIcon_;
        NoLongerExists = false;
        AttachedShipGameStats = attachedShipGameStats_;
        AttachedMinimapImage = attachedMinimapImage_;
    }
}

public enum ObjectOnRadarType
{
    ship,
    asteroid
}
