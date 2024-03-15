using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Настройка")]
    [SerializeField] GameObject playerShip;

    [Header("Отладка")]
    [SerializeField] ModuleOnShipData[] modulesOnPlayerShipData;
    [SerializeField] ulong ownerClientID;
    [SerializeField] NetworkVariable<float> teamID = new NetworkVariable<float>();

    NetworkObject myNetworkObject;

    private void Start()
    {
        myNetworkObject = GetComponent<NetworkObject>();
        ownerClientID = myNetworkObject.OwnerClientId;
        if (IsServer)
        {
            teamID.Value = Mathf.Round(Random.Range(Mathf.Pow(10, 30), Mathf.Pow(10, 30) * 2));
        }
        if (IsOwner)
        {

        }
    }

    public void SpawnPlayerShip()
    {
        GameObject playerShipSpawning = Instantiate(playerShip, new Vector3(0, 0, 0), Quaternion.identity);
        playerShipSpawning.GetComponent<NetworkObject>().SpawnWithOwnership(ownerClientID);
    }

    [Rpc(SendTo.Server)]
    void SendPlayerShipDataToServerRpc(int shipNum)
    {

    }
}