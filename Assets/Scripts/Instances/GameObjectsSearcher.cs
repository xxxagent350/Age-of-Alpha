using System.Collections;
using UnityEngine;

public class GameObjectsSearcher : MonoBehaviour
{
    private static GameObject[] _allModulesGameObjects = new GameObject[0];
    private static GameObject[] _allShipsGameObjects = new GameObject[0];
    private static GameObjectsSearcher _instance;
    private const string ModulesLayerMask = "Module";
    private const string ShipsLayerMask = "Ship";

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(LowUpdateCoroutine());
    }

    public static GameObject[] GetAllModulesGameObjects()
    {

        foreach (GameObject gameObject in _allModulesGameObjects)
        {
            if (gameObject == null)
            {
                SearchModulesGameObjects();
                break;
            }
        }
        return _allModulesGameObjects;
    }

    public static GameObject[] GetAllShipGameObjects()
    {
        foreach (GameObject gameObject in _allShipsGameObjects)
        {
            if (gameObject == null)
            {
                SearchShipsGameObjects();
                break;
            }
        }
        return _allShipsGameObjects;
    }

    private IEnumerator LowUpdateCoroutine()
    {
        const float Frequency = 10;
        while (true)
        {
            SearchModulesGameObjects();
            SearchShipsGameObjects();
            yield return new WaitForSeconds(1 / Frequency);
        }
    }

    //аналог FixedUpdate, но с пониженной частотой обновления для уменьшения нагрузки
    private static void SearchModulesGameObjects()
    {
        _allModulesGameObjects = GameObject.FindGameObjectsWithTag(ModulesLayerMask);
    }

    private static void SearchShipsGameObjects()
    {
        _allShipsGameObjects = GameObject.FindGameObjectsWithTag(ShipsLayerMask);
    }
}
