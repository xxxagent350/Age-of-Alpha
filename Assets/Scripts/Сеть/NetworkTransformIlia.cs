using Unity.Netcode;
using UnityEngine;

public class NetworkTransformIlia : NetworkBehaviour
{
    Rigidbody2D myRigidbody2D;
    NetworkObject myNetworkObject;
    [SerializeField] Transform2D gais;

    private void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myNetworkObject = GetComponent<NetworkObject>();
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            myRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            myRigidbody2D.simulated = false;
        }
    }

    private void FixedUpdate()
    {
        if (myNetworkObject.IsSpawned)
        {
            IliaTransform();
        }
    }

    void IliaTransform()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            //�� ��� ��� �������� ����� ����������� � �������
            SetPositionRpc(new Transform2D(transform));
        }
        else
        {
            if (NetworkManager.Singleton.IsClient)
            {
                //�� ��� ��� ��������  ����������� � ��������
                Debug.Log($"X: {gais.xPos} Y: {gais.yPos} �������: {gais.rotationDegrees}");
            }
        }
    }

    //[Rpc(SendTo.NotServer)] ���������� ��� ��������� ���� ����� �������
    [Rpc(SendTo.NotServer)]
    void SetPositionRpc(Transform2D newTransform)
    {
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            newTransform.SetTransformAtThis(transform);
            gais = newTransform ;
        }
    }
}