using UnityEngine;

public class EnergyBar : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] Transform energyBarFilling;

    [Header("Отладка")]
    public float fillingValue = 1;

    private void OnEnable()
    {
        Update();
    }

    private void Update()
    {
        if (fillingValue > 1)
            fillingValue = 1;
        if (fillingValue < 0)
            fillingValue = 0;

        energyBarFilling.localScale = new Vector3(fillingValue, energyBarFilling.localScale.y, 0);
    }
}
