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
        //��������� ���� �� ������ � ���� ��� ���� ��������� ����� ���������
        PooledObject prefabsPooledObject = GOsPrefab.GetComponent<PooledObject>();
        if (prefabsPooledObject == null)
        {
            Debug.LogError($"�� ������� {GOsPrefab} ����������� ��������� PooledObject, ����������� ��� ������� �������");
            return;
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
