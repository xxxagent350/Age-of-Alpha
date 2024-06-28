using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ram Effects Data", menuName = "Scriptable Objects/Ram Effects Data")]
public class RamEffectsData : ScriptableObject
{
    //set
    [SerializeField] private List<string> _touchEffects;
    [SerializeField] private List<string> _lowDamageEffects;
    [SerializeField] private List<string> _mediumDamageEffects;
    [SerializeField] private List<string> _highDamageEffects;

    //get
    public List<string> TouchEffects => _touchEffects;
    public List<string> LowDamageEffects => _lowDamageEffects;
    public List<string> MediumDamageEffects => _mediumDamageEffects;
    public List<string> HighDamageEffects => _highDamageEffects;
}
