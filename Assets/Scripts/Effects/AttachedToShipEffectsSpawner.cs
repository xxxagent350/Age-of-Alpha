using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class AttachedToShipEffectsSpawner : NetworkBehaviour
{
    public void SpawnAndAttachEffects(List<string> effectsNames, Vector2 localPosition, Quaternion localRotation)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkString[] effectsNamesNetworkStringArray = new NetworkString[effectsNames.Count];
            for (int effectNameNum = 0; effectNameNum < effectsNames.Count; effectNameNum++)
            {
                effectsNamesNetworkStringArray[effectNameNum] = new NetworkString(effectsNames[effectNameNum]);
            }
            SpawnAndAttachEffectsRpc(effectsNamesNetworkStringArray, localPosition, localRotation);
        }
        else
        {
            Debug.LogError($"{this}: SpawnAndAttachDestroyedModuleEffects(List<string> effectsNames, Vector2 localPosition) можно вызывать только на сервере!");
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SpawnAndAttachEffectsRpc(NetworkString[] effectsNamesNetworkStringArray, Vector2 localPosition, Quaternion localRotation)
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
            effectGO.transform.localRotation = localRotation;
        }
    }
}
