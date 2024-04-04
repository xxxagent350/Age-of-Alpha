using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformByAlphaGames : NetworkBehaviour
{
    [Header("Настройка")]
    [Tooltip("Network variable - с помощью сетевой переменной, rpc - с помощью сообщений(рекомендуется для моментальной синхронизации), interpolate - более плавная синхронизация за счёт искуственной задержки")]
    [SerializeField] NetworkVariable<TransformSyncTypes> syncType;
    [Tooltip("На сколько кадров(fixed update) будет оставать анимация от сервера (для interpolate), больше = плавнее")]
    [SerializeField] uint framesBuffering = 5;
    [Tooltip("Чем больше этот параметр, тем медленнее будет приближаться объект к последней полученной по сети позиции(должен быть более 1; нужен для interpolate)")]
    [SerializeField] float interpolatingTimeScaleMod = 1.1f;
    [Tooltip("Чем больше этот параметр, тем медленнее будет приближаться объект к последней полученной по сети позиции(должен быть более framesBuffering; нужен для interpolate)")]
    [SerializeField] uint framesMaxBuffer = 50;

    Rigidbody2D myRigidbody2D;
    NetworkObject myNetworkObject;

    private void Start()
    {
        myRigidbody2D = GetComponent<Rigidbody2D>();
        myNetworkObject = GetComponent<NetworkObject>();
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            myRigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            myRigidbody2D.simulated = false;
        }
        if (framesBuffering == 0)
        {
            framesBuffering = 1;
            Debug.LogWarning($"framesBuffering не может быть менее 1 ({gameObject.name})");
        }
        if (interpolatingTimeScaleMod < 1)
        {
            interpolatingTimeScaleMod = 1.1f;
            Debug.LogWarning($"interpolatingTimeScaleMod не может быть менее 1 ({gameObject.name})");
        }
        if (framesMaxBuffer <= framesBuffering)
        {
            framesMaxBuffer = framesBuffering + 1;
            Debug.LogWarning($"framesMaxBuffer должен быть более framesBuffering ({gameObject.name})");
        }
    }

    private void FixedUpdate()
    {
        if (myNetworkObject.IsSpawned)
        {
            if (syncType.Value == TransformSyncTypes.NetworkVariable)
            {
                NetworkVariableTransform();
            }
            if (syncType.Value == TransformSyncTypes.Rpc)
            {
                RpcTransform();
            }
            if (syncType.Value == TransformSyncTypes.Interpolate)
            {
                InterpolatingTransform();
            }
        }
    }

    readonly NetworkVariable<Transform2D> NetworkTransform2DVar = new NetworkVariable<Transform2D>();

    void NetworkVariableTransform()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkTransform2DVar.Value = new Transform2D(transform);
        }
        else
        {
            if (NetworkManager.Singleton.IsClient)
            {
                NetworkTransform2DVar.Value.SetTransformAtThis(transform);
            }
        }
    }




    void RpcTransform()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SetPositionRpc(new Transform2D(transform));
        }
    }

    [Rpc(SendTo.NotServer)]
    void SetPositionRpc(Transform2D newTransform)
    {
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            newTransform.SetTransformAtThis(transform);
        }
    }


    [Header("Отладка")]
    [SerializeField] List<Transform2D> transformsInterpolating = new List<Transform2D>(0);

    ulong serversCount;
    [SerializeField] float clientsCount;

    void InterpolatingTransform()
    {
        //удачи, Илья
        if (NetworkManager.Singleton.IsServer) //сервер
        {
            SetPositionRpcForInterpolatingRpc(new Transform2D(transform), serversCount);
            serversCount++;
        }
        else
        {
            if (NetworkManager.Singleton.IsClient) //все клиенты кроме сервера
            {
                while (transformsInterpolating.Count > framesMaxBuffer)
                {
                    transformsInterpolating.RemoveAt(0);
                }
                if (transformsInterpolating.Count > 0)
                {
                    Interpolate();
                }
            }
        }
    }

    void Interpolate()
    {
        clientsCount++;
        if (clientsCount < 0)
        {
            clientsCount = 0;
        }
        if (clientsCount > framesMaxBuffer - 1)
        {
            clientsCount = framesMaxBuffer - 1;
        }

        float targetPositionInOrder = transformsInterpolating.Count - 1 - framesBuffering;
        clientsCount += (targetPositionInOrder - clientsCount) * interpolatingTimeScaleMod;

        Transform2D finalPos = new Transform2D();
        if (clientsCount == Mathf.RoundToInt(clientsCount))
        {
            finalPos = transformsInterpolating[Mathf.RoundToInt(clientsCount)];
        }
        else
        {
            float olderRatio = Mathf.RoundToInt(clientsCount + 0.5f) - clientsCount;
            float newerRatio = clientsCount - Mathf.RoundToInt(clientsCount - 0.5f);
            Transform2D olderTransform = transformsInterpolating[Mathf.RoundToInt(clientsCount - 0.5f)];
            Transform2D newerTransform = transformsInterpolating[Mathf.RoundToInt(clientsCount + 0.5f)];
            finalPos.xPos = (olderTransform.xPos * olderRatio) + (newerTransform.xPos * newerRatio);
            finalPos.yPos = (olderTransform.yPos * olderRatio) + (newerTransform.yPos * newerRatio);
            finalPos.rotationDegrees = (olderTransform.rotationDegrees * olderRatio) + (newerTransform.rotationDegrees * newerRatio);
        }
        

        if (clientsCount < 0)
        {
            clientsCount = 0;
        }
        if (clientsCount > framesMaxBuffer - 1)
        {
            clientsCount = framesMaxBuffer - 1;
        }

        finalPos.SetTransformAtThis(transform);
    }

    [Rpc(SendTo.NotServer)]
    void SetPositionRpcForInterpolatingRpc(Transform2D newTransform_, ulong numInOrder_)
    {
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            transformsInterpolating.Add(newTransform_);
            clientsCount--;
        }
    }


    public enum TransformSyncTypes
    {
        NetworkVariable,
        Rpc,
        Interpolate
    }
}


[Serializable]
public struct Transform2D : INetworkSerializable
{
    public float xPos;
    public float yPos;
    public float rotationDegrees;

    public Transform2D(Vector2 position_, float rotationDegrees_)
    {
        xPos = position_.x;
        yPos = position_.y;
        rotationDegrees = rotationDegrees_;
    }
    public Transform2D(Transform transform_)
    {
        xPos = transform_.position.x;
        yPos = transform_.position.y;
        rotationDegrees = transform_.eulerAngles.z;
    }

    public Vector2 GetPosition()
    {
        return new Vector2(xPos, yPos);
    }
    public float GetRotationDegrees()
    {
        return rotationDegrees;
    }
    public void SetTransformAtThis(Transform transform_)
    {
        transform_.SetPositionAndRotation(new Vector3(xPos, yPos, transform_.position.z), Quaternion.Euler(transform_.eulerAngles.x, transform_.eulerAngles.y, rotationDegrees));
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref xPos);
        serializer.SerializeValue(ref yPos);
        serializer.SerializeValue(ref rotationDegrees);
    }
}