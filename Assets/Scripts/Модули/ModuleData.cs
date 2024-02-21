using UnityEngine;

public class ModuleData : MonoBehaviour
{
    public categories category;
    public types type;
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

    public enum categories
    {
        None, //для фильтра модулей

        Weapons,
        DefenceModules,
        EnergyBlocks,
        Engines,
        Drones,
        SpecialModules
    }

    public enum types
    {
        None, //для фильтра модулей

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
