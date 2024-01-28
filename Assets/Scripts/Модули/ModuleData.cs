using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleData : MonoBehaviour
{
    enum categories
    {
        Weapons,
        DefenceModules,
        EnergyBlocks,
        Engines,
        Drones,
        SpecialModules
    }

    [SerializeField]
    enum types
    {
        //Weapons
        Ballistics,

        //Defence
        //EnergyBlocks
        //Engines
        //Drones
        //SpecialModules
    }

    [SerializeField] categories category;
    [SerializeField] types type;
}
