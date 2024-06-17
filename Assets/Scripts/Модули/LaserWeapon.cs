using System.Collections;
using UnityEngine;

public class LaserWeapon : Weapon
{
    [SerializeField] private GameObject _laser;
    [SerializeField] private SpriteRenderer _laserSprite;
    [SerializeField] private float _laserWorkTime;

    private void Start()
    {
        _laserSprite = _laser.GetComponent<SpriteRenderer>();
    }
    public override void FixedServerUpdate()
    {

    }

    public void Shoot()
    {
        StartCoroutine(ShowLaser());
    }

    private IEnumerator ShowLaser()
    {
        Color OldColor = _laserSprite.color;

        while (_laserSprite.color.a < 1f)
        {
            _laserSprite.color = new Color(OldColor.r, OldColor.g, OldColor.b, _laserSprite.color.a + 0.1f);
            yield return new WaitForSeconds(_laserWorkTime / 60);
        }

        while (_laserSprite.color.a > 0f)
        {
            _laserSprite.color = new Color(OldColor.r, OldColor.g, OldColor.b, _laserSprite.color.a - 0.1f);
            yield return new WaitForSeconds(_laserWorkTime / 60);
        }
    }
}
