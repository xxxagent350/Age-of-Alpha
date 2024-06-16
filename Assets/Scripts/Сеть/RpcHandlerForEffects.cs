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

    public static void SpawnEffectsOnClients(List<string> effectsNames, Vector3 position, Quaternion rotation)
    {
        foreach (string effectName in effectsNames)
        {
            SpawnEffectOnClients(effectName, position, rotation);
        }
    }

    public static void SpawnEffectOnClients(string effectName, Vector3 position, Quaternion rotation)
    {
        if (instance != null)
        {
            instance.SpawnEffectOnClientsRpc(effectName, position, rotation);
        }
        else
        {
            Debug.LogError($"RpcHandlerForEffects: не возможно заспавнить эффект {effectName}, так как RpcHandlerForEffects отсутствует на сцене");
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnEffectOnClientsRpc(string effectName, Vector3 position, Quaternion rotation)
    {
        if (IsSpawned)
        {
            Effect effect;
            if (effectsDictionary.TryGetValue(effectName, out effect))
            {
                effect.SpawnEffectsFromPool(position, rotation);
            }
            else
            {
                Debug.Log($"RpcHandlerForEffects: эффекта {effectName} не найдено в словаре");
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject}: не возможно заспавнить эффект {effectName}, так как RpcHandlerForEffects ещЄ не заспавнен");
        }
    }   

    [Serializable]
    struct EffectWithName
    {
        public string effectName;
        public Effect effect;
    }
}
