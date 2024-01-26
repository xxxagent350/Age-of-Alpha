using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipStats : MonoBehaviour
{

    [HideInInspector] public string teamID;

    [SerializeField] float modulesCollidersKeepActiveTime = 3;
    float modulesCollidersKeepActiveTimer;
    bool modulesCollidersActive = true;
    public bool spawnBullet;//
    public GameObject bullet;//

    float radiansAngle;

    private void Awake()
    {
        teamID = Random.Range(1000000000, 2000000000) + "" + Random.Range(1000000000, 2000000000) + "" + Random.Range(1000000000, 2000000000);
        ModulesCollidersSetActive(false);
    }

    private void FixedUpdate()
    {
        radiansAngle = (transform.eulerAngles.z + 90) * Mathf.Deg2Rad;
        modulesCollidersKeepActiveTimer += Time.deltaTime;
        if (modulesCollidersKeepActiveTimer >= modulesCollidersKeepActiveTime && modulesCollidersActive == true)
        {
            ModulesCollidersSetActive(false);
        }
        if (spawnBullet)//
        {
            spawnBullet = false;
            GameObject bulletPrepared = Instantiate(bullet, transform.position, transform.rotation);
            Bullet bulletComponent = bulletPrepared.GetComponent<Bullet>();
            bulletComponent.teamID = teamID;
            bulletComponent.CustomStart();
            bulletPrepared.GetComponent<Rigidbody2D>().velocity = new Vector2(Mathf.Cos(radiansAngle), Mathf.Sin(radiansAngle)) * bulletComponent.speed;
        }
    }

    public void TakeDamage(float damage)
    {
        modulesCollidersKeepActiveTimer = 0;
        if (!modulesCollidersActive)
        {
            ModulesCollidersSetActive(true);
        }
    }

    void ModulesCollidersSetActive(bool state)
    {
        modulesCollidersActive = state;
        BoxCollider2D[] modulesColliders = GetComponentsInChildren<BoxCollider2D>();
        foreach (BoxCollider2D collider in modulesColliders)
        {
            collider.enabled = state;
        }
    }

}
