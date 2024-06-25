using System.Collections;
using UnityEngine;

public class GameObjectsSearcher : MonoBehaviour
{
    public static GameObject[] AllModulesGameObjects { get; private set; }

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

    private IEnumerator LowUpdateCoroutine()
    {
        while (true)
        {
            LowUpdate();
            yield return new WaitForSeconds(1 / Frequency);
        }
    }

    //аналог FixedUpdate, но с пониженной частотой обновления для уменьшения нагрузки
    private void LowUpdate()
    {
        AllModulesGameObjects = GameObject.FindGameObjectsWithTag(ModulesLayerMask);
    }
}
