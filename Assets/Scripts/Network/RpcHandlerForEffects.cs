using UnityEngine;
using Unity.Netcode;
using System;
using System.Collections.Generic;

//он нужен дл€ спавна эффектов у клиентов
public class RpcHandlerForEffects : NetworkBehaviour
{
    [Header("Ёффекты")]
    [SerializeField] List<EffectWithName> effectsList;

    public static RpcHandlerForEffects instance;
    private Dictionary<string, Effect> effectsDictionary = new Dictionary<string, Effect>();

    private void Awake()
    {
        instance = this;
        for (int effectNum = 0; effectNum < effectsList.Count; effectNum++)
        {
            effectsDictionary.Add(effectsList[effectNum].effectName, effectsList[effectNum].effect);
        }
    }

    public static void SpawnEffectsOnClients(List<string> effectsNames, Vector3 position, Quaternion rotation, Vector3 speed)
    {
        foreach (string effectName in effectsNames)
        {
            SpawnEffectOnClients(effectName, position, rotation, speed);
        }
    }

    public static void SpawnEffectOnClients(string effectName, Vector3 position, Quaternion rotation, Vector3 speed)
    {
        if (instance != null)
        {
            instance.SpawnEffectOnClientsRpc(effectName, position, rotation, speed);
        }
        else
        {
            Debug.LogError($"RpcHandlerForEffects: не возможно заспавнить эффект {effectName}, так как RpcHandlerForEffects отсутствует на сцене");
        }
    }

    public static List<GameObject> SpawnEffectLocal(string effectName, Vector3 position, Quaternion rotation, Vector3 speed)
    {
        if (instance != null)
        {
            return instance.SpawnEffect(effectName, position, rotation, speed);
        }
        else
        {
            Debug.LogError($"RpcHandlerForEffects: не возможно заспавнить эффект {effectName}, так как RpcHandlerForEffects отсутствует на сцене");
            return null;
        }
    }

    public static List<GameObject> SpawnEffectsLocal(List<string> effectsNames, Vector3 position, Quaternion rotation, Vector3 speed)
    {
        List<GameObject> effectsSpawnedGOs = new List<GameObject>(0);
        foreach (string effectName in effectsNames)
        {
            if (instance != null)
            {
                effectsSpawnedGOs.AddRange(instance.SpawnEffect(effectName, position, rotation, speed));
            }
            else
            {
                Debug.LogError($"RpcHandlerForEffects: не возможно заспавнить эффект {effectName}, так как RpcHandlerForEffects отсутствует на сцене");
            }
        }
        return effectsSpawnedGOs;
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnEffectOnClientsRpc(string effectName, Vector3 position, Quaternion rotation, Vector3 speed)
    {
        SpawnEffect(effectName, position, rotation, speed);
    }

    public List<GameObject> SpawnEffect(string effectName, Vector3 position, Quaternion rotation, Vector3 speed)
    {
        if (IsSpawned)
        {
            Effect effect;
            if (effectsDictionary.TryGetValue(effectName, out effect))
            {
                List<GameObject> effectGOs = effect.SpawnEffectsFromPool(position, rotation);
                if (speed != Vector3.zero)
                {
                    foreach (GameObject effectGO in effectGOs)
                    {
                        effectGO.GetComponent<PooledEffect>().speed = speed;
                    }
                }
                return effectGOs;
            }
            else
            {
                Debug.LogError($"RpcHandlerForEffects: эффекта {effectName} не найдено в словаре");
                return null;
            }
        }
        else
        {
            Debug.LogError($"{gameObject}: не возможно заспавнить эффект {effectName}, так как RpcHandlerForEffects ещЄ не заспавнен");
            return null;
        }
    }

    [Serializable]
    struct EffectWithName
    {
        public string effectName;
        public Effect effect;
    }
}
