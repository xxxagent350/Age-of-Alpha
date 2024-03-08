using UnityEngine;
using TMPro;

public class ModuleInstallationErrorMessage : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] float showedTime; //время отображения текста
    [SerializeField] float blinkingTime; //время моргания текста
    [SerializeField] float blinkingInterval; //интервал моргания текста
    [SerializeField] AudioClip errorSound; //звук при отображении текста
    [SerializeField] float errorSoundVolume;

    [Header("Отладка")]
    [SerializeField] float showedTimer = 0; //таймер отображения текста
    [SerializeField] float blinkingIntervalTimer = 0; //таймер моргания текста
    [SerializeField] bool showedByBlinking = true; //показан ли текст из-за моргания
    [SerializeField] TextMeshProUGUI textComponent;

    private void Start()
    {
        showedTimer = showedTime + 1;
        textComponent = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        showedTimer += Time.deltaTime;
        blinkingIntervalTimer += Time.deltaTime;

        if (showedTimer < showedTime) //текст активен
        {
            if (showedTimer < blinkingTime || !showedByBlinking) //текст моргает
            {
                if (showedByBlinking) //текст активен
                {
                    textComponent.enabled = true;
                    if (blinkingIntervalTimer > blinkingInterval)
                    {
                        blinkingIntervalTimer = 0;
                        showedByBlinking = false;
                    }
                }
                else //текст не активен
                {
                    textComponent.enabled = false;
                    if (blinkingIntervalTimer > blinkingInterval)
                    {
                        blinkingIntervalTimer = 0;
                        showedByBlinking = true;
                    }
                }
            }
            else //текст не моргает
            {
                textComponent.enabled = true;
            }
        }
        else //текст не активен
        {
            textComponent.enabled = false;
        }
    }

    public void ShowErrorMessage(string message)
    {
        textComponent.text = message;
        showedTimer = 0;
        blinkingIntervalTimer = 0;
        showedByBlinking = true;
        if (errorSound != null)
        {
            DataOperator.instance.PlayUISound(errorSound, errorSoundVolume);
        }
    }
}
