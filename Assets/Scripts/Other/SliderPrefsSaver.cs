using UnityEngine;
using UnityEngine.UI;

public class SliderPrefsSaver : MonoBehaviour
{
    [SerializeField] string prefsName;
    float lastValue;
    Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        if (!PlayerPrefs.HasKey(prefsName))
        {
            PlayerPrefs.SetFloat(prefsName, slider.value);
        }
        else
        {
            slider.value = PlayerPrefs.GetFloat(prefsName);
        }
        lastValue = slider.value;
    }

    private void FixedUpdate()
    {
        if (lastValue != slider.value)
        {
            lastValue = slider.value;
            PlayerPrefs.SetFloat(prefsName, slider.value);
        }
    }
}
