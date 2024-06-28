using System.Collections;
using UnityEngine;

public class GameObjectsSearcher : MonoBehaviour
{
    private static GameObject[] _allModulesGameObjects;
    private static GameObjectsSearcher _instance;
    private const float Frequency = 10;
    private const string ModulesLayerMask = "Module";

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
                LowUpdate();
                break;
            }
        }
        return _allModulesGameObjects;
    }

    private IEnumerator LowUpdateCoroutine()
    {
        while (true)
        {
            LowUpdate();
            yield return new WaitForSeconds(1 / Frequency);
        }
    }

    //аналог FixedUpdate, но с пониженной частотой обновления для уменьшения нагрузки
    private static void LowUpdate()
    {
        _allModulesGameObjects = GameObject.FindGameObjectsWithTag(ModulesLayerMask);
    }
}
