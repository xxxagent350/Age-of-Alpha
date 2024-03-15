using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Random = UnityEngine.Random;
using Unity.Netcode;

public class DataOperator : MonoBehaviour
{
    [Header("Настройка")]
    public GameObject[] modulesPrefabs;
    [SerializeField] AudioSource UIAudioSource;
    [SerializeField] GameObject defaultAudioSourcePrefab;
    [SerializeField] GameObject DataPathAccessErrorScreen;

    [Header("Отладка")]
    [SerializeField] Data[] gameData;
    public SupportedLanguages userLanguage;
    public GraphicsPresets userGraphics = GraphicsPresets.high;

    Camera camera_;
    [HideInInspector] public static DataOperator instance = null;
    BinaryFormatter binaryFormatter;
    string dataPath;
    string deviceType;
    [HideInInspector] public float cameraSize;

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

        if (PlayerPrefs.GetString("UserLanguage") == "Russian")
            userLanguage = SupportedLanguages.Russian;
        if (PlayerPrefs.GetString("UserLanguage") == "English")
            userLanguage = SupportedLanguages.English;

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

    private void Update()
    {
        if (TryFoundCamera())
        {
            cameraSize = camera_.orthographicSize;
        }
    }

    public Camera GetCamera()
    {
        TryFoundCamera();
        return camera_;
    }

    bool TryFoundCamera()
    {
        if (camera_ == null)
        {
            camera_ = (Camera)FindFirstObjectByType(typeof(Camera));
            if (camera_ == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return true;
        }
    }


    public GameObject GetRandomGameObject(GameObject[] gameObjects)
    {
        int GONum = Random.Range(0, gameObjects.Length);
        return gameObjects[GONum];
    }
    public void PlayRandom3DSound(Vector3 position, AudioClip[] audioClips)
    {
        int audioClipNum = Random.Range(0, audioClips.Length);
        GameObject soundGO = Instantiate(defaultAudioSourcePrefab, position, Quaternion.identity);
        AudioSource soundCMP = soundGO.GetComponent<AudioSource>();
        soundCMP.clip = audioClips[audioClipNum];
        soundCMP.Play();
        Destroy(soundGO, audioClips[audioClipNum].length + 0.05f);
    }
    public void PlayRandom3DSound(Vector3 position, AudioClip[] audioClips, float volume)
    {
        int audioClipNum = Random.Range(0, audioClips.Length);
        GameObject soundGO = Instantiate(defaultAudioSourcePrefab, position, Quaternion.identity);
        AudioSource soundCMP = soundGO.GetComponent<AudioSource>();
        soundCMP.clip = audioClips[audioClipNum];
        soundCMP.volume = volume;
        soundCMP.Play();
        Destroy(soundGO, audioClips[audioClipNum].length + 0.05f);
    }
    public void PlayRandom3DSound(Vector3 position, AudioClip[] audioClips, float volume, float minDistance)
    {
        int audioClipNum = Random.Range(0, audioClips.Length);
        GameObject soundGO = Instantiate(defaultAudioSourcePrefab, position, Quaternion.identity);
        AudioSource soundCMP = soundGO.GetComponent<AudioSource>();
        soundCMP.clip = audioClips[audioClipNum];
        soundCMP.volume = volume;
        soundCMP.minDistance = minDistance;
        soundCMP.Play();
        Destroy(soundGO, audioClips[audioClipNum].length + 0.05f);
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
            Debug.LogWarning("Попытка воспроизвести не заданный UI звук через DataOperator");
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
    public void SaveData(ModulesOnStorageData dataModulesOnStorage)
    {
        Module moduleSaving = dataModulesOnStorage.module;
        string fileName = GetDataNameForModule(moduleSaving);
        FileStream file = File.Create(dataPath + fileName + ".data");
        Data data = new Data(fileName, dataModulesOnStorage);
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

    public ModulesOnStorageData LoadDataModulesOnStorage(Module module_)
    {
        ModulesOnStorageData data = null;
        string dataName = GetDataNameForModule(module_);

        foreach (Data searchingData in gameData)
        {
            if (searchingData.dataName == dataName)
            {
                data = searchingData.dataModulesOnStorage;
                break;
            }
        }
        if (data == null)
        {
            data = new ModulesOnStorageData(module_, 0);
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

    public void CreateGameObjects(GameObject[] gameObjects, Vector2 position, Quaternion rotation)
    {
        foreach (GameObject go in gameObjects)
        {
            Instantiate(go, position, rotation);
        }
    }

    public string GetDataNameForModule(Module module)
    {
        string dataName = "DataModulesOnStorage(" + module.moduleNum + ")";
        for (int upgrade = 0; upgrade < Enum.GetNames(typeof(ModuleUpgradesTypes)).Length; upgrade++)
        {
            for (int moduleUpgrade = 0; moduleUpgrade < module.moduleUpgrades.Length; moduleUpgrade++)
            {
                if ((int)module.moduleUpgrades[moduleUpgrade].upgradeType == upgrade)
                {
                    dataName += "_u(" + (int)module.moduleUpgrades[moduleUpgrade].upgradeType + ";";
                    dataName += module.moduleUpgrades[moduleUpgrade].upgradeMod + ")";
                    break;
                }
            }
        }
        return dataName;
    }

    public ModulesOnStorageData[] GetModulesOnStorageDataClonedArray()
    {
        ModulesOnStorageData[] modulesOnStorageDataCloning = new ModulesOnStorageData[0];
        for (int dataNum = 0; dataNum < gameData.Length; dataNum++)
        {
            if (gameData[dataNum].dataModulesOnStorage != null && gameData[dataNum].dataModulesOnStorage.amount > 0)
            {
                Array.Resize(ref modulesOnStorageDataCloning, modulesOnStorageDataCloning.Length + 1);
                modulesOnStorageDataCloning[modulesOnStorageDataCloning.Length - 1] = (ModulesOnStorageData)gameData[dataNum].dataModulesOnStorage.Clone();
            }
        }
        return modulesOnStorageDataCloning;
    }

    public float RoundFloat(float input)
    {
        return RoundFloat(input, 2);
    }

    public float RoundFloat(float input, uint decimalPoints)
    {
        float operatingValue = input * Mathf.Pow(10, decimalPoints);
        operatingValue = Mathf.RoundToInt(operatingValue);
        operatingValue /= Mathf.Pow(10, decimalPoints);
        return operatingValue;
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

[Serializable]
public class Limits
{
    public float minValue;
    public float maxValue;
    float constantRandomValue;
    bool constantRandomValueGenerated = false;

    //каждый раз выдаёт случайное число в заданных лимитах
    public float GetRandomValue()
    {
        return Random.Range(minValue, maxValue);
    }

    //один раз генерирует случайное число в заданных лимитах и потом всё время выдаёт его
    public float GetConstantRandomValue()
    {
        if (!constantRandomValueGenerated)
        {
            constantRandomValueGenerated = true;
            constantRandomValue = Random.Range(minValue, maxValue);
        }
        return constantRandomValue;
    }
}


[Serializable]
public struct Vector2Serializable
{
    public float x;
    public float y;

    public Vector2Serializable(Vector2 vector2ToConvert)
    {
        x = vector2ToConvert.x;
        y = vector2ToConvert.y;
    }
    public Vector2 GetVector2()
    {
        return new Vector2(x, y);
    }
}


public enum GraphicsPresets
{
    high,
    medium,
    low,
    none
}

[Serializable]
public class Effect
{
    public GameObject[] HighQualityEffects;
    public GameObject[] MediumQualityEffects;
    public GameObject[] LowQualityEffects;

    GraphicsPresets graphicsPreset = GraphicsPresets.none;

    public void SpawnEffects(Vector2 position, Quaternion rotation)
    {
        if (graphicsPreset == GraphicsPresets.none)
        {
            graphicsPreset = DataOperator.instance.userGraphics;
        }
        if (graphicsPreset == GraphicsPresets.low)
        {
            DataOperator.instance.CreateGameObjects(LowQualityEffects, position, rotation);
        }
        if (graphicsPreset == GraphicsPresets.medium)
        {
            DataOperator.instance.CreateGameObjects(MediumQualityEffects, position, rotation);
        }
        if (graphicsPreset == GraphicsPresets.high)
        {
            DataOperator.instance.CreateGameObjects(HighQualityEffects, position, rotation);
        }
    }
}