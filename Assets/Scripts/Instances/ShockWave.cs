using System.Collections.Generic;
using UnityEngine;

public class ShockWave : MonoBehaviour
{
    private const float MinDamage = 0.25f;
    private const float MaxDistanceToDamage = 100;

    //вызывать строго на сервере/хосте
    public static void CreateShockWave(float epicenterDamage, Vector3 shockWavePosition)
    {
        //на расстоянии от 0 до 1 метра наносится урон epicenterDamage, от 1 до maxDistance урон плавно спадает до _minDamage
        float maxDistance = Mathf.Pow(epicenterDamage / MinDamage, 0.5f);

        List<GameObject> modulesToDamage = new(0);
        List<float> distancesToModules = new(0);
        foreach (GameObject moduleGameObject in GameObjectsSearcher.AllModulesGameObjects)
        {
            float distanceToModule = Vector3.Distance(shockWavePosition, moduleGameObject.transform.position);
            if (distanceToModule <= maxDistance)
            {
                Durability durabilityComponent = moduleGameObject.GetComponent<Durability>();
                if (durabilityComponent != null && durabilityComponent.AlreadyExploded == false)
                {
                    modulesToDamage.Add(moduleGameObject);
                    distancesToModules.Add(distanceToModule);
                }
            }
        }

        while (modulesToDamage.Count > 0)
        {
            float minFoundedDistance = maxDistance;
            int moduleNumToDamage = 0;

            for (int moduleNum = 0; moduleNum < distancesToModules.Count; moduleNum++)
            {
                if (distancesToModules[moduleNum] < minFoundedDistance)
                {
                    minFoundedDistance = distancesToModules[moduleNum];
                    moduleNumToDamage = moduleNum;
                }
            }

            DamageModule(modulesToDamage[moduleNumToDamage], epicenterDamage, shockWavePosition);
            
            modulesToDamage.RemoveAt(moduleNumToDamage);
            distancesToModules.RemoveAt(moduleNumToDamage);
        }
    }
    
    static void DamageModule(GameObject module, float epicenterDamage, Vector3 shockWavePosition)
    {
        RaycastHit2D raycastHit = Physics2D.Raycast(shockWavePosition, module.transform.position - shockWavePosition, MaxDistanceToDamage, LayerMask.GetMask("Module"));
        if (raycastHit.collider != null && raycastHit.collider.gameObject == module)
        {
            Durability durabilityComponent = module.GetComponent<Durability>();
            if (durabilityComponent != null)
            {
                float distanceForDamageCalculating = raycastHit.distance;
                if (distanceForDamageCalculating < 1)
                {
                    distanceForDamageCalculating = 1;
                }
                float fullDamage = epicenterDamage * Mathf.Pow(distanceForDamageCalculating, -2);
                durabilityComponent.durability.TakeDamage(new Damage(fullDamage * 0.3f, 0, fullDamage * 0.7f));
            }
        }
    }
}