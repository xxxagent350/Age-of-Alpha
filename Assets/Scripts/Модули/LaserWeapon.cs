using System.Collections;
using UnityEngine;

public class LaserWeapon : Weapon
{
    [SerializeField] private GameObject _laser;
    [SerializeField] private SpriteRenderer _laserSprite;
    [SerializeField] private float _laserWorkTime;
    [SerializeField] private float _maxLaserRange;
    [SerializeField] private float _energyPerSecond;
    [SerializeField] private float _maxTemperature;
    [SerializeField] private float _heatPerSecond;
    [SerializeField] private float _coldPerSecond;

    [SerializeField] private float _currentTemperature = 0;

    public override void Initialize()
    {
        base.Initialize();
        _laserSprite = _laser.GetComponent<SpriteRenderer>();
        StartCoroutine(ShowLaser());
    }

    public override void FixedServerUpdate()
    {
        if (isWorking)
        {
            ControlTemperature();
            Reload();
        }
    }

    private void ControlTemperature()
    {
        if (FIRE && IsOverHeated() == false)
            _currentTemperature += _heatPerSecond * Time.deltaTime;

        if(_currentTemperature >= 0)
            _currentTemperature -= _coldPerSecond * Time.deltaTime;
    }

    private bool IsOverHeated() => _currentTemperature >= _maxTemperature;

    public override void RandomizedServerUpdate()
    {

    }

    public void Shoot()
    {
        StartCoroutine(ShowLaser());
    }

    private IEnumerator ShowLaser()
    {
        Color OldColor = _laserSprite.color;

        //while (_laserSprite.color.a < 1f)
        //{
        //    _laserSprite.color = new Color(OldColor.r, OldColor.g, OldColor.b, _laserSprite.color.a + 0.1f);
        //    yield return new WaitForSeconds(_laserWorkTime / 60);
        //}

        //yield return new WaitForSeconds(2);

        //while (_laserSprite.color.a > 0f)
        //{
        //    _laserSprite.color = new Color(OldColor.r, OldColor.g, OldColor.b, _laserSprite.color.a - 0.1f);
        //    yield return new WaitForSeconds(_laserWorkTime / 60);
        //}

        //_laserSprite.color = new Color(OldColor.r, OldColor.g, OldColor.b, 0f);

        if (FIRE && IsOverHeated() == false)
        {
            if (_laserSprite.color.a < 1)
            {
                _laserSprite.color = new Color(OldColor.r, OldColor.g, OldColor.b, _laserSprite.color.a + 0.1f);
            }
        }
        else
        {
            if (_laserSprite.color.a > 0)
            {
                _laserSprite.color = new Color(OldColor.r, OldColor.g, OldColor.b, _laserSprite.color.a - 0.1f);
            }
        }
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        StartCoroutine(ShowLaser());
    }
}
