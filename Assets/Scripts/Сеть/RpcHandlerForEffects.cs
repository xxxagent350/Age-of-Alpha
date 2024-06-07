using UnityEngine;
using Unity.Netcode;
using System;

//он нужен дл€ спавна эффектов у клиентов
public class RpcHandlerForEffects : NetworkBehaviour
{
    [Header("Ёффекты")]
    [SerializeField] EffectWithName[] effects;

    public static RpcHandlerForEffects instance;

    private void Awake()
    {
        instance = this;
    }

    public static void SpawnEffectOnClients(NetworkString effectName, Vector3 position, Quaternion rotation)
    {
        if (instance != null)
        {
            int effectsIndex = -1;
            string effectNameString = effectName.GetString();

            for (int index = 0; index < instance.effects.Length; index++)
            {
                if (instance.effects[index].effectName == effectNameString)
                {
                    effectsIndex = index;
                    break;
                }
            }

            if (effectsIndex < 0)
            {
                Debug.LogError($"Ёффект {effectNameString} не найден в массиве префаба RpcHandlerForEffects");
            }
            else
            {
                instance.SpawnEffectOnClientsRpc((uint)effectsIndex, position, rotation);
            }
        }
        else
        {
            Debug.LogError($"RpcHandlerForEffects: не возможно заспавнить эффект {effectName}, так как RpcHandlerForEffects отсутствует на сцене");
        }
    }

    public static void SpawnEffectOnClients(uint effectNumInArray, Vector3 position, Quaternion rotation)
    {
        if (instance != null)
        {
            instance.SpawnEffectOnClientsRpc(effectNumInArray, position, rotation);
        }
        else
        {
            Debug.LogError($"RpcHandlerForEffects: не возможно заспавнить эффект {effectNumInArray}, так как RpcHandlerForEffects отсутствует на сцене");
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnEffectOnClientsRpc(uint effectNumInArray, Vector3 position, Quaternion rotation)
    {
        if (IsSpawned)
        {
            if (effects.Length > effectNumInArray)
            {
                effects[effectNumInArray].effect.SpawnEffectsFromPool(position, rotation);
            }
        }
        else
        {
            Debug.LogWarning($"{gameObject}: не возможно заспавнить эффект {effectNumInArray}, так как RpcHandlerForEffects ещЄ не заспавнен");
        }
    }   

    [Serializable]
    struct EffectWithName
    {
        public string effectName;
        public Effect effect;
    }
}
