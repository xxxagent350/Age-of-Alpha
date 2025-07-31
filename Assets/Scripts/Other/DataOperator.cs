
using UnityEngine;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Random = UnityEngine.Random;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class DataOperator : MonoBehaviour
{
    [Header("Настройка")]
    [SerializeField] int targetFrameRate_ = 60;
    public GameObject[] shipsPrefabs;
    public GameObject[] modulesPrefabs;
    [SerializeField] AudioSource UIAudioSource;
    [SerializeField] GameObject defaultAudioSourcePrefab;
    [SerializeField] GameObject DataPathAccessErrorScreen;

    [Header("Отладка")]
    [SerializeField] Data[] gameData;
    public static bool gameScene;

    Camera camera_;
    [HideInInspector] public static DataOperator instance = null;
    BinaryFormatter binaryFormatter;
    string dataPath;
    string deviceType;
    [HideInInspector] public float cameraSize;

    private void Awake()
    {
        Application.targetFrameRate = targetFrameRate_;
        SceneManager.activeSceneChanged += ChangedActiveScene;
        ChangedActiveScene(new Scene(), new Scene());

        if (instance == null)
        {
            instance = this;
            binaryFormatter = new BinaryFormatter();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        

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
            //dataPath = "/data/data/com.AlphaGames.TheEraofAlpha/gameData";
        }

        try
        {
            Directory.CreateDirectory(dataPath + "/testFolder");
            Directory.Delete(dataPath + "/testFolder");
        }
        catch
        {
            DataPathAccessErrorScreen.SetActive(true);
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
                file.Close();

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
        //Debug.Log(SceneManager.sceneCount);
    }

    private void ChangedActiveScene(Scene current, Scene next)
    {
        if (ShipInterfaceManager.Instance != null)
        {
            gameScene = true;
        }
        else
        {
            gameScene = false;
        }
    }

    public static void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
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
                if (searchingData.deviceUniqueIdentifier != SystemInfo.deviceUniqueIdentifier)
                {
                    return "";
                }
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
                if (searchingData.deviceUniqueIdentifier != SystemInfo.deviceUniqueIdentifier)
                {
                    return 0;
                }
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
                if (searchingData.deviceUniqueIdentifier != SystemInfo.deviceUniqueIdentifier)
                {
                    return 0;
                }
                data = searchingData.dataFloat;
                break;
            }
        }
        return data;
    }

    public ModulesOnStorageData LoadDataModulesOnStorage(Module module_)
    {
        ModulesOnStorageData data = new ModulesOnStorageData();
        string dataName = GetDataNameForModule(module_);

        foreach (Data searchingData in gameData)
        {
            if (searchingData.dataName == dataName)
            {
                if (searchingData.deviceUniqueIdentifier != SystemInfo.deviceUniqueIdentifier)
                {
                    return new ModulesOnStorageData();
                }
                data = searchingData.dataModulesOnStorage;
                break;
            }
        }
        if (data.amount == 0)
        {
            data = new ModulesOnStorageData(module_, 0);
        }
        return data;
    }

    public ModuleOnShipData[] LoadDataModulesOnShip(string name_)
    {
        ModuleOnShipData[] data = new ModuleOnShipData[0];
        foreach (Data searchingData in gameData)
        {
            if (searchingData.dataName == name_)
            {
                if (searchingData.deviceUniqueIdentifier != SystemInfo.deviceUniqueIdentifier)
                {
                    return new ModuleOnShipData[0];
                }
                data = searchingData.dataModulesOnShip;
                break;
            }
        }
        if (data == null)
        {
            data = new ModuleOnShipData[0];
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

    public List<GameObject> CreateGameObjects(List<GameObject> gameObjects, Vector2 position, Quaternion rotation)
    {
        List<GameObject> toReturn = new List<GameObject>(0);
        foreach (GameObject go in gameObjects)
        {
            toReturn.Add(Instantiate(go, position, rotation));
        }
        return toReturn;
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
            if (gameData[dataNum].dataModulesOnStorage.amount > 0)
            {
                Array.Resize(ref modulesOnStorageDataCloning, modulesOnStorageDataCloning.Length + 1);
                modulesOnStorageDataCloning[modulesOnStorageDataCloning.Length - 1] = gameData[dataNum].dataModulesOnStorage;
            }
        }
        return modulesOnStorageDataCloning;
    }

    public static float RoundFloat(float input)
    {
        return RoundFloat(input, 2);
    }

    public static float RoundFloat(float input, uint decimalPoints)
    {
        float operatingValue = input * Mathf.Pow(10, decimalPoints);
        operatingValue = Mathf.RoundToInt(operatingValue);
        operatingValue /= Mathf.Pow(10, decimalPoints);
        return operatingValue;
    }

    public static float GetVector2DirInDegrees(Vector2 vector)
    {
        if (vector == null)
        {
            return 0;
        }
        float x = vector.x;
        float y = vector.y;
        float Rad2Deg = Mathf.Rad2Deg;

        if (x > 0 && y > 0)
        {
            return -(MathF.Atan(x / y) * Rad2Deg);
        }
        if (x < 0 && y > 0)
        {
            return -(MathF.Atan(x / y) * Rad2Deg);
        }
        if (x < 0 && y < 0)
        {
            return -(MathF.Atan(x / y) * Rad2Deg - 180);
        }
        if (x > 0 && y < 0)
        {
            return -(MathF.Atan(x / y) * Rad2Deg + 180);
        }
        if (x == 0 && y >= 0)
        {
            return 0;
        }
        if (x == 0 && y < 0)
        {
            return -180;
        }
        if (x > 0 && y == 0)
        {
            return -90;
        }
        if (x < 0 && y == 0)
        {
            return 90;
        }

        return 0;
    }

    public static Vector2 RotateVector2(Vector2 vectorInput, float dirInDegrees)
    {
        Vector3 oldVector = new Vector3(vectorInput.x, vectorInput.y, 0);
        float magnitude = oldVector.magnitude;
        float dirInRadians = (dirInDegrees + 90) * Mathf.Deg2Rad;
        float oldVectorDirRadians = GetVector2DirInDegrees(vectorInput) * Mathf.Deg2Rad;

        Vector2 newVector = new Vector2(magnitude * Mathf.Cos(dirInRadians + oldVectorDirRadians), magnitude * Mathf.Sin(dirInRadians + oldVectorDirRadians));
        //Debug.Log($"dirInRadians: {dirInRadians} Sin: {Mathf.Sin(dirInRadians + oldVectorDirRadians)} Cos: {Mathf.Cos(dirInRadians + oldVectorDirRadians)}");
        return newVector;
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
        dataModulesOnStorage = new ModulesOnStorageData();
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


/*
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
*/

[Serializable]
public struct Vector2Serializable : INetworkSerializable
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref x);
        serializer.SerializeValue(ref y);
    }
}




[Serializable]
public class Effect
{
    public List<GameObject> HighQualityEffects;
    public List<GameObject> MediumQualityEffects;
    public List<GameObject> LowQualityEffects;

    GraphicsPresets graphicsPreset = GraphicsPresets.none;

    public List<GameObject> SpawnEffects(Vector3 position, Quaternion rotation)
    {
        if (graphicsPreset == GraphicsPresets.none)
        {
            graphicsPreset = GameSettingsKeeper.instance.userGraphics;
        }
        if (graphicsPreset == GraphicsPresets.low)
        {
            return DataOperator.instance.CreateGameObjects(LowQualityEffects, position, rotation);
        }
        if (graphicsPreset == GraphicsPresets.medium)
        {
            return DataOperator.instance.CreateGameObjects(MediumQualityEffects, position, rotation);
        }
        if (graphicsPreset == GraphicsPresets.high)
        {
            return DataOperator.instance.CreateGameObjects(HighQualityEffects, position, rotation);
        }
        return null;
    }

    public List<GameObject> SpawnEffectsFromPool(Vector3 position, Quaternion rotation)
    {
        if (graphicsPreset == GraphicsPresets.none)
        {
            graphicsPreset = GameSettingsKeeper.instance.userGraphics;
        }
        if (graphicsPreset == GraphicsPresets.low)
        {
            return PoolingSystem.Instance.SpawnGOs(LowQualityEffects, position, rotation);
        }
        if (graphicsPreset == GraphicsPresets.medium)
        {
            return PoolingSystem.Instance.SpawnGOs(MediumQualityEffects, position, rotation);
        }
        if (graphicsPreset == GraphicsPresets.high)
        {
            return PoolingSystem.Instance.SpawnGOs(HighQualityEffects, position, rotation);
        }
        return null;
    }
}