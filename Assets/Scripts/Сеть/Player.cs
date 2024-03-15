using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Настройка")]
    [SerializeField] GameObject playerShip;

    [Header("Отладка")]
    [SerializeField] string teamID;
    [SerializeField] ulong ownerClientID;

    NetworkObject myNetworkObject;

    private void Start()
    {
        myNetworkObject = GetComponent<NetworkObject>();
        ownerClientID = myNetworkObject.OwnerClientId;
        if (IsServer)
        {
            SpawnPlayerShip();
        }
    }

    public void SpawnPlayerShip()
    {
        GameObject playerShipSpawning = Instantiate(playerShip, new Vector3(0, 0, 0), Quaternion.identity);
        playerShipSpawning.GetComponent<NetworkObject>().SpawnWithOwnership(ownerClientID);
    }
}
