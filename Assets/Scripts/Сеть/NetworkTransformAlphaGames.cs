using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformByAlphaGames : NetworkBehaviour
{
    [Header("Ќастройка")]
    [Tooltip("Network variable - с помощью сетевой переменной, rpc - с помощью сообщений(рекомендуетс€ дл€ моментальной синхронизации), interpolate - более плавна€ синхронизаци€ за счЄт искуственной задержки")]
    [SerializeField] NetworkVariable<TransformSyncTypes> syncType;
    [Tooltip("Ќа сколько кадров(fixed update) будет оставать анимаци€ от сервера (дл€ interpolate), больше = плавнее")]
    [SerializeField] uint framesBuffering = 5;
    [Tooltip("„ем больше этот параметр, тем более агрессивно клиент будет измен€ть скорость течени€ времени чтобы выровн€ть своЄ врем€ и врем€ сервера(должен быть более 0; нужен дл€ interpolate; 1 = линейное изменение)")]
    [SerializeField] float interpolatingTimeScaleMod = 1;
    [Tooltip("ћаксимальна€ длина списка кадров дл€ интерпол€ции")]
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
        if (interpolatingTimeScaleMod < 0)
        {
            interpolatingTimeScaleMod = 1f;
            Debug.LogWarning($"interpolatingTimeScaleMod не может быть менее 0 ({gameObject.name})");
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


    [Header("ќтладка")]
    [SerializeField] List<Transform2D> transformsInterpolating = new List<Transform2D>(0);
    [SerializeField] float clientsCount;

    void InterpolatingTransform()
    {
        if (NetworkManager.Singleton.IsServer) //сервер
        {
            SetPositionRpcForInterpolatingRpc(new Transform2D(transform));
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
        float targetPositionInOrder = transformsInterpolating.Count - 1 - framesBuffering;
        if (clientsCount >= targetPositionInOrder)
        {
            clientsCount += Mathf.Pow((transformsInterpolating.Count - 1 - clientsCount) / framesBuffering, interpolatingTimeScaleMod);
        }
        else
        {
            clientsCount += Mathf.Pow(1 + ((targetPositionInOrder - clientsCount) / framesBuffering), interpolatingTimeScaleMod);
        }
       

        if (clientsCount < 0)
        {
            clientsCount = 0;
        }
        if (clientsCount > transformsInterpolating.Count - 1)
        {
            clientsCount = transformsInterpolating.Count - 1;
        }

        Transform2D finalPos = new Transform2D();
        if (clientsCount == Mathf.RoundToInt(clientsCount))
        {
            finalPos = transformsInterpolating[Mathf.RoundToInt(clientsCount)];
        }
        else
        {
            float olderRatio = Mathf.RoundToInt(clientsCount + 0.5f) - clientsCount;
            float newerRatio = 1 - olderRatio;
            Transform2D olderTransform = transformsInterpolating[Mathf.RoundToInt(clientsCount - 0.5f)];
            Transform2D newerTransform = transformsInterpolating[Mathf.RoundToInt(clientsCount + 0.5f)];
            finalPos.xPos = (olderTransform.xPos * olderRatio) + (newerTransform.xPos * newerRatio);
            finalPos.yPos = (olderTransform.yPos * olderRatio) + (newerTransform.yPos * newerRatio);
            finalPos.rotationDegrees = olderTransform.rotationDegrees + (Mathf.DeltaAngle(olderTransform.rotationDegrees, newerTransform.rotationDegrees) * newerRatio);
        }

        finalPos.SetTransformAtThis(transform);
    }


    bool interpolationInitialized;
    long initializingFrameNum;
    [Rpc(SendTo.NotServer)]
    void SetPositionRpcForInterpolatingRpc(Transform2D newTransform_)
    {
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.IsClient)
        {
            if (!interpolationInitialized)
            {
                for (int transform2DNum = 0; transform2DNum < framesMaxBuffer; transform2DNum++)
                {
                    transformsInterpolating.Add(newTransform_);
                }
                clientsCount = transformsInterpolating.Count - 1;
                interpolationInitialized = true;
                initializingFrameNum = transformsInterpolating.Count - 1 - framesBuffering;
            }
            //if (initializingFrameNum == )
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