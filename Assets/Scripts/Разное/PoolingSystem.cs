using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;

public class PoolingSystem : MonoBehaviour
{
    [Header("�������")]
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

    //GO - ���������� �� GameObject
    public void ReturnGOToPool(GameObject GO)
    {
        //���������� ������ � ���
        PooledObject pooledObject = GO.GetComponent<PooledObject>();
        if (pooledObject == null)
        {
            Debug.LogError($"�� ������� {GO} ����������� ��������� PooledObject, ����������� ��� ������� �������");
            return;
        }
        string goID = pooledObject.pooledObjID;
        if (GetRegisteredObjectsListOfGOsID(goID) == null) //��������� ���������� �� ������ �������� ������ ����
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

        //������������ ������ � ��������� � "������ �����"
        PooledBehaviour[] pooledBehaviours = GO.GetComponents<PooledBehaviour>();
        foreach (PooledBehaviour pooledBehaviour_ in pooledBehaviours)
        {
            pooledBehaviour_.OnReturnedToPool();
        }
        GO.SetActive(false);

        GO.transform.parent = null;
        DontDestroyOnLoad(GO);
        GO.transform.SetParent(transform);
    }

    public List<GameObject> SpawnGOs(List<GameObject> GOsPrefabs, Vector3 position, Quaternion rotation)
    {
        List<GameObject> toReturn = new List<GameObject>(0);
        foreach (GameObject GOPrefab in GOsPrefabs)
        {
            toReturn.Add(SpawnGO(GOPrefab, position, rotation));
        }
        return toReturn;
    }

    public GameObject SpawnGO(GameObject GOsPrefab, Vector3 position, Quaternion rotation)
    {
        //��������� ���� �� ������ � ���� ��� ���� ��������� ����� ���������
        PooledObject prefabsPooledObject = GOsPrefab.GetComponent<PooledObject>();
        if (prefabsPooledObject == null)
        {
            Debug.LogError($"�� ������� {GOsPrefab} ����������� ��������� PooledObject, ����������� ��� ������� �������");
            return null;
        }
        string goID = prefabsPooledObject.pooledObjID;
        SameTypeObjectsList sameTypeObjectsList = GetRegisteredObjectsListOfGOsID(goID);
        if (sameTypeObjectsList != null && sameTypeObjectsList.pooledSameTypeObjects.Count > 0)
        {
            GameObject pooledGO = sameTypeObjectsList.pooledSameTypeObjects[sameTypeObjectsList.pooledSameTypeObjects.Count - 1];

            //��������� ������ �� ������ �����
            pooledGO.transform.SetParent(null);
            SceneManager.MoveGameObjectToScene(pooledGO, SceneManager.GetActiveScene());
            pooledGO.transform.position = position;
            pooledGO.transform.rotation = rotation;
            sameTypeObjectsList.pooledSameTypeObjects.RemoveAt(sameTypeObjectsList.pooledSameTypeObjects.Count - 1);

            //���������� ������
            pooledGO.SetActive(true);

            PooledBehaviour[] pooledBehaviours = pooledGO.GetComponents<PooledBehaviour>();
            foreach (PooledBehaviour pooledBehaviour_ in pooledBehaviours)
            {
                pooledBehaviour_.OnSpawnedFromPool();
            }
            return pooledGO;
        }
        else
        {
            GameObject newGO = Instantiate(GOsPrefab, position, rotation);

            PooledBehaviour[] pooledBehaviours = newGO.GetComponents<PooledBehaviour>();
            foreach (PooledBehaviour pooledBehaviour_ in pooledBehaviours)
            {
                pooledBehaviour_.Initialize();
                pooledBehaviour_.OnSpawnedFromPool();
            }

            return newGO;
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
