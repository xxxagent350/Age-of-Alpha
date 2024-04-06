using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;
using System;
using System.Collections.Generic;

public class NetworkTransformIlia : NetworkBehaviour
{
    Rigidbody2D myRigidbody2D;
    NetworkObject myNetworkObject;
    [SerializeField] Transform2D gais;
    List<Transform2D> Pozition;
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
            //то что тут напишешь будет выполняться у сервера
            SetPositionRpc(new Transform2D(transform));
        }
        else
        {
            if (NetworkManager.Singleton.IsClient)
            {
                //то что тут напишешь  выполняться у клиентов
                Debug.Log($"X: {gais.xPos} Y: {gais.yPos} Поворот: {gais.rotationDegrees}");
                transform.position = new Vector2(gais.xPos, gais.yPos);
                Pozition.Add(gais);
            }
        }
    }

    //[Rpc(SendTo.NotServer)] отправляет это сообщение всем кроме сервера
    [Rpc(SendTo.NotServer)]
    void SetPositionRpc(Transform2D newTransform)
    {
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            //newTransform.SetTransformAtThis(transform);
            gais = newTransform ;
        }
    }
}