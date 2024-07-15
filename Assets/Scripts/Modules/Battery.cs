using UnityEngine;

public class Battery : MonoBehaviour
{
    [Header("Настройка")]
    public float maxCapacity;

    [Header("Отладка")]
    [SerializeField] float capacity;
}
