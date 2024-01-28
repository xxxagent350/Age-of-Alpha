using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModule : MonoBehaviour
{

    public float durability;
    float maxDurability;

    private void Awake()
    {
        maxDurability = durability;
    }

    public void TakeDamage(float damage)
    {
        durability -= damage;
        if (durability <= 0)
        {
            Destroy(gameObject);
        }
    }

}
