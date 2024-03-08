using UnityEngine;

public class ModuleData : MonoBehaviour
{
    public categories category;
    public types type;

    public enum categories
    {
        None, //��� ������� �������

        Weapons,
        DefenceModules,
        EnergyBlocks,
        Engines,
        Drones,
        SpecialModules
    }

    public enum types
    {
        None, //��� ������� �������

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
