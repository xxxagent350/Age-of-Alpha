using UnityEngine;

public class ModuleData : MonoBehaviour
{
    [SerializeField] categories category;
    [SerializeField] types type;
    [SerializeField] float maxDurability;
    float durability;

    private void Start()
    {
        durability = maxDurability;
    }

    public void TakeDamage(float damage)
    {
        durability -= damage;
        if (durability <= 0)
        {
            Destroy(gameObject);
        }
    }

    enum categories
    {
        Weapons,
        DefenceModules,
        EnergyBlocks,
        Engines,
        Drones,
        SpecialModules
    }

    enum types
    {
        None,
        //Weapons
        Ballistics,
        Lasers,
        Rockets,
        Special,
        //Defence
        Armour,
        EnergyShields,
        //EnergyBlocks
        Generators,
        Batteries,
        //Engines
        AccelerationEngines,
        RotationEngines,
        //Drones
        //SpecialModules
        ControlModules
    }
}
