using UnityEngine;

public class EnergyBar : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] Transform energyBarFilling;
    [SerializeField] float smoothness = 10f;

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

        float mod = smoothness * Time.deltaTime;
        float nextFillingValue = energyBarFilling.localScale.x + ((fillingValue - energyBarFilling.localScale.x) * mod);
        if (nextFillingValue < 0)
        {
            nextFillingValue = 0;
        }
        if (nextFillingValue > 1)
        {
            nextFillingValue = 1;
        }
        energyBarFilling.localScale = new Vector3(nextFillingValue, energyBarFilling.localScale.y, 0);
    }
}
