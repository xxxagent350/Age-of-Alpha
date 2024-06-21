using UnityEngine;

public class GameSettingsKeeper : MonoBehaviour
{
    public static GameSettingsKeeper instance;

    public SupportedLanguages userLanguage;
    public GraphicsPresets userGraphics = GraphicsPresets.high;
    public HealthCellsShowPreset healthCellsShowPreset = HealthCellsShowPreset.onlyWhenZoomedIn;
    public float healthCellsShowTimeOnDurabilityChanged = 3;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (PlayerPrefs.HasKey("UserLanguage"))
        {
            SetLanguage();
        }
        else
        {
            SetSystemLanguage();
        }
    }

    void SetSystemLanguage()
    {        
        if (Application.systemLanguage == SystemLanguage.Russian || Application.systemLanguage == SystemLanguage.Belarusian)
        {
            PlayerPrefs.SetString("UserLanguage", "Russian");
        }
        else
        {
            PlayerPrefs.SetString("UserLanguage", "English");
        }
        SetLanguage();
    }

    void SetLanguage()
    {
        if (PlayerPrefs.GetString("UserLanguage") == "Russian")
            userLanguage = SupportedLanguages.Russian;
        if (PlayerPrefs.GetString("UserLanguage") == "English")
            userLanguage = SupportedLanguages.English;
    }
}

public enum SupportedLanguages
{
    Russian,
    English
}

public enum GraphicsPresets
{
    high,
    medium,
    low,
    none
}

public enum HealthCellsShowPreset
{
    onlyWhenZoomedIn,
    always,
    never
}