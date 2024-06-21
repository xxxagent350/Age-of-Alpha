using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using Unity.Netcode;

public class TextTranslator : NetworkBehaviour
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
            textUI.text = text.GetTranslatedString();
        }
        if (textMeshProUI != null)
        {
            textMeshProUI.text = text.GetTranslatedString();
        }
        if (textMeshPro != null)
        {
            textMeshPro.text = text.GetTranslatedString();
        }
    }
}

[Serializable]
public struct TranslatedText
{
    public string RussianText;
    public string EnglishText;

    public string GetTranslatedString()
    {
        SupportedLanguages userLanguage = GameSettingsKeeper.instance.userLanguage;

        if (userLanguage == SupportedLanguages.Russian)
            return RussianText;
        if (userLanguage == SupportedLanguages.English)
            return EnglishText;

        return "Error";
    }
}

[Serializable]
public struct TranslatedNetworkText : INetworkSerializable
{
    NetworkString RussianText;
    NetworkString EnglishText;

    public TranslatedNetworkText(TranslatedText translatedText)
    {
        RussianText = new NetworkString(translatedText.RussianText);
        EnglishText = new NetworkString(translatedText.EnglishText);
    }

    public TranslatedText GetTranslatedText()
    {
        TranslatedText translatedText = new TranslatedText();
        translatedText.RussianText = RussianText.GetString();
        translatedText.EnglishText = EnglishText.GetString();
        return translatedText;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        RussianText.NetworkSerialize(serializer);
        EnglishText.NetworkSerialize(serializer);
    }
}
