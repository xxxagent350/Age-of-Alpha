using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class DataOperator : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] AudioSource UIAudioSource;

    [Header("Отладка")]
    public Data[] gameData;
    public string userLanguage;
    [SerializeField] GameObject DataPathAccessErrorScreen;

    public static DataOperator instance = null;
    BinaryFormatter binaryFormatter;
    string dataPath;
    string deviceType;

    private void Awake()
    {
        Application.targetFrameRate = 90;


        if (instance == null)
        {
            instance = this;
            binaryFormatter = new BinaryFormatter();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        if (!PlayerPrefs.HasKey("UserLanguage"))
        {
            if (Application.systemLanguage == SystemLanguage.Russian || Application.systemLanguage == SystemLanguage.Belarusian)
            {
                PlayerPrefs.SetString("UserLanguage", "Russian");
            }
            else
            {
                PlayerPrefs.SetString("UserLanguage", "English");
            }
        }
        userLanguage = PlayerPrefs.GetString("UserLanguage");


        dataPath = Application.persistentDataPath + "/Data/";
        if (Application.platform == RuntimePlatform.Android)
        {
            deviceType = "android";
        }
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            deviceType = "windows";
        }
        if (deviceType == "android")
        {
            dataPath = "/data/data/com.AlphaGames.TheEraofAlpha/gameData";
            try
            {
                Directory.CreateDirectory("/data/data/com.AlphaGames.TheEraofAlpha/testFolder");
                Directory.Delete("/data/data/com.AlphaGames.TheEraofAlpha/testFolder");
            }
            catch
            {
                DataPathAccessErrorScreen.SetActive(true);
            }
        }
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        else
        {
            foreach (string dataFilePath in Directory.GetFiles(dataPath))
            {
                FileStream file = File.Open(dataFilePath, FileMode.Open);
                Data data = (Data)binaryFormatter.Deserialize(file);

                Array.Resize(ref gameData, gameData.Length + 1);
                gameData[gameData.Length - 1] = data;
            }
        }
    }

    public void PlayUISound(AudioClip sound, float volume)
    {
        if (sound != null)
        {
            UIAudioSource.clip = sound;
            UIAudioSource.volume = volume;
            UIAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Попытка воспроизвести не заданный UI звук");
        }
    }

    public void SaveData(string name_, string dataString)
    {
        FileStream file = File.Create(dataPath + name_ + ".data");
        Data data = new Data(name_, dataString);
        binaryFormatter.Serialize(file, data);
        file.Close();
        AddDataToArray(data);
    }
    public void SaveData(string name_, int dataInt)
    {
        FileStream file = File.Create(dataPath + name_ + ".data");
        Data data = new Data(name_, dataInt);
        binaryFormatter.Serialize(file, data);
        file.Close();
        AddDataToArray(data);
    }
    public void SaveData(string name_, float dataFloat)
    {
        FileStream file = File.Create(dataPath + name_ + ".data");
        Data data = new Data(name_, dataFloat);
        binaryFormatter.Serialize(file, data);
        file.Close();
        AddDataToArray(data);
    }
    public void SaveData(string name_, ModulesOnStorageData dataModulesOnStorage)
    {
        FileStream file = File.Create(dataPath + name_ + ".data");
        Data data = new Data(name_, dataModulesOnStorage);
        binaryFormatter.Serialize(file, data);
        file.Close();
        AddDataToArray(data);
    }
    public void SaveData(string name_, ModuleOnShipData[] dataModulesOnShip)
    {
        FileStream file = File.Create(dataPath + name_ + ".data");
        Data data = new Data(name_, dataModulesOnShip);
        binaryFormatter.Serialize(file, data);
        file.Close();
        AddDataToArray(data);
    }
    void AddDataToArray(Data data)
    {
        for (int dataNum = 0; dataNum < gameData.Length; dataNum++)
        {
            if (gameData[dataNum].dataName == data.dataName)
            {
                gameData[dataNum] = data;
                return;
            }
        }
        Array.Resize(ref gameData, gameData.Length + 1);
        gameData[gameData.Length - 1] = data;
    }
    


    public string LoadDataString(string name_)
    {
        string data = "";
        foreach (Data searchingData in gameData)
        {
            if (searchingData.dataName == name_)
            {
                data = searchingData.dataString;
                break;
            }
        }
        return data;
    }

    public int LoadDataInt(string name_)
    {
        int data = 0;
        foreach (Data searchingData in gameData)
        {
            if (searchingData.dataName == name_)
            {
                data = searchingData.dataInt;
                break;
            }
        }
        return data;
    }

    public float LoadDataFloat(string name_)
    {
        float data = 0;
        foreach (Data searchingData in gameData)
        {
            if (searchingData.dataName == name_)
            {
                data = searchingData.dataFloat;
                break;
            }
        }
        return data;
    }

    public ModulesOnStorageData LoadDataModulesOnStorage(string name_)
    {
        ModulesOnStorageData data = null;
        foreach (Data searchingData in gameData)
        {
            if (searchingData.dataName == name_)
            {
                data = searchingData.dataModulesOnStorage;
                break;
            }
        }
        return data;
    }
    public ModuleOnShipData[] LoadDataModulesOnShip(string name_)
    {
        ModuleOnShipData[] data = null;
        foreach (Data searchingData in gameData)
        {
            if (searchingData.dataName == name_)
            {
                data = searchingData.dataModulesOnShip;
                break;
            }
        }
        return data;
    }



    void CopyDirectory(string folderName, string copyFrom, string copyTo)
    {
        string copyToPath = copyTo + "/" + folderName;
        string copyFromPath = copyFrom + "/" + folderName;
        if (!Directory.Exists(copyToPath))
        {
            Directory.CreateDirectory(copyToPath);
        }
        else
        {
            DeleteFilesInDirectory(copyToPath);
        }
        string[] copingFilesNames = Directory.GetFiles(copyFromPath);
        for (int file = 0; file < copingFilesNames.Length; file++)
        {
            File.Copy(copingFilesNames[0], copyToPath + "/" + Path.GetFileName(copingFilesNames[0]));
        }
        foreach (string directory in Directory.GetDirectories(copyFromPath))
        {
            CopyDirectory(new DirectoryInfo(directory).Name, new DirectoryInfo(directory).Parent + "", copyToPath);
        }
    }

    void DeleteFilesInDirectory(string directoryPath)
    {
        foreach (string file in Directory.GetFiles(directoryPath))
        {
            File.Delete(file);
        }
        foreach (string directory in Directory.GetDirectories(directoryPath))
        {
            DeleteFilesInDirectory(directory);
            Directory.Delete(directory);
        }
    }
}

[Serializable]
public class Data
{
    public string dataName;
    public string dataString;
    public int dataInt;
    public float dataFloat;
    public ModulesOnStorageData dataModulesOnStorage;
    public ModuleOnShipData[] dataModulesOnShip;

    [HideInInspector] public string deviceUniqueIdentifier;

    void SetDefaultDataValues()
    {
        dataString = "";
        dataInt = 0;
        dataFloat = 0;
        dataModulesOnStorage = null;
        dataModulesOnShip = null;
    }

    public Data(string dataName_, string dataString_)
    {
        SetDefaultDataValues();
        dataName = dataName_;
        dataString = dataString_;
        deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
    }
    public Data(string dataName_, int dataInt_)
    {
        SetDefaultDataValues();
        dataName = dataName_;
        dataInt = dataInt_;
        deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
    }
    public Data(string dataName_, float dataFloat_)
    {
        SetDefaultDataValues();
        dataName = dataName_;
        dataFloat = dataFloat_;
        deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
    }
    public Data(string dataName_, ModulesOnStorageData dataModulesOnStorage_)
    {
        SetDefaultDataValues();
        dataName = dataName_;
        dataModulesOnStorage = dataModulesOnStorage_;
        deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
    }
    public Data(string dataName_, ModuleOnShipData[] dataModulesOnShip_)
    {
        SetDefaultDataValues();
        dataName = dataName_;
        dataModulesOnShip = dataModulesOnShip_;
        deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
    }
}