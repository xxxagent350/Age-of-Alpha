using System.Collections.Generic;
using UnityEngine;

public class LaserWeapon : Weapon
{
    [Header("Настройка")]
    [SerializeField] private string _laserMainEffectName;
    [Tooltip("Позиция откуда будет 'вылетать' лазер (обозначена значком прицела в редакторе)")]
    [SerializeField] Vector2 _barrelPosition;
    [SerializeField] private float _damagePerSecond = 15;
    [SerializeField] private float _energyPerSecond = 10;
    [SerializeField] private float _maxLaserDistance = 15;
    [SerializeField] private float _laserEnablingTime = 0.5f;
    [Tooltip("Сколько секунд хранится информация о том, хватало ли энергии лазеру. Чем этот параметр больше, тем плавнее лазер будет подстраивать прозрачность под нехватку энергии")]
    [SerializeField] private float _laserPowerDataAmount = 1;

    [Header("Отладка")]
    [SerializeField] private List<bool> _laserPowerData;
    [SerializeField] private float _laserGlobalAlpha;
    [SerializeField] private LasersEffectsNetworkSynchronizer _lasersEffectsNetworkSynchronizer;

    [Tooltip("Модификатор прозрачности лазера, зависящий от того включается или выключается ли он")]
    private float _laserEnablingAlpha;

    public override void Initialize()
    {
        _lasersEffectsNetworkSynchronizer = GetComponentInParent<LasersEffectsNetworkSynchronizer>();
    }

    public override void FixedServerUpdate()
    {
        ManageLaserEnablingData();
        ManageLaserGlobalAlpha();
        VisualizeLazer();
    }

    private void ManageLaserEnablingData()
    {
        float alphaChangingSpeed = Time.deltaTime / _laserEnablingTime;

        if (isFiring)
        {
            if (_laserEnablingAlpha < 1)
            {
                _laserEnablingAlpha += alphaChangingSpeed;
            }
            else
            {
                _laserEnablingAlpha = 1;
            }
        }
        else
        {
            if (_laserEnablingAlpha > 0)
            {
                _laserEnablingAlpha -= alphaChangingSpeed;
            }
            else
            {
                _laserEnablingAlpha = 0;
            }
        }
    }

    private void ManageLaserGlobalAlpha()
    {
        _laserGlobalAlpha = _laserEnablingAlpha;
    }

    private void VisualizeLazer()
    {
        Vector3 localLaserPosition = transform.localPosition + (Vector3)DataOperator.RotateVector2(_barrelPosition, transform.eulerAngles.z);
        NetworkString laserEffectNetworkName = new NetworkString(_laserMainEffectName);
        _lasersEffectsNetworkSynchronizer.VisualizeLaserRpc(new LowAccuracyVector2(localLaserPosition), transform.localEulerAngles.z, laserEffectNetworkName, 10, _laserGlobalAlpha);
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
