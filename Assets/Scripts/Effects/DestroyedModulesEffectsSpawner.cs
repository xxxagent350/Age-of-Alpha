using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class DestroyedModulesEffectsSpawner : NetworkBehaviour
{
    [Rpc(SendTo.Everyone)]
    public void SpawnAndAttachDestroyedModuleEffectRpc(NetworkString[] effectsNamesNetworkStringArray, Vector2 localPosition)
    {
        List<string> effectsNamesList = new List<string>(0);
        foreach (NetworkString effectsNameNetworkString in effectsNamesNetworkStringArray)
        {
            effectsNamesList.Add(effectsNameNetworkString.String);
        }

        List<GameObject> effectsGOs = RpcHandlerForEffects.SpawnEffectsLocal(effectsNamesList, Vector3.zero, Quaternion.identity, Vector3.zero);
        foreach (GameObject effectGO in effectsGOs)
        {
            effectGO.transform.parent = transform;
            effectGO.transform.localPosition = localPosition;
        }
    }
}
