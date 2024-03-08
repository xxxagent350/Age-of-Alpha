using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class TextTranslator : MonoBehaviour
{
    [SerializeField] TranslatedText text;

    Text textUI;
    TextMeshProUGUI textMeshProUI;
    TextMeshPro textMeshPro;

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    void Initialize()
    {
        if (DataOperator.instance != null)
            Translate();
        else
            Invoke(nameof(Translate), 0.05f);
    }

    void Translate()
    {
        //получаем возможные компоненты текста
        textUI = GetComponent<Text>();
        textMeshProUI = GetComponent<TextMeshProUGUI>();
        textMeshPro = GetComponent<TextMeshPro>();

        //передаём компонентам текста перевод если они есть
        if (textUI != null)
        {
            textUI.text = text.GetTranslatedText();
        }
        if (textMeshProUI != null)
        {
            textMeshProUI.text = text.GetTranslatedText();
        }
        if (textMeshPro != null)
        {
            textMeshPro.text = text.GetTranslatedText();
        }
    }
}

[Serializable]
public class TranslatedText
{
    public string RussianText;
    public string EnglishText;

    public string GetTranslatedText()
    {
        SupportedLanguages userLanguage = DataOperator.instance.userLanguage;

        if (userLanguage == SupportedLanguages.Russian)
            return RussianText;
        if (userLanguage == SupportedLanguages.English)
            return EnglishText;

        return "Error";
    }
}

public enum SupportedLanguages
{
    Russian,
    English
}
