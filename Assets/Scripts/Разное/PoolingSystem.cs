using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class PoolingSystem : MonoBehaviour
{
    [Header("Отладка")]
    [SerializeField] List<SameTypeObjectsList> allPooledObjects;

    public static PoolingSystem instance;

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
        }
    }

    //GO - сокращение от GameObject
    public void ReturnGOToPool(GameObject GO)
    {
        //закидываем объект в пул
        PooledObject pooledObject = GO.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            Debug.LogError($"На объекте {GO} отсутствует компонент PooledObject, необходимый для пулинга объекта");
            return;
        }
        string goID = pooledObject.pooledObjID;
        if (GetRegisteredObjectsListOfGOsID(goID) == null) //проверяем существует ли список объектов такого типа
        {
            RegisterNewObjectsType(goID);
        }
        foreach (SameTypeObjectsList registeredObjectsType in allPooledObjects)
        {
            if (goID == registeredObjectsType.pooledObjectsID)
            {
                registeredObjectsType.pooledSameTypeObjects.Add(GO);
                break;
            }
        }

        //деактивируем объект и переводим в "спящий режим"
        PooledBehaviour[] pooledBehaviours = GO.GetComponents<PooledBehaviour>();
        foreach (PooledBehaviour pooledBehaviour_ in pooledBehaviours)
        {
            pooledBehaviour_.OnReturnedToPool();
        }
        GO.SetActive(false);

        DontDestroyOnLoad(GO);
        GO.transform.SetParent(transform);
    }

    public void SpawnGOs(GameObject[] GOsPrefabs, Vector3 position, Quaternion rotation)
    {
        foreach (GameObject GOPrefab in GOsPrefabs)
        {
            SpawnGO(GOPrefab, position, rotation);
        }
    }

    public void SpawnGO(GameObject GOsPrefab, Vector3 position, Quaternion rotation)
    {
        //проверяем есть ли объект в пуле или надо создавать новый экземпляр
        PooledObject prefabsPooledObject = GOsPrefab.GetComponent<PooledObject>();
        if (prefabsPooledObject == null)
        {
            Debug.LogError($"На объекте {GOsPrefab} отсутствует компонент PooledObject, необходимый для пулинга объекта");
            return;
        }
        string goID = prefabsPooledObject.pooledObjID;
        SameTypeObjectsList sameTypeObjectsList = GetRegisteredObjectsListOfGOsID(goID);
        if (sameTypeObjectsList != null && sameTypeObjectsList.pooledSameTypeObjects.Count > 0)
        {
            GameObject pooledGO = sameTypeObjectsList.pooledSameTypeObjects[sameTypeObjectsList.pooledSameTypeObjects.Count - 1];

            //переносим объект на нужную сцену
            pooledGO.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(pooledGO, SceneManager.GetActiveScene());
            pooledGO.transform.position = position;
            pooledGO.transform.rotation = rotation;
            sameTypeObjectsList.pooledSameTypeObjects.RemoveAt(sameTypeObjectsList.pooledSameTypeObjects.Count - 1);

            //активируем объект
            pooledGO.SetActive(true);

            PooledBehaviour[] pooledBehaviours = pooledGO.GetComponents<PooledBehaviour>();
            foreach (PooledBehaviour pooledBehaviour_ in pooledBehaviours)
            {
                pooledBehaviour_.OnSpawnedFromPool();
            }
        }
        else
        {
            GameObject newGO = Instantiate(GOsPrefab, position, rotation);

            PooledBehaviour[] pooledBehaviours = newGO.GetComponents<PooledBehaviour>();
            foreach (PooledBehaviour pooledBehaviour_ in pooledBehaviours)
            {
                pooledBehaviour_.OnSpawnedFromPool();
            }
        }
    }

    SameTypeObjectsList GetRegisteredObjectsListOfGOsID(string ID)
    {
        foreach (SameTypeObjectsList registeredObjectsType in allPooledObjects)
        {
            if (registeredObjectsType.pooledObjectsID == ID)
            {
                return registeredObjectsType;
            }
        }
        return null;
    }

    void RegisterNewObjectsType(string ID)
    {
        SameTypeObjectsList newSameTypeObjectsList = new SameTypeObjectsList();
        newSameTypeObjectsList.pooledObjectsID = ID;
        allPooledObjects.Add(newSameTypeObjectsList);
    }
}

[Serializable]
public class SameTypeObjectsList
{
    public string pooledObjectsID;
    public List<GameObject> pooledSameTypeObjects;

    public SameTypeObjectsList()
    {
        pooledSameTypeObjects = new List<GameObject>(0);
    }
}
