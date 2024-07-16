using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : Weapon
{
    [Header("Настройка")]
    [SerializeField] private string _laserMainEffectName;
    [Tooltip("Позиция откуда будет 'вылетать' лазер (обозначена значком прицела в редакторе)")]
    [SerializeField] private Vector2 _barrelPosition;
    [SerializeField] private Damage _damagePerSecond;
    [SerializeField] private float _energyPerSecond = 10;
    [SerializeField] private float _maxLaserDistance = 100;
    [SerializeField] private LayerMask _modulesLayer;
    [SerializeField] private float _laserEnablingTime = 0.5f;
    [Tooltip("Сколько секунд хранится информация о том, хватало ли энергии лазеру. Чем этот параметр больше, тем плавнее лазер будет подстраивать прозрачность под нехватку энергии")]
    [SerializeField] private float _laserPowerDataAmount = 0.5f;

    [Header("Отладка")]
    [SerializeField] private List<bool> _laserPowerData = new();
    [SerializeField] private float _laserGlobalAlpha;
    [SerializeField] private LasersEffectsNetworkSynchronizer _lasersEffectsNetworkSynchronizer;
    [SerializeField] private ShipGameStats _myShipGameStats;
    [SerializeField] private float _laserVisualDistance;

    [Tooltip("Модификатор прозрачности лазера, зависящий от того включается или выключается ли он")]
    private float _laserEnablingProgress;
    private bool _visualLaserAlreadyTurnedOff;
    private bool _lastLaserPowerData;
    private float _laserPowerAlphaMod;

    public override void Initialize()
    {
        _myShipGameStats = GetComponentInParent<ShipGameStats>();
        _lasersEffectsNetworkSynchronizer = _myShipGameStats.GetComponent<LasersEffectsNetworkSynchronizer>();
        int laserPowerDataListLength = Mathf.RoundToInt(_laserPowerDataAmount / Time.fixedDeltaTime);
        for (int laserPowerDataElementNum = 0; laserPowerDataElementNum < laserPowerDataListLength; laserPowerDataElementNum++)
        {
            _laserPowerData.Add(true);
        }
    }

    public override void RandomizedServerUpdate()
    {
        ManageLaserEnablingAlpha();
        if (_laserEnablingProgress > 0)
        {
            RaycastLaser();
            ManageLaserPowerData();
            ManageLaserPowerAlphaMod();
            ManageLaserGlobalAlpha();
            VisualizeLazer();
        }
        else
        {
            TurnOffLaserVisual();
        }
    }

    private void ManageLaserEnablingAlpha()
    {
        float enablingPowerChangingSpeed = Time.deltaTime / _laserEnablingTime;

        if (isFiring)
        {
            if (_laserEnablingProgress < 1)
            {
                _laserEnablingProgress += enablingPowerChangingSpeed;
            }
            else
            {
                _laserEnablingProgress = 1;
            }
        }
        else
        {
            if (_laserEnablingProgress > 0)
            {
                _laserEnablingProgress -= enablingPowerChangingSpeed;
            }
            else
            {
                _laserEnablingProgress = 0;
            }
        }
    }

    private void ManageLaserPowerData()
    {
        _laserPowerData.RemoveAt(0);
        _laserPowerData.Add(_lastLaserPowerData);
    }

    private void ManageLaserPowerAlphaMod()
    {
        _laserPowerAlphaMod = 0;
        foreach (bool laserPowerDataElement in _laserPowerData)
        {
            if (laserPowerDataElement == true)
            {
                _laserPowerAlphaMod += 1f / _laserPowerData.Count;
            }
        }
    }

    private void ManageLaserGlobalAlpha()
    {
        _laserGlobalAlpha = _laserEnablingProgress * _laserPowerAlphaMod;
    }

    private void RaycastLaser()
    {
        bool enoughEnergy = _myShipGameStats.TrySpendEnergy(_energyPerSecond * Time.deltaTime);
        _lastLaserPowerData = enoughEnergy;
        Vector2 origin = (Vector2)transform.position + DataOperator.RotateVector2(_barrelPosition, transform.eulerAngles.z);
        Vector2 direction = DataOperator.RotateVector2(Vector2.up, transform.eulerAngles.z);
        RaycastHit2D[] laserHitsInfos = Physics2D.RaycastAll(origin, direction, _maxLaserDistance, _modulesLayer);
        float newLaserVisualDistance = _maxLaserDistance;
        foreach (RaycastHit2D laserHitInfo in laserHitsInfos)
        {
            Durability hittedModulesDurability = laserHitInfo.collider.GetComponent<Durability>();
            if (hittedModulesDurability != null && hittedModulesDurability.TeamID != _myShipGameStats.TeamID.Value.String)
            {
                //лазер попал во вражеский модуль
                newLaserVisualDistance = laserHitInfo.distance;
                if (isFiring && enoughEnergy)
                {
                    DamageModule(hittedModulesDurability);
                }
                break;
            }
        }
        _laserVisualDistance = newLaserVisualDistance;
    }

    private void DamageModule(Durability modulesDurability)
    {
        Damage oneFrameDamage = new(_damagePerSecond.fireDamage * Time.deltaTime, _damagePerSecond.energyDamage * Time.deltaTime, _damagePerSecond.physicalDamage * Time.deltaTime);
        modulesDurability.durability.TakeDamage(oneFrameDamage);
    }

    private void VisualizeLazer()
    {
        _visualLaserAlreadyTurnedOff = false;
        Vector3 localLaserPosition = transform.localPosition + (Vector3)_barrelPosition;
        NetworkString laserEffectNetworkName = new(_laserMainEffectName);
        _lasersEffectsNetworkSynchronizer.VisualizeLaserRpc(new LowAccuracyVector2(localLaserPosition), transform.localEulerAngles.z, laserEffectNetworkName, _laserVisualDistance, _laserGlobalAlpha);
    }

    public override void OnDestroyServer()
    {
        TurnOffLaserVisual();
    }

    private void TurnOffLaserVisual()
    {
        if (!_visualLaserAlreadyTurnedOff)
        {
            _laserVisualDistance = 0;
            _laserGlobalAlpha = 0;
            VisualizeLazer();
            _visualLaserAlreadyTurnedOff = true;

            for (int powerDataElementNum = 0; powerDataElementNum < _laserPowerData.Count; powerDataElementNum++)
            {
                _laserPowerData[powerDataElementNum] = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Vector2 myPosition = transform.position;
            Gizmos.DrawIcon(myPosition + _barrelPosition, "Aim icon.png", false);
        }
    }
}
