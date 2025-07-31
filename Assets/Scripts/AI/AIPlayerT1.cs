using UnityEngine;

public class AIPlayerT1 : MonoBehaviour
{
    [Header("Настройка")]
    [Tooltip("Тип поведения бота: None - бездействует, Simple - летит сломя голову на ближайшего противника и атакует")]
    public BehaviourModes behaviourMode;
    [Tooltip("Сохранённый пресет поведения. Можно оставить пустым")]
    public AIParametresT1 parametresPreset;
    [Tooltip("Текущие параметры поведения")]
    public AIParametresT1Struct currentParametres;

    [Header("Отладка")]
    [SerializeField] private ShipGameStats target;

    private void FixedUpdate()
    {
        
    }

    public enum BehaviourModes
    {
        None,
        Sipmle
    }
}
